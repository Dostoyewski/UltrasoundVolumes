import math
import os
from math import sin, cos, atan2

import cv2
import numpy as np
import tqdm

DEBUG = True


def extract_dist(json):
    try:
        return float(json['dist'])
    except KeyError:
        return 0


class Image(object):
    def __init__(self, path, angle, img=None):
        self.path = path
        self.angle = angle
        if img is None:
            self.image = cv2.imread(self.path, 0)
        else:
            self.image = img
        self.offset = self.image.shape[0] / 2
        self.dists = None
        self.shape = self.image.shape

    def get_dists(self, i, j, k):
        dists = []
        for z in range(len(self.image[k])):
            x, y = self.get_real_coords(z)
            dist = ((x - i) ** 2 + (y - j) ** 2) ** 0.5
            dists.append({"dist": dist,
                          "index": z,
                          "path": self.path,
                          "angle": self.angle})
        dists.sort(key=extract_dist)
        self.dists = dists
        return self.dists

    def __add__(self, other):
        return Image(".", (self.angle + other.angle) * 0.5, img=np.add(self.image, other.image) * 0.5)

    def get_real_coords(self, i):
        return cos(self.angle) * (i - self.offset), sin(self.angle) * (i - self.offset)


class Model(object):
    def __init__(self, path, angle_file):
        self.a_file = angle_file
        self.angles = None
        self.path = path
        self.names = None
        self.load_angles()
        self.get_filenames()
        self.coefs = None
        max_x, max_y = 0, 0
        for name in self.names:
            img = cv2.imread(self.names[1])
            if max_x < img.shape[0]:
                max_x = img.shape[0]
            if max_y < img.shape[1]:
                max_y = img.shape[1]
        self.shape = (max_x, max_y)
        self.a = np.full((self.shape[1], int(self.shape[0]), int(self.shape[0])), -1)
        self.images = []
        self.construct_images()
        self.poly = construct_interp_poly([imag.image for imag in self.images[0:3]],
                                          [float(ang) for ang in self.angles[0:3]])

    def construct_images(self):
        for i in range(len(self.angles)):
            if DEBUG:
                self.images.append(Image(self.names[i], float(self.angles[i])))

    def transform_img(self):
        for i in tqdm.tqdm(range(len(self.images))):
            try:
                n_img = self.images[i] + self.images[i + 1]
            except IndexError:
                # Тут создан отдельный класс картинки
                n_img = self.images[i] + self.images[0]
            self.process_images([self.images[i], n_img], [-3, -2, -1, 0, 1, 2, 3])

    def run_fillament(self):
        for z in tqdm.tqdm(range(len(self.a))):
            for i in range(len(self.a[z])):
                for j in range(len(self.a[z][i])):
                    params = self.coefs[i][j]
                    if params['ind0'] < 0 or params['ind0'] >= self.shape[0] or params['ind1'] < 0 or params['ind1'] >= \
                            self.shape[0]:
                        self.a[z][i][j] = 0
                    else:
                        w1 = self.images[params['first']].image[z][params['ind0']] * params['a1']
                        w2 = self.images[params['first']].image[z][params['ind1']] * params['a2']
                        w3 = self.images[params['second']].image[z][params['ind0']] * params['a3']
                        w4 = self.images[params['second']].image[z][params['ind1']] * params['a4']
                        self.a[z][i][j] = w1 + w2 + w3 + w4

    def process_images(self, imgs, offsets=[0]):
        """
        Process image with offset
        :param offsets: array woth offsets
        :param imgs: array with images
        :return:
        """
        for img in imgs:
            for offset in offsets:
                offset_x = offset * sin(img.angle)
                offset_y = offset * cos(img.angle)
                for j in range(img.shape[0]):
                    for k in range(img.shape[1]):
                        x = round((k - img.offset) * cos(img.angle) + img.offset + offset_x)
                        y = round((k - img.offset) * sin(img.angle) + img.offset + offset_y)
                        try:
                            if self.a[j][y][x] != -1:
                                self.a[j][y][x] = 0.5 * (self.a[j][y][x] + img.image[j][k])
                            else:
                                self.a[j][y][x] = img.image[j][k]
                        except IndexError:
                            pass

    def get_nearest_img(self, angle):
        for i in range(len(self.angles)):
            ang = self.angles[i]
            if ang < angle:
                continue
            else:
                return i - 1, i
        return len(self.angles) - 1, 0

    def interp_plate(self):
        for j in tqdm.tqdm(range(len(self.a[0]))):
            r, c = np.where(self.a[0] >= 5)
            for k in range(len(self.a[0][j])):
                if self.a[0][j][k] == -1:
                    min_idx = ((r - j) ** 2 + (c - k) ** 2)
                    d = min_idx.argsort()[0:4]
                    points = [[r[v], c[v], min_idx[v] ** 0.5] for v in d]
                    for i in range(len(self.a)):
                        val = 0
                        coefs = 0
                        for point in points:
                            val += self.a[i][point[0]][point[1]] / point[2]
                            coefs += 1 / point[2]
                        self.a[i][j][k] = round(val / coefs)

    def calc_coefs(self):
        coefs = [[{} for j in range(self.shape[0])] for i in range(self.shape[0])]
        offset = round(self.shape[0] / 2)
        for i in range(self.shape[0]):
            for j in range(self.shape[0]):
                x, y = i - offset, offset - j
                a = math.atan2(y, x)
                if a < 0:
                    a += math.pi
                l = (x ** 2 + y ** 2) ** 0.5
                i1, i2 = int(l), int(l) + 1
                if x < 0:
                    i1 = offset - i1
                    i2 = offset - i2
                else:
                    i1 += offset
                    i2 += offset
                first, second = self.get_nearest_img(a)
                coefs[i][j]['first'], coefs[i][j]['second'] = first, second
                coefs[i][j]['ind0'], coefs[i][j]['ind1'] = i1, i2
                # TODO fix calculation
                a1, a2, a3, a4 = self.calc_weights(x, y, int(l), int(l) + 1,
                                                   self.angles[first], self.angles[second])
                coefs[i][j]['a1'], coefs[i][j]['a2'] = a1, a2
                coefs[i][j]['a3'], coefs[i][j]['a4'] = a3, a4
        self.coefs = coefs

    def calc_weights(self, x, y, p1, p2, a1, a2):
        """
        Calculates weights for interpolation
        :param x: x
        :param y: y
        :param p1: dist of image point 1
        :param p2: dist of image point 2
        :param a1: angle of first image
        :param a2: angle of second image
        :return:
        """
        l1 = ((x - p1 * cos(a1)) ** 2 + (y - p1 * sin(a1)) ** 2) ** 0.5
        l2 = ((x - p2 * cos(a1)) ** 2 + (y - p2 * sin(a1)) ** 2) ** 0.5
        l3 = ((x - p1 * cos(a2)) ** 2 + (y - p1 * sin(a2)) ** 2) ** 0.5
        l4 = ((x - p2 * cos(a2)) ** 2 + (y - p2 * sin(a2)) ** 2) ** 0.5
        try:
            c = (l1 * l2 * l3 * l4) / (l1 * l2 * (l3 + l4) + l1 * l3 * l4 + l2 * l3 * l4)
            return c / l1, c / l2, c / l3, c / l4
        except ZeroDivisionError:
            c = 0.5
            return 0, c / l2, 0, c / l4

    def process_transform(self):
        for j in tqdm.tqdm(range(len(self.a))):
            for k in range(len(self.a[j][0])):
                if self.a[j][0][k] == -1:
                    a, b = self.get_nearest_img(atan2(k, j))
                    imgs = [self.images[a], self.images[b]]
                    dists = []
                    for im in imgs:
                        dists.extend(im.get_dists(j, k, 0))
                    dists.sort(key=extract_dist)
                    points = dists[0:4]
                    coefs = []
                    for point in points:
                        try:
                            coefs.append(1 / point['dist'])
                        except ZeroDivisionError:
                            coefs = [0 for i in range(4)]
                            coefs[0] = 1
                            break
                    kc = 1 / sum(coefs)
                    for i in range(len(self.a)):
                        val = 0
                        for ind, point in enumerate(points):
                            im = self.get_image_from_angle(point['angle'])
                            val += im.image[i][point['index']] * kc * coefs[ind]
                        self.a[j][i][k] = val
                else:
                    pass

    def get_image_from_angle(self, angle):
        for im in self.images:
            if im.angle == angle:
                return im

    def get_filenames(self):
        os.chdir(self.path)
        self.names = [name for name in os.listdir(os.getcwd()) if os.path.splitext(name)[1] == '.png']

    def load_angles(self):
        """
        Loades file with angles
        :return:
        """
        angles = []
        with open(self.a_file, 'r') as f:
            lines = f.readlines()
            for line in lines:
                angles.append(line.split(sep=';')[10])
        first_val = float(angles[0])
        self.angles = [float(angle) - first_val for angle in angles]

    def save_vertex(self):
        os.mkdir("./result")
        print(self.a.shape)
        for i in tqdm.tqdm(range(self.shape[0])):
            img = self.a[:, i, :]
            if i >= 100:
                cv2.imwrite("./result/Image" + str(i) + ".png", img)
            elif i < 10:
                cv2.imwrite("./result/Image00" + str(i) + ".png", img)
            elif i < 100:
                cv2.imwrite("./result/Image0" + str(i) + ".png", img)


