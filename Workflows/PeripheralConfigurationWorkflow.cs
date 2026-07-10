using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class PeripheralConfigurationWorkflow : IWorkflowStep
    {
        public string Name => "Peripheral Configuration";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}