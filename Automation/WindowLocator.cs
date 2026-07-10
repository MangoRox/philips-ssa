using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace SystemSetupAutomation.Automation
{
    internal sealed class WindowLocator
    {
        private readonly AutomationElement _desktop;

        public WindowLocator(AutomationElement desktop)
        {
            _desktop = desktop;
        }

        public Window FindWindowByProcessId(int processId, int maxAttempts, TimeSpan interval)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var window = _desktop
                    .FindFirstChild(cf => cf.ByProcessId(processId))
                    ?.AsWindow();

                if (window is not null)
                {
                    return window;
                }

                if (attempt < maxAttempts)
                {
                    Console.WriteLine(
                        "Top-level window not found for PID {0}. Retrying in {1:0}s ({2}/{3})...",
                        processId,
                        interval.TotalSeconds,
                        attempt,
                        maxAttempts);
                    Thread.Sleep(interval);
                }
            }

            throw new InvalidOperationException(
                $"No top-level window found for PID {processId} after {maxAttempts} attempts.");
        }

        public Window FindWindowByTitle(int processId, string titlePrefix, int maxAttempts, TimeSpan interval)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var window = _desktop
                    .FindAllChildren(cf => cf.ByProcessId(processId))
                    .Select(element => element.AsWindow())
                    .FirstOrDefault(w => w.Title.StartsWith(titlePrefix, StringComparison.OrdinalIgnoreCase));

                if (window is not null)
                {
                    return window;
                }

                if (attempt < maxAttempts)
                {
                    Console.WriteLine(
                        "Window '{0}' not found for PID {1}. Retrying in {2:0}s ({3}/{4})...",
                        titlePrefix,
                        processId,
                        interval.TotalSeconds,
                        attempt,
                        maxAttempts);
                    Thread.Sleep(interval);
                }
            }

            throw new InvalidOperationException(
                $"Window '{titlePrefix}' not found for PID {processId} after {maxAttempts} attempts.");
        }
    }
}
