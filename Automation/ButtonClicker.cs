using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Automation
{
    internal sealed class ButtonClicker
    {
        private const int MaxEnableAttempts = 40;
        private static readonly TimeSpan EnableCheckInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PostEnableDelay = TimeSpan.FromSeconds(1);

        public static void Click(Window window, string automationId, string? displayName = null, int times = 1)
        {
            for (var click = 1; click <= times; click++)
            {
                var button = window.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button)
                        .And(cf.ByAutomationId(automationId)))
                    ?.AsButton();

                if (button is null)
                {
                    Console.WriteLine(
                        "ERROR: could not find button with AutomationId '{0}'.",
                        automationId);
                    return;
                }

                WaitForEnabled(button, displayName ?? automationId);

                try
                {
                    button.Invoke();
                }
                catch (FlaUI.Core.Exceptions.ElementNotAvailableException)
                {
                    Console.WriteLine(
                        "ERROR: '{0}' button is no longer available. Continuing execution.",
                        displayName ?? automationId);
                    return;
                }

                Console.WriteLine("'{0}' button clicked.", displayName ?? automationId);
            }
        }

        public static void WaitUntilButtonEnabled(Window window, string automationId, string? displayName = null)
        {
            var button = window.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Button)
                    .And(cf.ByAutomationId(automationId)))
                ?.AsButton();

            if (button is null)
            {
                Console.WriteLine(
                    "ERROR: could not find button with AutomationId '{0}'.",
                    automationId);
                return;
            }

            WaitForEnabled(button, displayName ?? automationId);
        }

        public static void Click(Button button, int times = 1)
        {
            for (var click = 1; click <= times; click++)
            {
                WaitForEnabled(button, button.Name);

                try
                {
                    button.Invoke();
                }
                catch (FlaUI.Core.Exceptions.ElementNotAvailableException)
                {
                    Console.WriteLine(
                        "ERROR: '{0}' button is no longer available. Continuing execution.",
                        button.Name);
                    return;
                }

                Console.WriteLine("'{0}' button clicked.", button.Name);
            }
        }

        private static void WaitForEnabled(Button button, string displayName)
        {
            for (var attempt = 1; attempt <= MaxEnableAttempts; attempt++)
            {
                if (button.IsEnabled)
                {
                    Console.WriteLine("'{0}' button is enabled after {1} attempt(s).", displayName, attempt);
                    Thread.Sleep(PostEnableDelay);
                    return;
                }

                Console.WriteLine(
                    "Attempt {0}: '{1}' button is still disabled. Retrying...",
                    attempt,
                    displayName);
                Thread.Sleep(EnableCheckInterval);
            }
        }
    }
}
