using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Configuration;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class TopologyWorkflow : IWorkflowStep
    {
        private const int MaxEditDialogAttempts = 3;

        private readonly SetupConfiguration _config;

        public string Name => "Topology";

        public TopologyWorkflow(SetupConfiguration config)
        {
            _config = config;
        }

        public void Execute(Window window)
        {
            window.SetForeground();
            window.Focus();
            window.Click();

            if (!_config.TopologyItemNameChange)
            {
                Console.WriteLine("Topology item name change is disabled. Skipping.");
                return;
            }

            var topologyGroupHeader = window
                .FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Group).And(cf.ByName("Host")));

            if (topologyGroupHeader is null)
            {
                Console.WriteLine("ERROR: could not find element with ControlType 'Group' and Name 'Host'.");
                return;
            }

            var topologyItems = topologyGroupHeader
                .FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem));

            var topologyEditButton = window
                .FindFirstDescendant(cf =>
                    cf.ByName("Edit...").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            if (topologyEditButton is null)
            {
                Console.WriteLine("ERROR: could not find element with Name 'Edit...'.");
                return;
            }

            foreach (var item in topologyItems)
            {
                RenameTopologyItem(window, item, topologyEditButton);
            }
        }

        private static void RenameTopologyItem(Window window, AutomationElement item, Button editButton)
        {
            Console.WriteLine("Changing Display Name for topology item: {0}", item.Name);
            var listItem = item.AsListBoxItem();
            listItem.Select();
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Window? editWindow = null;
            for (var attempt = 1; attempt <= MaxEditDialogAttempts && editWindow is null; attempt++)
            {
                editButton.Focus();
                Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);
                Console.WriteLine("Selected item: {0} (edit attempt {1}).", item.Name, attempt);

                Thread.Sleep(TimeSpan.FromSeconds(1));
                editWindow = Retry.WhileNull(
                    () => window.ModalWindows.FirstOrDefault(),
                    timeout: TimeSpan.FromSeconds(5),
                    interval: TimeSpan.FromSeconds(1)).Result;

                if (editWindow is null && attempt < MaxEditDialogAttempts)
                {
                    Console.WriteLine("Edit dialog did not appear for '{0}'. Retrying...", item.Name);
                }
            }

            if (editWindow is null)
            {
                Console.WriteLine("ERROR: edit dialog did not appear for '{0}' after {1} attempts.", item.Name, MaxEditDialogAttempts);
                return;
            }

            ApplyDisplayNameFromInternalName(editWindow, item.Name);
        }

        private static void ApplyDisplayNameFromInternalName(Window editWindow, string itemName)
        {
            var internalNameField = editWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtInternalName"))
                ?.AsTextBox();

            if (internalNameField is null)
            {
                Console.WriteLine("ERROR: could not find '_txtInternalName' in the edit dialog.");
                editWindow.Close();
                return;
            }

            var internalName = internalNameField.Text;

            var displayNameField = editWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtDisplayName"))
                ?.AsTextBox();

            if (displayNameField is null)
            {
                Console.WriteLine("ERROR: could not find '_txtDisplayName' in the edit dialog.");
                editWindow.Close();
                return;
            }

            displayNameField.Text = internalName;
            Console.WriteLine("Set Display Name to '{0}' for topology item '{1}'.", internalName, itemName);

            var okButton = editWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_btnOk"))
                ?.AsButton();

            if (okButton is null)
            {
                Console.WriteLine("ERROR: could not find '_btnOk' in the edit dialog.");
                editWindow.Close();
                return;
            }

            okButton.Invoke();
            Console.WriteLine("OK button clicked.");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
