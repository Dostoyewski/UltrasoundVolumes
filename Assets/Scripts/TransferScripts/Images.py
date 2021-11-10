import math
import os
from math import sin, cos

import cv2
import numpy as np
import tqdm

DEBUG = True


class Point(object):
    """
    Класс точки на картинке, нужен чтобы оптимально находить ближайшие
    """

    def __init__(self, a, p, x, y):
        self.angle = a
        self.p = p
        self.x = x
        self.y = y
        self.l = ((x - p * cos(a)) ** 2 + (y - p * sin(a)) ** 2) ** 0.5


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
        self.a = np.full((self.shape[0], int(self.shape[1]), int(self.shape[1])), -1)
        self.images = []
        self.construct_images()

    def construct_images(self):
        for i in range(len(self.angles)):
            if DEBUG:
                self.images.append(Image(self.names[i], float(self.angles[i])))

    def run_fillament(self):
        for z in tqdm.tqdm(range(len(self.a))):
            for i in range(len(self.a[z])):
                for j in range(len(self.a[z][i])):
                    params = self.coefs[i][j]
                    if params[2] < 0 or params[2] >= self.shape[1] or params[3] < 0 or params[3] >= self.shape[1]:
                        self.a[z][i][j] = 0
                    else:
                        w1 = self.images[params[0]].image[z][params[2]] * params[4]
                        w2 = self.images[params[0]].image[z][params[3]] * params[5]
                        w3 = self.images[params[1]].image[z][params[2]] * params[6]
                        w4 = self.images[params[1]].image[z][params[3]] * params[7]
                        self.a[z][i][j] = w1 + w2 + w3 + w4

    def get_nearest_img(self, angle):
        for i in range(len(self.angles)):
            ang = self.angles[i]
            if ang < angle:
                continue
            else:
                return i - 1, i
        return len(self.angles) - 1, 0

    def calc_coefs(self):
        self.coefs = [[{} for j in range(self.shape[1])] for i in range(self.shape[1])]
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
                points = [Point(self.angles[first], int(l) - 1, x, y),
                          Point(self.angles[second], int(l) - 1, x, y),
                          Point(self.angles[first], int(l), x, y),
                          Point(self.angles[second], int(l), x, y),
                          Point(self.angles[first], int(l) + 1, x, y),
                          Point(self.angles[second], int(l) + 1, x, y)]
                points.sort(key=lambda x: x.l)
                a1, a2, a3, a4 = self.calc_weights(x, y, int(l), int(l) + 1,
                                                   self.angles[first], self.angles[second])
                self.coefs[i][j] = [first, second, i1, i2, a1, a2, a3, a4]

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
            p = 4
            a1 = 1 / (l1 ** p)
            a2 = 1 / (l2 ** p)
            a3 = 1 / (l3 ** p)
            a4 = 1 / (l4 ** p)
            c = 1 / (a1 + a2 + a3 + a4)
            return c * a1, c * a2, c * a3, c * a4
        except ZeroDivisionError:
            c = 0.5
            return 0, c / l2, 0, c / l4

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
        foldername = 'result'
        os.mkdir("./" + foldername)
        print(self.a.shape)
        for i in tqdm.tqdm(range(self.shape[0])):
            img = self.a[i, :, :]
            # img = cv2.blur(img, (3, 3))
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
    m.run_fillament()
    print("Image transformed")
    m.save_vertex()
