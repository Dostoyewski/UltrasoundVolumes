import os

import tqdm
from PIL import Image

os.chdir("C:\\Users\\Федор\\Documents\\GIT\\UnityVolumeRendering\\DataFiles\\sv1\\linear\\2")
os.mkdir("./png/")

RESIZE = True
size = (300, 300)

for fname in tqdm.tqdm(os.listdir(os.getcwd())):
    try:
        if RESIZE:
            im = Image.open(fname)
            out = im.resize(size, Image.BICUBIC)
            out.save("./png/" + os.path.splitext(fname)[0] + '.png')
        else:
            Image.open(fname).save("./png/" + os.path.splitext(fname)[0] + '.png')
    except:
        pass
