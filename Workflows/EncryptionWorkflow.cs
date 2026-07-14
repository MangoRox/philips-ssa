using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Configuration;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class EncryptionWorkflow : IWorkflowStep
    {
        public string Name => "System Encryption Configuration";

        private readonly SetupConfiguration _config;

        public EncryptionWorkflow(SetupConfiguration config)
        {
            _config = config;
        }

        public void Execute(Window window)
        {
            string primaryServerName = _config.PrimaryServerName;
            string hostname = Environment.MachineName;

            if (hostname == primaryServerName)
            {
                var useDefaultButton = window
                    .FindFirstDescendant(cf =>
                        cf.ByAutomationId("_btnDefault").And(cf.ByControlType(ControlType.Button)))
                    ?.AsButton();

                if (useDefaultButton is null)
                {
                    Console.WriteLine("ERROR: could not find 'Use Default' button on Encryption page.");
                    return;
                }

                if (!useDefaultButton.IsEnabled)
                {
                    Console.WriteLine("'Use Default' button is disabled. Skipping.");
                    return;
                }

                useDefaultButton.Invoke();
                Console.WriteLine("'Use Default' button clicked.");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                return;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));

            var requestButton = window
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_btnDoWork").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            if (requestButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Request' button on Encryption page.");
                return;
            }

            if (!requestButton.IsEnabled)
            {
                Console.WriteLine("'Request' button is disabled. Skipping.");
                return;
            }

            requestButton.Invoke();
            Console.WriteLine("'Request' button clicked.");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
