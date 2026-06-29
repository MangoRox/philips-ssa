using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class FinalizationWorkflow
    {
        private const int MessageBoxMaxAttempts = 2;

        private readonly Window _window;

        public FinalizationWorkflow(Window window)
        {
            _window = window;
        }

        public void Execute()
        {
            HandleConfirmationMessageBox();
            SelectExitAndFinish();
        }

        private void HandleConfirmationMessageBox()
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));

            Window? messageBoxWindow = null;
            for (var attempt = 1; attempt <= MessageBoxMaxAttempts && messageBoxWindow is null; attempt++)
            {
                messageBoxWindow = _window
                    .FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Window)
                            .And(cf.ByName("Patient Information Center iX")))
                    ?.AsWindow();

                if (messageBoxWindow is null && attempt < MessageBoxMaxAttempts)
                {
                    Console.WriteLine(
                        "Attempt {0}/{1}: 'Patient Information Center iX' message box not found. Retrying...",
                        attempt,
                        MessageBoxMaxAttempts);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }

            if (messageBoxWindow is not null)
            {
                Console.WriteLine("Found 'Patient Information Center iX' message box.");
                ButtonClicker.Click(messageBoxWindow, "_btnLeft", "Yes");
            }
            else
            {
                Console.WriteLine("'Patient Information Center iX' message box did not appear. Continuing...");
            }
        }

        private void SelectExitAndFinish()
        {
            var exitRadioButton = _window
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_rdbExit").And(cf.ByControlType(ControlType.RadioButton)))
                ?.AsRadioButton();

            if (exitRadioButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Exit' radio button.");
                return;
            }

            exitRadioButton.Patterns.Invoke.Pattern.Invoke();
            Console.WriteLine("Exit radio button selected.");

            ButtonClicker.Click(_window, "_btnFinish", "Finish");
        }
    }
}
