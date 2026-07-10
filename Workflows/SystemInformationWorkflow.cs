using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class SystemInformationWorkflow : IWorkflowStep
    {
        public string Name => "System Information";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}
