# Artemis PAYDAY 2 plugin

[![GitHub release version](https://img.shields.io/github/v/release/alixti/Artemis.Plugins.Payday2GSI.svg)](https://github.com/alixti/Artemis.Plugins.Payday2GSI/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Artemis plugin for PAYDAY 2 Game State Integration.

## Important Requirement

> [!WARNING]
> This plugin requires SuperBLT (or BLT) in your PAYDAY 2 installation.
> SuperBLT is not bundled with this plugin and must be installed separately.

## Installation

- Download the latest plugin release from [Releases](https://github.com/alixti/Artemis.Plugins.Payday2GSI/releases/latest).
- In Artemis, go to Settings -> Plugins, click Import plugin, and select the downloaded archive.
- Make sure SuperBLT/BLT is already installed in PAYDAY 2.
- The plugin prerequisite installs the PAYDAY 2 GSI mod and creates `PAYDAY 2/GSI/Artemis.xml`.

## Building from Source

**Requirements:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or JetBrains Rider

**Steps:**

```bash
git clone https://github.com/alixti/Artemis.Plugins.Payday2GSI.git
cd Artemis.Plugins.Payday2GSI
dotnet build src/Artemis.Plugins.sln -p:EnablePluginCopy=true
```

When building inside Rider or Visual Studio, the plugin is automatically copied to the Artemis plugins folder after a successful build.  
When building via the command line, include `-p:EnablePluginCopy=true` to enable the same behavior.

## How It Works

- The plugin starts a local HTTP server on `127.0.0.1:17042`.
- PAYDAY 2 GSI sends game state updates to `GET /gameState/pd2`.
- The plugin parses incoming JSON and updates Artemis data model values for:
	- Level phase
	- Local player health and armor
	- Swansong/state/suspicion
	- Primary/secondary ammo left and selected weapon

> [!NOTE]
> PAYDAY 2 SuperBLT does not provide a method to use HTTP POST (only GET).
> Because of this limitation, the plugin must run a separate local server instead of using the Artemis Web Server (Artemis only allows POST requests).

## Prerequisite Behavior

- Validates that PAYDAY 2 is installed through Steam.
- Validates that SuperBLT/BLT is present.
- Ensures `mods` and `GSI` folders exist.
- Downloads and extracts the PAYDAY 2 GSI mod into the `mods` folder.
- Writes `Artemis.xml` with the correct local endpoint.

## Contributing

Found a bug or have a feature request? [Open an issue](https://github.com/alixti/Artemis.Plugins.Payday2GSI/issues/new).  
Want to contribute code? Fork the repo and open a pull request.