def construct_interp_poly(images, angles):
    """
    Constructs 2nd order poly
    :param images: array with images
    :param angles: array with corresponding angles
    :return: interp function
    """
    a2 = (images[2] - images[0]) / ((angles[2] - angles[0]) * (angles[2] - angles[1])) - \
         (images[1] - images[0]) / ((angles[1] - angles[0]) * (angles[2] - angles[1]))
    a1 = (images[1] - images[0]) / (angles[1] - angles[0]) - a2 * (angles[1] + angles[0])
    a0 = images[0] - a1 * angles[0] - a2 * angles[0] ** 2
    return lambda x: a0 + a1 * x + a2 * x ** 2


if __name__ == "__main__":
    # i = Image("tests/test_2.png", 30)
    # shape = i.shape
    # a = np.ndarray((shape[1] + 2, int(1.2 * shape[0]), int(1.2 * shape[0])))
    # i.get_dists(10, 10, 0)
    m = Model("C:\\Users\\FEDOR\\Documents\\data\\5\\png",
              "C:\\Users\\FEDOR\\Documents\\data\\5\\png\\CaptSave.tag")
    m.calc_coefs()
    m.run_fillament()
    # m.transform_img()
    print("Image transformed")
    # m.interp_plate()
    # m.process_transform()
    # m.interp_plate()
    m.save_vertex()
