# POKI brand assets

## Desktop cat

- `cat-mark.png`: transparent, face-only pixel cat mark used by the in-app wordmark and as the source for every compact POKI logo.
- `poki.ico`: Windows icon container generated from `cat-mark.png` at 16, 24, 32, 48, 64, 128 and 256 px. Used by the executable, windows and tray.
- `docs/assets/poki-cat.png`: 256 px website favicon generated from the same cat-face source.
- Cat-mark prompt: “Use the supplied orange pixel-art cat as the exact identity reference. Isolate and redraw only its front-facing head and face, centered on a square transparent canvas. Preserve the orange tabby stripes, cream muzzle and inner ears, large dark round eyes, small plum nose and dark plum pixel outline. Crisp blocky pixel art. No neck, chest, body, paws, tail, border, badge, circle, text, shadow, glow or background.” Generated with the built-in image generator.

- `cat-sprites.png`: transparent 4 × 2 atlas based on the user-provided orange pixel kitten. Top row is the four-frame walking cycle; bottom row contains seated, half-blink, blink and tail-turn poses. The app mirrors the walking frames for leftward movement.
- Generated with the built-in image generator, then copied into this project. The sprite atlas is rendered with nearest-neighbor sampling to preserve the pixel-art style.
- Prompt: “Use the supplied orange pixel-art kitten as the exact character reference: orange tabby, cream chest and muzzle, dark plum blocky outline, large dark eyes and cream-tipped tail. Create a transparent 4-column by 2-row atlas with four walk-right poses and four seated/blinking/tail poses. Keep scale, baseline and padding consistent. No grid, text, ground line, shadow or background.”

The older robot artwork remains available as a legacy asset; the desktop companion and in-app wordmark use the cat.

- `robot-head.png`: transparent robot-only cutout retained for legacy compatibility and renderer checks.
- The older robot `poki.ico` was replaced by the cat-face icon.
- The in-app wordmark pairs the cat face with the text `poki`.

Image edit prompt: “Extract ONLY the tilted blue robot head at the left, including its separate blue top and bottom cap shapes. Remove all pale background and remove the word poki. Preserve the exact silhouette, proportions, tilt, vivid royal blue color and two circular eye holes; eye holes must be transparent too. Center the isolated complete robot symbol on a square transparent canvas, with about 8 percent transparent padding. Flat crisp logo, no redesign, no shadows, no new elements, no text.”
