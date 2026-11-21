# POKI · Obsidian

A complete visual concept pack as the black dark-mode counterpart to the Violet Air pack.

Open **index.html** to browse all 14 design boards. Select a board for a full-size view; use the arrow keys to move through the pack. Original PNGs are in **screens/**.

## Scope

These images propose a new interface for POKI. They are design mockups, not screenshots of implemented application changes. Demo accounts, messages, dates and usage values are fictional. The existing application and the repository README are not modified by this pack.

## Theme

| Token | Value |
|---|---|
| Canvas | #08090B |
| Panel | #14161B |
| Primary violet | #9480FF |
| Selected background | #28223F |
| Text | #F1F2F6 |
| Secondary text | #A3A8B5 |
| Border | #2A2D35 |
| Typography | Segoe UI / Inter-style sans serif |
| Panel radius | 18 px |
| Control radius | 10 px |
| Spacing | 8 px base; 24–32 px section gaps |

Use the existing blue robot identity, thin line icons, quiet borders and restrained shadows. Keep important text readable against black and charcoal; final implementation must verify contrast, keyboard focus and Windows scaling.

## Screen inventory

01. **Floating robot & one-touch launcher** — [PNG](screens/01-floating-launcher.png)
02. **Shortcuts — full workspace** — [PNG](screens/02-shortcuts-workspace.png)
03. **Chat & assistant actions** — [PNG](screens/03-chat.png)
04. **Reminder overview** — [PNG](screens/04-reminders.png)
05. **Add / edit reminder** — [PNG](screens/05-reminder-editor.png)
06. **Email inbox & account switching** — [PNG](screens/06-email-inbox.png)
07. **Email composition & send approval** — [PNG](screens/07-email-compose.png)
08. **Manual ChatGPT usage tracker** — [PNG](screens/08-usage.png)
09. **Settings — appearance & one-touch behavior** — [PNG](screens/09-settings-general.png)
10. **Settings — manage app shortcuts** — [PNG](screens/10-settings-shortcuts.png)
11. **Settings — AI provider & voice** — [PNG](screens/11-settings-ai-voice.png)
12. **Settings — connected accounts & sign-in** — [PNG](screens/12-settings-accounts.png)
13. **Settings — notifications & privacy** — [PNG](screens/13-settings-privacy-notifications.png)
14. **Tray menu, empty states & feedback** — [PNG](screens/14-system-states.png)

## Primary journey

Floating robot → icon-only shortcut launcher → hamburger → full Shortcuts workspace. The hamburger opens the app directly. The launcher close button hides the popup. Both buttons share an equal hit area. Adding/removing shortcuts happens in Settings or the full Shortcuts workspace.

Full workspace navigation: Shortcuts, Chat, Reminders, Email, Usage, Settings. Settings contains General, Shortcuts, AI & voice, Accounts, Notifications and Privacy.

## Behavior to preserve

- Manual floating icon size control and animation on/off; preserve robot aspect ratio.
- Icon-only compact launcher; accessible app names and optional hover tooltips.
- Persist reminders and shortcut settings; allow snooze and completion.
- Email preview and explicit send confirmation. A success state appears only after confirmed provider success.
- Account connection stays visibly pending until OAuth succeeds; retain cancel, retry, active-account and change-account actions.
- Usage percentages must be explicitly manual estimates when no supported automatic source exists; never substitute API billing for ChatGPT subscription quota.
- Voice starts only after microphone activation. No continuous recording.
- Sensitive actions have clear, reversible cancellation paths.

## Proposed extensions

Some mockups explore features beyond the current build: conversation grouping, attachment controls, calendar overview, email thread summaries/reply/forward/drafts, quiet hours, selectable voices, appearance customization and folder categories. Treat these as proposed UI, not a statement that they are already implemented. The screenshots show several mutually exclusive states together on boards 01, 07, 13 and 14 to document the full flow.

## Generation

Generated with the built-in image generation tool; no API-key or paid CLI workflow was used. The complete prompts and screen descriptions are in **prompts.json**. Input references: the light Violet Air screens and POKI's existing robot logo; the black Shortcuts master screen was then used to keep subsequent screens consistent.

