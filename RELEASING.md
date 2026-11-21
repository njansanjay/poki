# POKI releases

Current release: **0.1.6**.

Increment the patch digit for each published release. After patch 9, increment
the minor number and reset patch to 0: `0.1.8` → `0.1.9` → `0.2.0`.
Do not publish a release for every small commit; keep each meaningful change
in its own commit, then publish the tested batch.

Update `Version` in `Poki.csproj`, the release version and notes in
`docs/index.html`, and this document together. Tag the release commit `vX.Y.Z`.
Publish a self-contained Windows x64 single-file executable. Never include
`oauth.local.json` or personal application data.

Upload both `POKI-vX.Y.Z-win-x64.exe` and the stable alias `POKI-win-x64.exe`
to every release. The website uses `/releases/latest/download/POKI-win-x64.exe`
so its download button continues to point at the latest release.
Keep release tags and filenames plain: use only the product name and version;
do not append cat, theme, or UI labels.

Update the local `bin/Release/net6.0-windows/Poki.exe` with the same published
executable and compare its SHA-256 hash before finishing.
