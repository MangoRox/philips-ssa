using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class PicCredsWorkflow : IWorkflowStep
    {
        public string Name => "Factory Account Passwords";

        public void Execute(Window window)
        {
            ButtonClicker.Click(window, "_btnUseDefaults", "Use Defaults");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
