using FlaUI.Core.AutomationElements;

namespace SystemSetupAutomation.Workflows
{
    internal interface IWorkflowStep
    {
        string Name { get; }
        void Execute(Window window);
    }
}
