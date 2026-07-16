# HPM SysEng — System Setup Automation

Automates the Philips PMP setup wizard using UI automation (FlaUI). The tool attaches to `Philips.PMP.SystemSetupHost.exe`, reads a `config.json` file, and drives each setup page through the wizard automatically — including topology configuration, licensing, platform security, host qualification, and finalization.

## Requirements

- .NET 8
- `Philips.PMP.SystemSetupHost.exe` running before launch
- A populated `config.json` in the working directory

## Usage

1. Start the Philips PMP setup wizard.
2. Populate `config.json` with your environment's values (see [Configuration](#configuration) below).
3. Run the tool — it will attach to the running wizard and drive through each page automatically.

```
dotnet run
```

## Configuration

All behaviour is controlled by `config.json` in the project root.

| Field | Type | Required | Description |
|---|---|---|---|
| `PrimaryServerName` | `string` | Yes | Hostname of the primary server (e.g. `"NX11PRIM"`). Used during topology and server connection steps. |
| `TopologyItemNameChange` | `bool` | No | When `true`, renames topology items during the topology configuration step. |
| `HostLicensingConfiguration` | `object` | No | Per-host licensing assignments. See [Host Licensing](#host-licensing) below. |
| `PlatformSecuritySkippableItems` | `string[]` | No | Platform Security checklist items to skip (e.g. `["Local Security Policy"]`). |
| `HostQualificationSkippableItems` | `string[]` | No | Host Qualification checklist items to skip (e.g. `["Windows Activation", "Link Speed and Duplex"]`). |

### Host Licensing

`HostLicensingConfiguration` is a three-level object structured as:

```
{
  "<HostName>": {
    "<LicenseGroup>": {
      "<FeatureName>": {
        "Enabled": true | false,
        "Row": <int>
      }
    }
  }
}
```

`Row` is the 1-based row index of the feature in the licensing grid for that host.

### Example `config.json`

```json
{
    "TopologyItemNameChange": true,
    "PrimaryServerName": "NRTPRIM",
    "HostLicensingConfiguration": {
        "NRTPRIM": {
            "Enterprise": {
                "Alarm Panel":        { "Enabled": true,  "Row": 12 },
                "Event Notification": { "Enabled": false, "Row": 11 }
            }
        },
        "nrtWEB-1": {
            "Host": {
                "Patient Center View": { "Enabled": true, "Row": 8 }
            }
        },
        "nrtWEB-2": {
            "Host": {
                "Web Proxy Server": { "Enabled": false, "Row": 10 },
                "Web Server":       { "Enabled": true,  "Row": 9  }
            }
        }
    },
    "PlatformSecuritySkippableItems": [
        "Local Security Policy"
    ],
    "HostQualificationSkippableItems": [
        "Windows Activation",
        "Link Speed and Duplex"
    ]
}
```

## Workflow Steps

The orchestrator executes the following steps in wizard page order. Steps with no registered workflow are skipped automatically.

| Workflow | Description |
|---|---|
| Login | Handles initial login credentials. |
| Language Settings | Configures language and locale. |
| Topology Configuration | Sets up server topology and optionally renames items. |
| System Information | Populates system information fields. |
| Server Connection | Configures server connection settings. |
| SQL Server Connection | Configures the SQL Server connection. |
| Database Installation | Handles database installation options. |
| PIC Credentials | Enters Patient Information Center credentials. |
| License Configuration | Assigns per-host feature licensing. |
| Display Configuration | Configures display settings. |
| Peripheral Configuration | Configures peripheral devices. |
| Technical Option Assignment | Assigns technical options. |
| Encryption | Configures encryption settings. |
| Platform Security | Steps through the Platform Security checklist, skipping configured items. |
| Host Qualification | Steps through the Host Qualification checklist, skipping configured items. |
| Setup Complete | Selects Exit and clicks Finish to complete the wizard. |