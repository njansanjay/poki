# POKI visual direction

Poki’s interaction reference is [Bloom](https://github.com/little-systems/bloom) and its [showcase](https://bloom.sehaz.space/). Use it as a continuing design reference for the app and website: dark glass-like islands, rounded surfaces, responsive motion, generous spacing, strong hierarchy, and controls that feel like part of the desktop rather than old dialog boxes. Do not copy Bloom’s branding, assets, or implementation; Poki remains centered on the orange cat.

Use this direction for future POKI interfaces unless a feature has a clear reason to differ.

## Character

POKI should feel like a small premium desktop object: warm, quiet, playful and precise. The orange pixel cat is the center of the identity. Use only its face for product marks, favicons, executable icons and compact branding. Use the full cat for the living desktop companion and illustrative moments.

## Palette

- Canvas: near-black `#0A0909`
- Sidebar: warm black `#0E0C0C`
- Surface: warm charcoal `#151211`
- Field: dark brown-black `#1F1A18`
- Border: muted brown `#3F3029`
- Primary accent: peach-orange `#FFAC77`
- Accent hover: light peach `#FFC7A3`
- Accent text: espresso `#301C13`
- Muted text: warm gray `#AE9D95`

Keep large areas dark and restrained. Use orange for primary actions, focus, selected states and small moments of character rather than filling every surface.

## Type

Use Bahnschrift for the Windows app and as the first website font. Prefer clear size and weight changes over many type styles. Headings are bold with tight spacing; supporting copy is regular and warm gray.

## Shape and motion

- Rounded corners: 8–14 px for controls and cards.
- Borders stay thin and quiet.
- Motion should be short, useful and optional.
- Floating panels close when focus moves elsewhere, with Escape, or through their close control.
- Load final visual assets before showing transient panels so placeholder icons do not flash.

## Reference implementation

The website in `docs/` is the visual reference for spacing, warm color, dark floating surfaces, typography hierarchy and restrained animation. Translate its ideas to native controls instead of copying browser-only effects that reduce reliability or accessibility.
