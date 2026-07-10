using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class SQLServerConnectionWorkflow : IWorkflowStep
    {
        public string Name => "SQL Server Connection";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}