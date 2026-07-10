using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class DatabaseInstallationWorkflow : IWorkflowStep
    {
        public string Name => "Database Installation";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}