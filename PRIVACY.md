# Privacy

The launcher itself does not transmit telemetry or account information. It invokes the bundled BetterDiscord CLI. During installation that component contacts upstream release services to obtain BetterDiscord; those services receive normal download-request information such as IP addresses. BetterDiscord and Discord have their own network behavior and policies.

The launcher writes a local diagnostic log. It does not read Discord messages, passwords, or tokens. See the upstream BetterDiscord CLI source at https://github.com/BetterDiscord/cli/tree/375c62f and BetterDiscord documentation at https://docs.betterdiscord.app/ for component behavior.
