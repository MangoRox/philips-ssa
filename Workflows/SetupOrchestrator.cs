using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class SetupOrchestrator(IReadOnlyList<IWorkflowStep> steps, Window window)
    {
        private static readonly TimeSpan PageTransitionDelay = TimeSpan.FromSeconds(5);

        private readonly Dictionary<string, IWorkflowStep> _stepMap =
            steps.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        public void Run()
        {
            var pageInfo = new SetupPageInfo(window);

            while (true)
            {
                var currentPage = pageInfo.CurrentSetupPageName();

                if (_stepMap.TryGetValue(currentPage, out var step))
                {
                    Console.WriteLine("[{0}] Executing.", currentPage);
                    step.Execute(window);
                }
                else
                {
                    Console.WriteLine("[{0}] No workflow registered. Skipping.", currentPage);
                }

                if (string.Equals(currentPage, "Finalization", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Setup complete.");
                    break;
                }

                ButtonClicker.Click(window, "_btnNext", "Next");
                Thread.Sleep(PageTransitionDelay);
                
            }
        }
    }
}
