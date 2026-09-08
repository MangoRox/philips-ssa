# HPM-SysEng-SSA — Project Summary

## 1. Project Overview & Purpose

**HPM SysEng — System Setup Automation (SSA)**: a Windows console tool that automates the **Philips PMP System Setup wizard** end-to-end using UI automation. It attaches to a running `Philips.PMP.SystemSetupHost.exe`, reads a declarative `config.json`, and drives every wizard page — login, topology, licensing, security/qualification checklists — through to "Setup Complete" without human interaction.

Target use case: unattended PIC iX system provisioning as the final leg of lab deployment. It is designed to run **alongside the Deployment Manager** (the companion `HPM-SysEng-API` repo, which lists this project in its ecosystem as "System Setup UI Automation — work in progress") and is untested outside that context. Point of contact: Steven Maynard.

What makes it interesting: where UI state can't be read reliably from automation properties (the Host Qualification / Platform Security checklists show colored status icons), it falls back to **screenshot capture + OpenCV color classification** to decide whether an item passed, warned, or failed.

## 2. Tech Stack & Dependencies

From `SysSetupAutomation.csproj`:
- **C# / .NET 8** (`net8.0-windows`), console exe, `RootNamespace: SystemSetupAutomation`.
- Published as a **self-contained single file** for `win-x64` (`SelfContained`, `PublishSingleFile`, `IncludeNativeLibrariesForSelfExtract`) — "all-in-one exe, no prerequisite programs required".
- **FlaUI.UIA3 5.0.0** — Microsoft UI Automation (UIA3) wrapper; the core automation engine.
- **OpenCvSharp4 4.10.0 (+ runtime.win)** — image processing for checklist status classification.
- `icons/**` content folder copied to output (reference images).
- Nullable reference types + implicit usings enabled.
- Runtime requirements: `Philips.PMP.SystemSetupHost.exe` running before launch; a populated `config.json` in the working directory (or `--File <path>`); if running on a non-primary host, primary services must be up and reachable.

No test framework, no CI configuration in the repo.

## 3. Architecture & File Hierarchy

```
HPM-SysEng-SSA/
├── Program.cs                      # Entry point: config load → attach → login → orchestrator
├── SysSetupAutomation.csproj
├── config.json                     # Sample/default configuration (NRT lab)
├── Config Files/config_nx11.json   # Environment-specific config (NX11 lab)
├── README.md                       # Full usage + configuration reference
├── code_guidelines.md              # C# clean-code standards ("system prompt" for AI-assisted dev)
├── Automation/                     # Reusable UIA primitives
│   ├── ProcessAttacher.cs          # Attach FlaUI to the running SystemSetupHost process
│   ├── WindowLocator.cs            # Poll for the app window by process id (retry loop)
│   ├── SetupPageInfo.cs            # Reads current page title from _bannerControl/_lblMessage
│   ├── ButtonClicker.cs            # Click/wait-until-enabled helpers keyed by AutomationId
│   └── CorrectionWaiter.cs         # Polls _toolStrip for the nameless "in progress" button
├── Configuration/
│   ├── SetupConfiguration.cs       # System.Text.Json loader + validation (PrimaryServerName required)
│   └── LicenseOption.cs            # { Enabled, Row } record for licensing grid entries
├── ImageProcessing/
│   └── ScreenshotClassifier.cs     # OpenCV HSV pixel-count classifier → success/warning/error
└── Workflows/                      # One IWorkflowStep per wizard page
    ├── IWorkflowStep.cs            # { string Name; void Execute(Window); }
    ├── SetupOrchestrator.cs        # Page-name-dispatched main loop
    ├── LoginWorkflow.cs
    ├── LanguageSettingsWorkflow.cs / SystemInformationWorkflow.cs
    ├── ServerConnectionWorkflow.cs / SQLServerConnectionWorkflow.cs
    ├── DatabaseInstallationWorkflow.cs
    ├── TopologyConfigurationWorkflow.cs
    ├── PicCredsWorkflow.cs / EncryptionWorkflow.cs
    ├── TechnicalOptionAssignmentWorkflow.cs
    ├── PlatformSecurityWorkflow.cs / HostQualificationWorkflow.cs
    ├── LicenseConfigurationWorkflow.cs / PeripheralConfigurationWorkflow.cs
    ├── DisplayConfigurationWorkflow.cs
    └── FinalizationWorkflow.cs     # "Setup Complete": Exit radio + Finish
```

