# TOKI releases

Current release: **0.1.4**.

Increment the patch digit for each published release. After patch 9, increment
the minor number and reset patch to 0: `0.1.8` → `0.1.9` → `0.2.0`.
Do not publish a release for every small commit; keep each meaningful change
in its own commit, then publish the tested batch.

Update `Version` in `Toki.csproj`, the release version and notes in
`docs/index.html`, and this document together. Tag the release commit `vX.Y.Z`.
Publish a self-contained Windows x64 single-file executable. Never include
`oauth.local.json` or personal application data.

Upload both `TOKI-vX.Y.Z-win-x64.exe` and the stable alias `TOKI-win-x64.exe`
to every release. The website uses `/releases/latest/download/TOKI-win-x64.exe`
so its download button continues to point at the latest release.

Update the local `bin/Release/net6.0-windows/Toki.exe` with the same published
executable and compare its SHA-256 hash before finishing.
