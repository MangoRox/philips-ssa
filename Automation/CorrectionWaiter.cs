using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Automation
{
    internal sealed class CorrectionWaiter
    {
        public static void WaitForCompletion(
            Window window,
            string contextLabel,
            int maxAttempts = 20,
            int intervalSeconds = 15)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(intervalSeconds));

                var toolStrip = window.FindFirstDescendant(cf => cf.ByAutomationId("_toolStrip"));
                var inProgressButton = toolStrip?
                    .FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                    .FirstOrDefault(btn => string.IsNullOrEmpty(btn.Name));

                if (inProgressButton is null)
                {
                    Console.WriteLine(
                        "{0} complete after {1} seconds.",
                        contextLabel,
                        attempt * intervalSeconds);
                    return;
                }

                Console.WriteLine(
                    "{0} in progress... {1} seconds elapsed.",
                    contextLabel,
                    attempt * intervalSeconds);
            }
        }
    }
}