**Architectural pattern: strategy/registry-driven page dispatcher.** Each wizard page is an `IWorkflowStep` keyed by its on-screen page name. `SetupOrchestrator` builds a case-insensitive `Dictionary<string, IWorkflowStep>` and loops: read current page name → execute matching step (or skip with a log line) → click Next → dismiss any modal → wait for transition → repeat until "Setup Complete". Pages with no registered workflow are skipped automatically, so the tool tolerates wizard variations.

## 4. Key Features & Code Walkthrough

### `Program.cs` (entry point)
Parses an optional `--File <path>` argument (default `config.json`), loads `SetupConfiguration` (throws if load fails or `PrimaryServerName` missing), creates a `UIA3Automation` desktop session, and attaches to `Philips.PMP.SystemSetupHost.exe` via `ProcessAttacher`. `WindowLocator.FindWindowByProcessId` polls (18 attempts × 5 s) for the first window; if its title contains "Login", `LoginWorkflow.Execute()` handles credentials and returns the post-login window. Then registers 15 workflow steps and hands off to `SetupOrchestrator.Run()`.

### `SetupOrchestrator` (`Workflows/SetupOrchestrator.cs`)
- Waits for the `_btnNext` button to be enabled (`ButtonClicker.WaitUntilButtonEnabled`) before each iteration — natural synchronization with wizard readiness.
- Reads the page name from `SetupPageInfo.CurrentSetupPageName()` (descendant lookup: pane `_bannerControl` → text `_lblMessage`).
- Executes the mapped step, then clicks Next, calls `DismissModalIfPresent()` (handles the "Patient Information Center iX" confirmation modal, 2 s check delay), and sleeps 5 s for the page transition.
- Special-cases: terminates after "Setup Complete"; skips the Next-enabled wait after "Host Qualification" because the wizard disables Next there.

### Checklist pages — `HostQualificationWorkflow` / `PlatformSecurityWorkflow` (~140 lines each)
The most complex steps. For Host Qualification:
- `CorrectionWaiter.WaitForCompletion` polls the `_toolStrip` for a nameless button that exists only while a scan/correction is in progress (default 20 attempts × 15 s; correction phase 30 × 20 s).
- Enumerates qualification checklist items, screenshots each item's status region into `Qualification_Items/`, and classifies the screenshot with `ScreenshotClassifier`.
- Items in `HostQualificationSkippableItems` (config) are ignored (e.g., "Windows Activation", "Link Speed and Duplex" — items expected to fail in lab environments).
- Loops: if any non-skipped item isn't green, clicks the "Correct" button and waits for the correction pass, re-verifying until fully corrected. Platform Security mirrors this with `PlatformSecuritySkippableItems` (e.g., "Local Security Policy").

### `ScreenshotClassifier` (`ImageProcessing/ScreenshotClassifier.cs`)
Converts the screenshot BGR→HSV, scans the left quarter of the image (`columnLimit = Cols / 4`, where the status icon lives), ignores desaturated/dark pixels (S < 50 or V < 60), and counts hue buckets: red (H ≤ 10), yellow (20–30), green (55–60). Verdict = the dominant color → `"success"` / `"warning"` / `"error"` (or `"unknown"` when zero colored pixels). This sidesteps the fact that the status icons expose no usable UIA properties.

### `TechnicalOptionAssignmentWorkflow`
Only runs on the primary server (compares `Environment.MachineName` against `PrimaryServerName`, otherwise skips). For each host in `HostLicensingConfiguration` (3-level dict: host → license group/tab → feature `{ Enabled, Row }`), it selects the host, walks the license-group tabs, toggles feature checkboxes by their 1-based grid row index, and saves. Note the naming twist: per-host licensing is configured on the *Technical Option Assignment* page; the *License Configuration* page workflow is an intentional no-op ("no configuration required for this page").

