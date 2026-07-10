using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class ServerConnectionWorkflow : IWorkflowStep
    {
        public string Name => "Server Connection";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}