import math
import os
import time
from multiprocessing import cpu_count, freeze_support, Pool

import cv2
import numpy as np
import tqdm
from scipy.interpolate import interp1d

CORES = cpu_count()
INTERP = False
TEST = False
MULTITHREADING = False
# TODO: refactor code & add to git

os.chdir("C:\\Users\\Федор\\Documents\\GIT\\UnityVolumeRendering\\DataFiles\\sv1\\test2")
if TEST:
    os.chdir("C:\\Users\\Федор\\Documents\\GIT\\UnityVolumeRendering\\DataFiles\\sv1\\test\\PNG_Rad")


def construct_linear(images, angles):
    """
    Constructs linear interpolation polynom
    :param images: array with two images
    :param angles: array with corresponding angles to images
    :return:
    """
    a1 = (images[1] - images[0]) / (angles[1] - angles[0])
    a0 = images[0] - a1 * angles[0]
    return lambda x: a0 + a1 * x


def fill_zeros(a, thresh):
    """
    Fills elements in array with zeros if val < thresh
    :param a:
    :param thresh:
    :return:
    """
    for i in range(len(a)):
        for j in range(len(a[i])):
            for k in range(len(a[i][j])):
                if a[i, j, k] < thresh:
                    a[i, j, k] = 0
    return a


def interp_through_1darr(arr):
    """
    Interpolates thorugh 1d array
    :param arr:
    :return:
    """
    x = np.arange(len(arr))
    idx = np.nonzero(arr)
    try:
        interp = interp1d(x[idx], arr[idx])
        return interp(x)
    except ValueError:
        return arr


def decart_interp(a):
    """
    Interpolates a in decarte coordinate system
    :param a: array
    :return:
    """
    a = fill_zeros(a, 40)
    for i in range(len(a)):
        for j in range(len(a[i][0])):
            a[i, :, j] = interp_through_1darr(a[i, :, j])
    return a


def process_photo(img, shape, a, angle):
    """
    Proceses image
    :param img:
    :param shape:
    :param a:
    :return:
    """
    for j in range(shape[0]):
        for k in range(shape[1]):
            x = math.floor((k - shape[0] / 2) * math.sin(angle) + 0.5 * a.shape[2] / math.sqrt(2))
            y = math.floor((k - shape[0] / 2) * math.cos(angle) + 0.5 * a.shape[2] / math.sqrt(2))
            try:
                # вероятно должна быть инверсия
                if a[j][y][x] != 0:
                    a[j][y][x] = 0.5 * (a[j][y][x] + img[j][k])
                else:
                    a[j][y][x] = img[j][k]
            except IndexError:
                pass
    return a


def process_photo_parallel(data):
    return process_photo(data[0], data[1], data[2], data[3])


def load_image(i):
    """
    Loads ith image from dataset
    :param i: index
    :return: image
    """
    name = "CaptSave" + str(i) + ".png"
    # name = "test_1.png"
    if TEST:
        num_true = str(i) if i > 9 else "0" + str(i)
        name = "rad" + num_true + ".png"
    img = cv2.imread(name, 0)
    return img


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


def load_angles():
    """
    Loades file with angles
    :return:
    """
    angles = []
    with open("./CaptSave.tag", 'r') as f:
        lines = f.readlines()
        for line in lines:
            angles.append(line.split(sep=';')[10])
    return angles


if __name__ == "__main__":
    angles = load_angles()
    names = [name for name in os.listdir(os.getcwd()) if os.path.splitext(name)[1] == '.png']
    max_x, max_y = 0, 0
    for name in names:
        img = cv2.imread(names[0])
        if max_x < img.shape[0]:
            max_x = img.shape[0]
        if max_y < img.shape[1]:
            max_y = img.shape[1]
    shape = (max_x, max_y)
    a = np.ndarray((shape[1] + 2, int(1.2 * shape[0]), int(1.2 * shape[0])))
    freeze_support()
    start_time = time.time()
    # Proxessing all standart photos
    print("Starting main conversation algorithm")
    if MULTITHREADING:
        with Pool(CORES - 1) as pool:
            a = sum(tqdm.tqdm(pool.imap(process_photo_parallel,
                                        [(load_image(i), shape, a, float(angles[i])) for i in range(len(angles))])))
    else:
        for i in tqdm.tqdm(range(len(angles))):
            a = process_photo_parallel((load_image(i), shape, a, float(angles[i])))
    print("Time taken: ", time.time() - start_time)
    start_time = time.time()
    # Interpolation
    if INTERP:
        for i in tqdm.tqdm(range(0, len(angles), 2)):
            images = []
            ang = []
            for j in range(2):
                if i + j < len(angles):
                    images.append(load_image(i + j))
                    ang.append(float(angles[i + j]))
                else:
                    images.append(j)
                    ang.append(float(angles[j]) + math.pi)
            # f = construct_interp_poly(images, ang)
            f = construct_linear(images, ang)
            num_step = 10
            delta = (ang[1] - ang[0]) / num_step
            for k in range(num_step):
                angle = ang[0] + i * delta
                a = process_photo(f(angle), shape, a, angle)
            # num_points = 10
            # for i in range(num_points):
            #     imgt = np.add(images[0] * i / num_points, images[1] * (1 - i / num_points))
            #     angs = ang[0] * i / num_points + ang[1] * (1 - i / num_points)
            #     a = process_photo(imgt, shape, a, angs)
    # a = decart_interp(a)
    print("Time taken: ", time.time() - start_time)
    os.mkdir("./result")
    print("\nSaving images...")
    for i in tqdm.tqdm(range(shape[0])):
        img = a[i, :, :]
        for j in range(len(img)):
            img[j] = interp_through_1darr(img[j])
        if img.max() > 20:
            if i >= 100:
                cv2.imwrite("./result/Image" + str(i) + ".png", img)
            elif i < 10:
                cv2.imwrite("./result/Image00" + str(i) + ".png", img)
            elif i < 100:
                cv2.imwrite("./result/Image0" + str(i) + ".png", img)
    z, x, y = a.nonzero()
