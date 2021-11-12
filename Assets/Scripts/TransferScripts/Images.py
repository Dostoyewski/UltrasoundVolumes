import math
import os
from math import sin, cos

import cv2
import numpy as np
import tqdm
from numba import njit

DEBUG = True


class Point(object):
    """
    Класс точки на картинке, нужен чтобы оптимально находить ближайшие
    """

    def __init__(self, a, p, x, y, index):
        self.angle = math.pi / 2 - a
        self.p = p
        self.x = x
        self.y = y
        self.l = ((x - p * cos(self.angle)) ** 2 + (y - p * sin(self.angle)) ** 2) ** 0.5
        self.i = index
        self.a = None

    def get_weight(self, p):
        if self.l != 0:
            self.a = 1 / (self.l ** p)
            return self.a
        else:
            self.a = 1
            return 1

    def construct_info_arr(self, c, offset):
        self.a *= c
        if self.x < 0:
            self.p = offset - self.p
        else:
            self.p += offset
        return [self.i, self.p, self.a]


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


@njit
def run_njit_fill(img, a, coefs, border):
    for z in range(len(a)):
        for i in range(len(a[z])):
            for j in range(len(a[z][i])):
                params = coefs[i][j]
                s = 0
                for k in range(4):
                    # Add to remove negative indexing
                    if params[k][1] < 0 or params[k][1] >= border:
                        a[z][i][j] = 0
                        break
                    image = img[int(params[0][0])]
                    s += image[z][int(params[k][1])] * params[k][2]
                a[z][i][j] = s
    return a


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
        self.a = np.full((self.shape[0], int(self.shape[1]), int(self.shape[1])), -1)
        self.images = []
        self.construct_images()

    def construct_images(self):
        for i in range(len(self.angles)):
            if DEBUG:
                self.images.append(Image(self.names[i], float(self.angles[i])))

    def run_fillament(self):
        img = [image.image for image in self.images]
        for z in tqdm.tqdm(range(len(self.a))):
            for i in range(len(self.a[z])):
                for j in range(len(self.a[z][i])):
                    params = self.coefs[i][j]
                    if params == -1:
                        self.a[z][i][j] = 0
                    else:
                        s = 0
                        for k in range(4):
                            s += img[params[k][0]][z][params[k][1]] * params[k][2]
                        self.a[z][i][j] = s

    def get_nearest_img(self, angle):
        for i in range(len(self.angles)):
            ang = self.angles[i]
            if ang < angle:
                continue
            else:
                return i, i - 1
        return 0, len(self.angles) - 1

    def calc_coefs(self):
        """
        Calculates coefficients for image interpolation.
        :return:
        """
        self.coefs = [[[] for j in range(self.shape[1])] for i in range(self.shape[1])]
        offset = round(self.shape[1] / 2)
        for i in range(self.shape[1]):
            for j in range(self.shape[1]):
                x, y = i - offset, offset - j
                a = math.atan2(x, y)
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
                # TODO fix distance calculating
                points_l = []
                points_r = []
                for k in range(int(l) - 2, int(l) + 2):
                    points_l.append(Point(self.angles[first], k, x, y, first))
                    points_r.append(Point(self.angles[second], k, x, y, second))
                points_l.extend(points_r)
                points_l.sort(key=lambda x: x.l)
                points_r.sort(key=lambda x: x.l)
                res = [points_l[0], points_l[1], points_r[0], points_r[1]]
                val = self.calc_weights_from_points(points_l[0:4], offset)
                self.coefs[i][j] = val
        self.coefs = np.array(self.coefs, dtype=np.float32)

    def calc_weights_from_points(self, points, offset):
        """
        Calculates weight for interpolation with Point list given
        :param points: list of points instances
        :param offset: offset from middle of center point
        :return:
        """
        p = 1
        sum_a = sum([point.get_weight(p) for point in points])
        c = 1 / sum_a
        return [point.construct_info_arr(c, offset) for point in points]

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
        foldername = 'result_new_alg2_blur'
        os.mkdir("./" + foldername)
        print(self.a.shape)
        for i in tqdm.tqdm(range(self.shape[0])):
            img = self.a[i, :, :]
            img = cv2.blur(img, (3, 3))
            if i >= 100:
                cv2.imwrite("./" + foldername + "/Image" + str(i) + ".png", img)
            elif i < 10:
                cv2.imwrite("./" + foldername + "/Image00" + str(i) + ".png", img)
            elif i < 100:
                cv2.imwrite("./" + foldername + "/Image0" + str(i) + ".png", img)


if __name__ == "__main__":
    m = Model("C:\\Users\\FEDOR\\Documents\\data\\5\\png",
              "C:\\Users\\FEDOR\\Documents\\data\\5\\png\\CaptSave.tag")
    m.calc_coefs()
    img = np.array([image.image for image in m.images])
    m.a = run_njit_fill(img, m.a, m.coefs, m.shape[1] - 1)
    print("Image transformed")
    m.save_vertex()
