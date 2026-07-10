using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LanguageSettingsWorkflow : IWorkflowStep
    {
        public string Name => "Language Settings";

        public void Execute(Window window)
        {
            // no configuration required for this page
        }
    }
}
