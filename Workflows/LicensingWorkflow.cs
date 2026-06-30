using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Configuration;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LicensingWorkflow
    {
        private readonly Window _window;
        private readonly SetupConfiguration _config;

        public LicensingWorkflow(Window window, SetupConfiguration config)
        {
            _window = window;
            _config = config;
        }

        public void Execute()
        {
            if (_config.HostLicensingConfiguration is null)
            {
                Console.WriteLine("No host licensing configuration found. Skipping.");
                return;
            }

            foreach (var (hostName, tabs) in _config.HostLicensingConfiguration)
            {
                SelectHost(hostName);
                ConfigureHostTabs(tabs);
                SaveLicensing();
            }
        }

        private void SelectHost(string hostName)
        {
            var hostTreeItem = _window
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

        private void ConfigureHostTabs(Dictionary<string, Dictionary<string, LicenseOption>> tabs)
        {
            foreach (var (tabName, options) in tabs)
            {
                SelectTab(tabName);

                foreach (var (optionName, licenseOption) in options)
                {
                    ConfigureLicenseCheckbox(optionName, licenseOption);
                }
            }
        }

        private void SelectTab(string tabName)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var tabItem = _window
                .FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.TabItem).And(cf.ByName(tabName)))
                ?.AsTabItem();

            if (tabItem?.IsSelected == false)
            {
                tabItem.Select();
                Console.WriteLine("Selected '{0}' tab.", tabName);
            }
        }

        private void ConfigureLicenseCheckbox(string optionName, LicenseOption licenseOption)
        {
            var checkboxName = $"Technical Option Row {licenseOption.Row}";

            var checkBox = _window
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

        private void SaveLicensing()
        {
            var saveButton = _window
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
