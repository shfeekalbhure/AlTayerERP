from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "assets" / "images" / "icon.png"
TARGETS = {
    "icon.png": 512,
    "splash-icon.png": 512,
    "favicon.png": 128,
    "android-icon-foreground.png": 512,
}


def main() -> None:
    image = Image.open(SOURCE).convert("RGBA")
    for filename, size in TARGETS.items():
        resized = image.resize((size, size), Image.Resampling.LANCZOS)
        resized.save(ROOT / "assets" / "images" / filename, format="PNG", optimize=True)


if __name__ == "__main__":
    main()
