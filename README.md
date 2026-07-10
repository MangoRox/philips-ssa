# HPM SysEng - System Setup Automation

Automates the Philips PMP setup wizard using UI automation (FlaUI). Attaches to `Philips.PMP.SystemSetupHost.exe`, reads config from `config.json`, and drives each setup page automatically.

## Requirements

- .NET 8
- `Philips.PMP.SystemSetupHost.exe` running
- a `config.json` configured

## Usage

1. Start the Philips setup wizard
2. Fill in `config.json` with your topology and licensing options
3. Run the tool — it will drive through the wizard automatically

## How to setup your config.json

section tba

## To-do

- config file revamp
- config option to stop on a page if needed
