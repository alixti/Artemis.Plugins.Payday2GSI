# Artemis PAYDAY 2 plugin

[![GitHub release version](https://img.shields.io/github/v/release/alixti/Artemis.Plugins.Payday2GSI.svg)](https://github.com/alixti/Artemis.Plugins.Payday2GSI/releases)

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