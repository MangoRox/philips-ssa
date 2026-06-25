using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.Core.Input;
using FlaUI.Core.Conditions;
using System.Security.Cryptography.X509Certificates;


namespace SystemSetupAutomation
{
    internal class Program
    {
        const string processName = "Philips.PMP.SystemSetupHost.exe";
        const string postLoginWindowTitle = "PIC iX System Setup";
        const int maxWindowLookupAttempts = 18;
        const int postLoginWindowLookupAttempts = 6;
        static readonly TimeSpan windowLookupInterval = TimeSpan.FromSeconds(5);
        static readonly TimeSpan postLoginWindowLookupInterval = TimeSpan.FromSeconds(5);
        static AutomationElement? desktop;
        // Helper method to click a button with retries if the button is not enabled    
        static void clickButton(int freq, Window? window, string automationId, string? name = null)
        {
            for (var nextButtonClicks = 1; nextButtonClicks <= freq; nextButtonClicks++)
            {
                if (window is null)
                {
                    Console.WriteLine("ERROR: main window reference is null.");
                    return;
                }
                Button? buttonQuery;
                if (!string.IsNullOrEmpty(name))
                {
                    buttonQuery = window?.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Button)
                        .And(cf.ByAutomationId(automationId))
                        )?.AsButton();
                }
                else
                {
                    buttonQuery = window?.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Button)
                        .And(cf.ByAutomationId(automationId))
                        )?.AsButton();
                }
                if (buttonQuery is null)
                {
                    Console.WriteLine($"ERROR: could not find button with AutomationId '{automationId}' and Name '{name}'");
                    return;
                }
                // If the button is found but not enabled, wait and retry a few times before giving up 
                for (var attempt = 1; attempt <= 6; attempt++)
                {
                    if (buttonQuery.IsEnabled)
                    {
                        Console.WriteLine($"{name} button is enabled after {attempt} attempts.");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    Console.WriteLine($"Attempt {attempt}: {name} button is still disabled. Retrying...");
                }
                try {
                buttonQuery.Invoke();
                }
                catch (FlaUI.Core.Exceptions.ElementNotAvailableException)
                {
                    Console.WriteLine($"ERROR: {name} button is no longer available. It may have been closed or the window may have changed. Continuing execution.");
                    return;
                }
                Console.WriteLine($"{name} button clicked.");
            }
        }
        
        static string ClassifyScreenshot(string screenshotPath)
        {
            Console.WriteLine($"Classifying screenshot '{screenshotPath}'...");
            using var bgr = OpenCvSharp.Cv2.ImRead(screenshotPath, OpenCvSharp.ImreadModes.Color);
            if (bgr.Empty())
            {
                Console.WriteLine(
                    $"ERROR: could not load screenshot '{screenshotPath}' into OpenCV."
                );
                return "unknown";
            }

            using var hsv = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.CvtColor(bgr, hsv, OpenCvSharp.ColorConversionCodes.BGR2HSV);

            int redPixels = 0,
                greenPixels = 0,
                yellowPixels = 0;
            var indexer = hsv.GetGenericIndexer<OpenCvSharp.Vec3b>();
            for (int row = 0; row < hsv.Rows; row++)
            {
                for (int col = 0; col < hsv.Cols / 4; col++) // we only care about the left quarter
                {
                    var pixel = indexer[row, col];
                    int h = pixel.Item0;
                    int s = pixel.Item1;
                    int v = pixel.Item2;

                    if (s < 50 || v < 60)
                        continue;

                    // Hue in OpenCV is 0-179. Red wraps (0-10 and 160-179).
                    if (h <= 10)
                        redPixels++;
                    else if (h >= 20 && h <= 30)
                        yellowPixels++;
                    else if (h >= 55 && h <= 60)
                        greenPixels++;
                }
            }

            Console.WriteLine($"red={redPixels} yellow={yellowPixels} green={greenPixels}");

            int best = Math.Max(redPixels, Math.Max(yellowPixels, greenPixels));
            if (best == 0)
                return "unknown";

            if (best == greenPixels)
                return "success";
            if (best == yellowPixels)
                return "warning";
            return "error";
        }
        static void Main(string[] args)
        {
            // Attempt to attach to the process and find the main window, with retries if necessary
            Console.WriteLine($"Attempting to attach to process '{processName}'...");
            using var automation = new UIA3Automation();
            var app = Application.Attach(processName);
            Console.WriteLine($"Successfully attached to process '{processName}' (PID: {app.ProcessId}).");

            // Look for the main window of the application, with retries if necessary
            desktop = automation.GetDesktop();
            Window? mainWindow = null;

            for (var attempt = 1; attempt <= maxWindowLookupAttempts && mainWindow is null; attempt++)
            {
                mainWindow = desktop
                    .FindFirstChild(cf => cf.ByProcessId(app.ProcessId))
                    ?.AsWindow();

                if (mainWindow is not null)
                {
                    break;
                }

                if (attempt < maxWindowLookupAttempts)
                {
                    Console.WriteLine($"Top-level window not found for PID {app.ProcessId}. Retrying in {windowLookupInterval.TotalSeconds:0} seconds ({attempt}/{maxWindowLookupAttempts})...");
                    Thread.Sleep(windowLookupInterval);
                }
            }

            if (mainWindow is null)
            {
                Console.WriteLine($"ERROR: no top-level window found for PID {app.ProcessId} after {maxWindowLookupAttempts} attempts over {windowLookupInterval.TotalMinutes * maxWindowLookupAttempts:0} minutes.");
                return;
            }
            Console.WriteLine($"Attached to: {mainWindow.Title}");

            // Enter credentials and click OK
            var usernameField = mainWindow?
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtUserName"))
                ?.AsTextBox();

            if (usernameField is null)
            {
                Console.WriteLine("ERROR: could not find element with AutomationId '_txtUserName'");
                return;
            }

            usernameField.Text = "PhilipsBD";
            Console.WriteLine("Username field updated.");

            var passwordField = mainWindow?
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtPassword"))
                ?.AsTextBox();

            if (passwordField is null)
            {
                Console.WriteLine("ERROR: could not find element with AutomationId '_txtPassword'");
                return;
            }

            passwordField.Text = "BK|Sup42p0rt!";
            Console.WriteLine("Password field updated.");

            clickButton(1, mainWindow!, "_btnOk");

            Window? postLoginWindow = null;

            // Login window exits

            // Wait for System setup window to appear and attach
            for (var attempt = 1; attempt <= postLoginWindowLookupAttempts && postLoginWindow is null; attempt++)
            {
                postLoginWindow = desktop
                    .FindAllChildren(cf => cf.ByProcessId(app.ProcessId))
                    .Select(element => element.AsWindow())
                    .FirstOrDefault(window => window.Title.StartsWith(postLoginWindowTitle, StringComparison.OrdinalIgnoreCase));

                if (postLoginWindow is not null)
                {
                    break;
                }

                if (attempt < postLoginWindowLookupAttempts)
                {
                    Console.WriteLine($"Window '{postLoginWindowTitle}' not found for PID {app.ProcessId}. Retrying in {postLoginWindowLookupInterval.TotalSeconds:0} seconds ({attempt}/{postLoginWindowLookupAttempts})...");
                    Thread.Sleep(postLoginWindowLookupInterval);
                }
            }

            if (postLoginWindow == null)
            {
                Console.WriteLine($"ERROR: window '{postLoginWindowTitle}' not found for PID {app.ProcessId} after {postLoginWindowLookupAttempts} attempts over {postLoginWindowLookupInterval.TotalSeconds * postLoginWindowLookupAttempts:0} seconds.");
                return;
            }

            Console.WriteLine($"Found window: {postLoginWindow.Title}");

            // hit next multiple times to go thru the menus
            clickButton(7, postLoginWindow, "_btnNext");
            postLoginWindow.SetForeground();
            postLoginWindow.Focus();
            postLoginWindow.Click(); 

            var topologyGroupHeader = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Group).And(cf.ByName("Host")));
            if (topologyGroupHeader is null)
            {
                Console.WriteLine("ERROR: could not find element with ControlType 'Group' and Name 'Host'");
                return;
            }

            var topologyItems = topologyGroupHeader
                .FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem));

            var topologyEditButton = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByName("Edit...").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();
            if (topologyEditButton is null)
            {
                Console.WriteLine("ERROR: could not find element with Name 'Edit...'");
                return;
            }

            foreach (var item in topologyItems)
            {
                Console.WriteLine($"Changing Display Name for topology item: {item.Name}");
                var listItemItem = item.AsListBoxItem();
                listItemItem.Select();
                Thread.Sleep(TimeSpan.FromSeconds(1));
         
                Window? editWindow = null;
                for (var editAttempt = 1; editAttempt <= 3 && editWindow is null; editAttempt++)
                {
                    topologyEditButton.Focus();
                    Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);
                    Console.WriteLine($"Selected item: {item.Name} (edit attempt {editAttempt}).");

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    editWindow = Retry.WhileNull(
                        () => postLoginWindow?.ModalWindows.FirstOrDefault(),
                        timeout: TimeSpan.FromSeconds(5),
                        interval: TimeSpan.FromSeconds(1)
                    ).Result;

                    if (editWindow is null && editAttempt < 3)
                    {
                        Console.WriteLine($"Edit dialog did not appear for '{item.Name}'. Retrying...");
                    }
                }

                if (editWindow is null)
                {
                    Console.WriteLine($"ERROR: edit dialog did not appear for '{item.Name}' after 3 attempts.");
                    continue;
                }
                var internalNameField = editWindow
                    .FindFirstDescendant(cf => cf.ByAutomationId("_txtInternalName"))
                    ?.AsTextBox();
                if (internalNameField is null)
                {
                    Console.WriteLine("ERROR: could not find element with AutomationId '_txtInternalName' in the edit dialog.");
                    editWindow.Close();
                    continue;
                }
                var internalName = internalNameField.Text;
                var displayNameField = editWindow
                    .FindFirstDescendant(cf => cf.ByAutomationId("_txtDisplayName"))
                    ?.AsTextBox();
                if (displayNameField is null)
                {
                    Console.WriteLine("ERROR: could not find element with AutomationId '_txtDisplayName' in the edit dialog.");
                    editWindow.Close();
                    continue;
                }
                displayNameField.Text = internalName;
                Console.WriteLine($"Set Display Name to '{internalName}' for topology item '{item.Name}'.");
                var okButton = editWindow
                    .FindFirstDescendant(cf => cf.ByAutomationId("_btnOk"))
                    ?.AsButton();
                if (okButton is null)
                {
                    Console.WriteLine("ERROR: could not find element with AutomationId '_btnOk' in the edit dialog.");
                    editWindow.Close();
                    continue;
                }
                okButton.Invoke();
                Console.WriteLine("OK button clicked.");
                Thread.Sleep(TimeSpan.FromSeconds(1)); // wait a bit for the dialog to close before proceeding to the next item
            }

            clickButton(2, postLoginWindow, "_btnNext");
            Thread.Sleep(TimeSpan.FromSeconds(2)); // wait a bit for the next page to load
            // Host Licensing
            // select host
            var hostnameInLicencing = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.TreeItem).And(cf.ByName("nrtWEB-1")))
                ?.AsTreeItem();
            if (hostnameInLicencing is null) {
                Console.WriteLine("ERROR: could not find tree item with Name 'nrtWEB-1' in the licensing page.");
                return;
            }
            if (!hostnameInLicencing.IsSelected)
                hostnameInLicencing.Select();
            Console.WriteLine("Selected host 'nrtWEB-1' in licensing page.");

            var tabItem = postLoginWindow?.FindFirstDescendant(
                cf => cf.ByControlType(ControlType.TabItem)
                .And(cf.ByName("Host")))
                ?.AsTabItem();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // wait a bit for the tab to be ready for interaction
            if (tabItem?.IsSelected == false) {
                tabItem?.Select();
                Console.WriteLine("Selected 'Host' tab.");
            };
            var checkBox = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByName("Technical Option Row 8").And(cf.ByControlType(ControlType.CheckBox)))
                ?.AsCheckBox();
            if (checkBox is null) {
                Console.WriteLine("ERROR: could not find checkbox with Name 'Technical Option Row 8' in the Host tab of the licensing page.");
                return;
            }
            Console.WriteLine($"Found checkbox 'Technical Option Row 8'. Current state: {(checkBox.IsToggled == true ? "Checked" : "Unchecked")}.");
            if (checkBox?.IsToggled == false)
            {
                checkBox?.Toggle();
                Console.WriteLine("Checked 'Technical Option Row 8' checkbox.");
            }
            else {
                Console.WriteLine("'Technical Option Row 8' checkbox is already checked."); 
            }
            var hostLicensingSaveButton = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByName("Save").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();
            hostLicensingSaveButton?.Invoke();
            Console.WriteLine("Clicked 'Save' button on licensing page.");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            clickButton(1, postLoginWindow, "_btnNext");
            // System Encryption Combination
            // clickButton(1, postLoginWindow, "_btnDefault"); // NOTE: UNCOMMENT AFTER TESTING DONE
            clickButton(1, postLoginWindow, "_btnNext");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            // Factory Account Passwords
            clickButton(1, postLoginWindow, "_btnUseDefaults");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            clickButton(1, postLoginWindow, "_btnNext");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            // Peripheral Configuration
            clickButton(1, postLoginWindow, "_btnNext");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            // Platform Security
            //  start correction process
            var platformSecurityCorrectButton = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByName("Correct").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();
            if (platformSecurityCorrectButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Correct' button on Platform Security page.");
                return;
            }
            platformSecurityCorrectButton.Invoke();
            //  wait for completion
            for (var attempt = 1; attempt <= 20; attempt++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(15));

                var toolStrip = postLoginWindow?.FindFirstDescendant(cf => cf.ByAutomationId("_toolStrip"));
                var inProgressButton = toolStrip?
                    .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                    .FirstOrDefault(btn => string.IsNullOrEmpty(btn.Name));

                if (inProgressButton is null)
                {
                    Console.WriteLine($"Platform Security correction complete after {attempt * 15} seconds.");
                    break;
                }

                Console.WriteLine($"Correction in progress... {attempt * 15} seconds elapsed");
            }
            // start correction process 
            var securityItems = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByAutomationId("_lvwQualificationItems"))
                ?.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem))
                .Select(item => item.AsListBoxItem())
                .ToList();
            if (securityItems is null)
            {
                Console.WriteLine("ERROR: could not find qualification items list.");
                return;
            }
            var platformSecurityOutputFolder = Path.Combine(Environment.CurrentDirectory, "Platform_Security_Items");
            Directory.CreateDirectory(platformSecurityOutputFolder);
            bool isPlatformSecurityFullyCorrect = false;
            while (!isPlatformSecurityFullyCorrect)
            {
                isPlatformSecurityFullyCorrect = true;
                for (var i = 0; i < securityItems.Count; i++)
                {
                    var item = securityItems[i];
                    item.ScrollIntoView();
                    var fileName = $"{i + 1}-{item.Name}.png";
                    var filePath = Path.Combine(platformSecurityOutputFolder, fileName);
                    using var bitmap = item.Capture();
                    bitmap.Save(filePath);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    var status = ClassifyScreenshot(filePath);
                    Console.WriteLine(
                        $"Saved screenshot of '{item.Name}' to '{filePath}' — status: {status}."
                    );
                    if (status != "success")
                    {
                        isPlatformSecurityFullyCorrect = false;
                    }
                }
                // rerun platform security to attempt to correct warning/error items
                if (!isPlatformSecurityFullyCorrect)
                {
                    platformSecurityCorrectButton.Invoke();
                    for (int attempt = 1; attempt <= 30; attempt++)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(20));

                        var toolStrip = postLoginWindow?.FindFirstDescendant(cf => cf.ByAutomationId("_toolStrip"));
                        var inProgressButton = toolStrip?
                            .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                            .FirstOrDefault(btn => string.IsNullOrEmpty(btn.Name));

                        if (inProgressButton is null)
                        {
                            Console.WriteLine($"Platform security correction complete after {attempt * 20} seconds.");
                            break;
                        }

                        Console.WriteLine($"Attempt {attempt}: Platform security correction in progress... {attempt * 20} seconds elapsed");
                    }
                }
            }
            clickButton(1, postLoginWindow, "_btnNext", "Next");
            // Host Qualification
            // auto-starts so wait for it to complete
            for (var attempt = 1; attempt <= 20; attempt++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(15));

                var toolStrip = postLoginWindow?.FindFirstDescendant(cf => cf.ByAutomationId("_toolStrip"));
                var inProgressButton = toolStrip?
                    .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                    .FirstOrDefault(btn => string.IsNullOrEmpty(btn.Name));

                if (inProgressButton is null)
                {
                    Console.WriteLine($"Host Qualification correction complete after {attempt * 15} seconds.");
                    break;
                }

                Console.WriteLine($"Host Qualification correction in progress... {attempt * 15} seconds elapsed");
            }
            // check for any warning/error items in the list 
            //   get correct button which may be used to correct any warning/error items in the list after check
            var hostQualificationCorrectButton = postLoginWindow?
                .FindFirstDescendant(cf =>
                    cf.ByName("Correct").And(cf.ByControlType(ControlType.Button))
                )
                ?.AsButton();
            if (hostQualificationCorrectButton is null)
            {
                Console.WriteLine("ERROR: could not find host qualification correct button.");
                return;
            }
            else
                Console.WriteLine("Found host qualification correct button");

            //  get all listitems from the List _lvwQualificationItems for processing
            var qualificationItems = postLoginWindow?
                .FindFirstDescendant(cf => cf.ByAutomationId("_lvwQualificationItems"))
                ?.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem))
                .Select(item => item.AsListBoxItem())
                .ToList();
            if (qualificationItems is null)
            {
                Console.WriteLine("ERROR: could not find qualification items list.");
                return;
            }

            var hostQualificationOutputFolder = Path.Combine(Environment.CurrentDirectory, "Qualification_Items");
            Directory.CreateDirectory(hostQualificationOutputFolder);

            bool isHostQualificationFullyCorrect = false;
            while (!isHostQualificationFullyCorrect)
            {
                isHostQualificationFullyCorrect = true;
                for (var i = 0; i < qualificationItems.Count; i++)
                {
                    var item = qualificationItems[i];
                    if (
                        item.Name.Contains("Windows Activation")
                        || item.Name.Contains("Link Speed and Duplex")
                    )
                    {
                        var itemTogglePattern = item.Patterns.Toggle.Pattern;
                        var itemToggleState = itemTogglePattern.ToggleState.Value;
                        if (itemToggleState == ToggleState.On)
                        {
                            Console.WriteLine(
                                $"Skipping '{item.Name}'"
                            );
                            itemTogglePattern.Toggle();
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            while (itemTogglePattern.ToggleState.Value != ToggleState.Off)
                            {
                                itemTogglePattern.Toggle();
                                Console.WriteLine($"Waiting for '{item.Name}' to toggle off...");
                                Thread.Sleep(TimeSpan.FromSeconds(1));
                            }
                        }
                        continue;
                    }
                    item.ScrollIntoView();
                    var fileName = $"{i + 1}-{item.Name}.png";
                    var filePath = Path.Combine(hostQualificationOutputFolder, fileName);
                    using var bitmap = item.Capture();
                    bitmap.Save(filePath);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    var status = ClassifyScreenshot(filePath);
                    Console.WriteLine(
                        $"Saved screenshot of '{item.Name}' to '{filePath}' — status: {status}."
                    );
                    if (status == "error")
                    {
                        isHostQualificationFullyCorrect = false;
                    }
                }
                // rerun host qualification to attempt to correct warning/error items
                if (!isHostQualificationFullyCorrect)
                {
                    hostQualificationCorrectButton.Invoke();
                    for (int attempt = 1; attempt <= 30; attempt++)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(20));

                        var toolStrip = postLoginWindow?.FindFirstDescendant(cf => cf.ByAutomationId("_toolStrip"));
                        var inProgressButton = toolStrip?
                            .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                            .FirstOrDefault(btn => string.IsNullOrEmpty(btn.Name));

                        if (inProgressButton is null)
                        {
                            Console.WriteLine($"Host qualification correction complete after {attempt * 20} seconds.");
                            break;
                        }

                        Console.WriteLine($"Attempt {attempt}: Host qualification correction in progress... {attempt * 20} seconds elapsed");
                    }
                }
            }
            
            postLoginWindow?.SetForeground();
            postLoginWindow?.Focus();
            postLoginWindow?.Click();
    
            clickButton(1, postLoginWindow, "_btnNext", "Next");

            Window? messageBoxWindow = null;
            Thread.Sleep(TimeSpan.FromSeconds(3));
            for (var attempt = 1; attempt <= 2 && messageBoxWindow is null; attempt++)
            {
                messageBoxWindow = postLoginWindow?
                    .FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Window)
                            .And(cf.ByName("Patient Information Center iX"))
                    )
                    ?.AsWindow();
                if (messageBoxWindow is null && attempt <= 2)
                {
                    Console.WriteLine($"Attempt {attempt}/2: 'Patient Information Center iX' message box not found. Retrying...");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            if (messageBoxWindow is not null)
            {
                Console.WriteLine("Found 'Patient Information Center iX' message box.");
                clickButton(1, messageBoxWindow, "_btnLeft", "Yes");
            }
            else
            {
                Console.WriteLine("'Patient Information Center iX' message box did not appear. Continuing...");
            }

            var exitRadioButton = postLoginWindow?
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_rdbExit").And(cf.ByControlType(ControlType.RadioButton))
                )
                ?.AsRadioButton();
            if (exitRadioButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Exit' radio button.");
                return;
            }
            exitRadioButton.Patterns.Invoke.Pattern.Invoke();
            Console.WriteLine("Exit radio button selected.");

            clickButton(1, postLoginWindow, "_btnFinish", "Finish");
        }
    }
}