### `TopologyConfigurationWorkflow` (154 lines)
Configures server topology using `PrimaryServerName`; when `TopologyItemNameChange` is true, additionally renames topology items during the pass.

### Simple pages
`LanguageSettings`, `SystemInformation`, `ServerConnection`, `SQLServerConnection`, `DatabaseInstallation`, `PicCreds`, `Peripheral`, `Display` (~14–30 lines each) — mostly minimal interactions or intentional no-ops that exist so the orchestrator logs "Executing" rather than "Skipping", plus `EncryptionWorkflow` (72 lines) for encryption settings. `FinalizationWorkflow` selects the `_rdbExit` radio button and clicks `_btnFinish`.

### Configuration (`config.json`, `Config Files/config_nx11.json`)
Documented fully in README: `PrimaryServerName` (required), `TopologyItemNameChange`, `HostLicensingConfiguration` (host → group → feature `{Enabled, Row}`), and the two skippable-items lists. A second environment config for the NX11 lab is version-controlled under `Config Files/`.

### `code_guidelines.md`
A detailed C# engineering-standards document (naming, Allman braces, C# 10–12 features, SOLID, DI guidance) written as a system prompt for AI-assisted code review — the codebase visibly follows it (primary constructors, `sealed internal` classes, `_camelCase` fields, pattern-matching switch expressions).

## 5. Build, Run & Setup Instructions

```powershell
# Build (requires .NET 8 SDK, Windows)
dotnet build

# Publish the distributable single-file exe
dotnet publish -c Release -r win-x64
# (SelfContained/PublishSingleFile are preset in the csproj)

# Run — on the target machine, AFTER starting the PMP setup wizard:
#  1. Start Philips.PMP.SystemSetupHost.exe (the setup wizard)
#  2. Place a populated config.json next to the exe
.\SysSetupAutomation.exe
# or with an explicit config path:
.\SysSetupAutomation.exe --File "Config Files\config_nx11.json"
```

Requirements: Windows host with the PMP setup wizard running; if executing on a non-primary host, the primary server's services must be running and reachable. Screenshots of qualification items are written to `.\Qualification_Items\`. There are no automated tests. The README notes a planned releases-page distribution (zip with exe + sample config) marked TODO.

## 6. Codebase Health & Notes

- **Explicitly WIP / environment-coupled**: README warns the tool is only tested alongside Deployment Manager automation; the install section's releases link is a TODO.
- **No tests, no CI** — inherent difficulty of testing UIA flows, but the pure logic (`ScreenshotClassifier`, `SetupConfiguration`) is easily unit-testable and currently isn't.
- **Timing-based synchronization**: fixed `Thread.Sleep` delays (5 s page transition, 2 s modal check, 15–20 s poll intervals) and attempt-capped polling loops. Functional but brittle on slow systems; `CorrectionWaiter` also identifies "in progress" by *a button with an empty name* in the toolstrip — a fragile UIA heuristic.
- **Potential infinite loop**: `HostQualificationWorkflow`'s `while (!isFullyCorrected)` has no iteration cap — if an item never turns green (and isn't in the skip list), the tool retries Correct forever.
- **Naming inconsistencies**: file `TopologyConfigurationWorkflow.cs` vs. class registered as `TopologyWorkflow` in `Program.cs`; licensing config applied by `TechnicalOptionAssignmentWorkflow` while `LicenseConfigurationWorkflow` is a no-op (correct behavior, confusing names). `README` workflow-table order differs from the actual registration order in `Program.cs` (dispatch is by page name, so order is cosmetic).
- **Hue-range fragility**: the green bucket (H 55–60 in OpenCV's 0–179 hue scale) is narrow; icon theme changes would silently reclassify statuses. The `MinSaturation`/`MinValue` gates mitigate background noise.
- **Good structure for its size** (~1,440 lines of C#): clean separation of automation primitives, configuration, image processing, and per-page workflows behind a single small interface; adding a wizard page = one class + one registration line. Code style consistently matches the committed `code_guidelines.md`.
- **Config duplication**: `config.json` at root doubles as sample and default runtime config; real environment values (NRT, NX11 hostnames) are committed — fine for an internal lab tool, but worth noting.
