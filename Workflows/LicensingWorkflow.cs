using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Configuration;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LicensingWorkflow : IWorkflowStep
    {
        private readonly SetupConfiguration _config;

        public string Name => "Licensing";

        public LicensingWorkflow(SetupConfiguration config)
        {
            _config = config;
        }

        public void Execute(Window window)
        {
            if (_config.HostLicensingConfiguration is null)
            {
                Console.WriteLine("No host licensing configuration found. Skipping.");
                return;
            }

            foreach (var (hostName, tabs) in _config.HostLicensingConfiguration)
            {
                SelectHost(window, hostName);
                ConfigureHostTabs(window, tabs);
                SaveLicensing(window);
            }
        }

        private static void SelectHost(Window window, string hostName)
        {
            var hostTreeItem = window
                .FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.TreeItem).And(cf.ByName(hostName)))
                ?.AsTreeItem();

            if (hostTreeItem is null)
            {
                Console.WriteLine("ERROR: could not find tree item with Name '{0}'.", hostName);
                return;
            }

            if (!hostTreeItem.IsSelected)
            {
                hostTreeItem.Select();
            }

            Console.WriteLine("Selected host '{0}' in licensing page.", hostName);
        }

        private static void ConfigureHostTabs(Window window, Dictionary<string, Dictionary<string, LicenseOption>> tabs)
        {
            foreach (var (tabName, options) in tabs)
            {
                SelectTab(window, tabName);

                foreach (var (optionName, licenseOption) in options)
                {
                    ConfigureLicenseCheckbox(window, optionName, licenseOption);
                }
            }
        }

        private static void SelectTab(Window window, string tabName)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var tabItem = window
                .FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.TabItem).And(cf.ByName(tabName)))
                ?.AsTabItem();

            if (tabItem?.IsSelected == false)
            {
                tabItem.Select();
                Console.WriteLine("Selected '{0}' tab.", tabName);
            }
        }

        private static void ConfigureLicenseCheckbox(Window window, string optionName, LicenseOption licenseOption)
        {
            var checkboxName = $"Technical Option Row {licenseOption.Row}";

            var checkBox = window
                .FindFirstDescendant(cf =>
                    cf.ByName(checkboxName).And(cf.ByControlType(ControlType.CheckBox)))
                ?.AsCheckBox();

            if (checkBox is null)
            {
                Console.WriteLine("ERROR: could not find checkbox '{0}' for option '{1}'.", checkboxName, optionName);
                return;
            }

            bool isCurrentlyChecked = checkBox.IsToggled == true;
            Console.WriteLine("Found checkbox '{0}' ({1}). Current state: {2}.",
                checkboxName,
                optionName,
                isCurrentlyChecked ? "Checked" : "Unchecked");

            if (licenseOption.Enabled && !isCurrentlyChecked)
            {
                checkBox.Toggle();
                Console.WriteLine("Checked '{0}' ({1}).", checkboxName, optionName);
            }
            else if (!licenseOption.Enabled && isCurrentlyChecked)
            {
                checkBox.Toggle();
                Console.WriteLine("Unchecked '{0}' ({1}).", checkboxName, optionName);
            }
            else
            {
                Console.WriteLine("No change needed for '{0}' ({1}).", checkboxName, optionName);
            }
        }

        private static void SaveLicensing(Window window)
        {
            var saveButton = window
                .FindFirstDescendant(cf =>
                    cf.ByName("Save").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            if (saveButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Save' button on licensing page.");
                return;
            }

            if (saveButton.IsEnabled)
            {
                saveButton.Invoke();
                Console.WriteLine("Clicked 'Save' button on licensing page.");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            else
            {
                Console.WriteLine("Save button is disabled. No changes to save.");
            }
        }
    }
}
