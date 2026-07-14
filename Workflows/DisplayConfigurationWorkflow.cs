using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class DisplayConfigurationWorkflow : IWorkflowStep
    {
        public string Name => "Display Configuration";

        public void Execute(Window window)
        {
            // No configuration currently needed for this page 
        }
    }
}