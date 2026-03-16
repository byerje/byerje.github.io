from PIL import Image
from pyzbar.pyzbar import decode
import os

codes_dir = "clue/codes"
for fname in sorted(os.listdir(codes_dir))[:6]:
    if fname.endswith(".png"):
        img = Image.open(os.path.join(codes_dir, fname))
        results = decode(img)
        for r in results:
            print(fname, "->", r.data.decode("utf-8"))
