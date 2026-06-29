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
            // System Encryption Combination — advance past it
            ButtonClicker.Click(_window, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Factory Account Passwords — use defaults
            ButtonClicker.Click(_window, "_btnUseDefaults", "Use Defaults");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            ButtonClicker.Click(_window, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Peripheral Configuration — advance past it
            ButtonClicker.Click(_window, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
