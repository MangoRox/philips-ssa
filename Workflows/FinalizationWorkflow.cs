using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class FinalizationWorkflow : IWorkflowStep
    {
        public string Name => "Setup Complete";

        public void Execute(Window window)
        {
            SelectExitAndFinish(window);
        }

        private static void SelectExitAndFinish(Window window)
        {
            var exitRadioButton = window
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

            ButtonClicker.Click(window, "_btnFinish", "Finish");
        }
    }
}
