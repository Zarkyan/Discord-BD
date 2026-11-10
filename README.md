# Discord-BD

Unofficial Windows launcher for Discord Stable and BetterDiscord. Not affiliated with Discord or BetterDiscord.

The launcher checks whether BetterDiscord is injected, runs the bundled official BetterDiscord CLI to install it when needed, opens Discord, and exits. No tray icon, startup registration or background monitoring. Checks happen only when this launcher is run. Client incompatibilities are not necessarily fixed by reinstalling.

## Requirements and use

Windows 10/11 x64, .NET Framework 4.8, and a standard Discord Stable installation under the current user's LocalAppData. Internet access is needed when the BetterDiscord CLI downloads BetterDiscord. No separate bdcli installation is required.

Run Discord-BD.exe. If a repair is needed, the CLI may stop and restart Discord. The unsigned launcher may trigger SmartScreen. No security setting is modified.

Bundled CLI files and their license are extracted to LocalAppData/BDGuardian/tools under a SHA-256 directory. Diagnostic messages are stored in LocalAppData/BDGuardian/guardian.log.

## Removal

Close the launcher and delete its executable. Its local files can be removed by deleting the BDGuardian folder under LocalAppData. This does not uninstall BetterDiscord: use the official BetterDiscord installer and its Uninstall option to remove BetterDiscord itself. Discord, themes and plugins are not removed by deleting the launcher.

## Build

Run `powershell -NoProfile -File .\build.ps1` on Windows with the .NET Framework compiler installed. The output is dist/Discord-BD.exe. The official upstream CLI is included unchanged, its checksum is checked before compilation, and its license is embedded with it. See THIRD_PARTY.md.

## Code signing policy

This project is currently UNSIGNED. A SignPath application is being prepared; no approval or certificate has been granted. Maintainer, reviewer and release approver: [Zarkyan](https://github.com/Zarkyan). Any future signing must use a traceable build from this repository, with explicit release approval. See PRIVACY.md.

