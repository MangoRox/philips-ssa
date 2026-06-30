using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class EncryptionWorkflow
    {
        private readonly Window _window;

        public EncryptionWorkflow(Window window)
        {
            _window = window;
        }

        public void Execute()
        {
            ButtonClicker.Click(_window, "_btnUseDefaults", "Use Defaults");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
