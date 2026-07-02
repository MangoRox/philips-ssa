using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class NavigationStep : IWorkflowStep
    {
        private readonly int _times;
        private readonly int _delaySeconds;

        public string Name => $"Navigate (Next x{_times})";

        public NavigationStep(int times = 1, int delaySeconds = 1)
        {
            _times = times;
            _delaySeconds = delaySeconds;
        }

        public void Execute(Window window)
        {
            ButtonClicker.Click(window, "_btnNext", "Next", times: _times);
            Thread.Sleep(TimeSpan.FromSeconds(_delaySeconds));
        }
    }
}
