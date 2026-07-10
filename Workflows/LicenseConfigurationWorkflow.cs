using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LicenseConfigurationWorkflow : IWorkflowStep
    {
        public string Name => "License Configuration";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}