using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class EncryptionWorkflow : IWorkflowStep
    {
        public string Name => "Encryption";

        public void Execute(Window window)
        {
            var configureButton = window
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_btnDoWork").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            Thread.Sleep(TimeSpan.FromSeconds(2));

            if (configureButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Configure' button on Encryption page.");
                return;
            }

            if (!configureButton.IsEnabled)
            {
                Console.WriteLine("'Configure' button is disabled. Skipping.");
                return;
            }

            configureButton.Invoke();
            Console.WriteLine("'Configure' button clicked.");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
