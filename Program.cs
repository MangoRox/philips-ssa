using FlaUI.UIA3;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Configuration;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation
{
    internal class Program
    {
        private const string ProcessName = "Philips.PMP.SystemSetupHost.exe";
        private const string ConfigFilePath = "primConfig.json";

        static void Main(string[] args)
        {
            var config = SetupConfiguration.LoadFromFile(ConfigFilePath);
            if (config is null)
            {
                Console.WriteLine("ERROR: failed to load configuration from '{0}'.", ConfigFilePath);
                return;
            }

            Console.WriteLine("Attempting to attach to process '{0}'...", ProcessName);
            using var automation = new UIA3Automation();
            var desktop = automation.GetDesktop();

            var processAttacher = new ProcessAttacher(desktop);
            var app = processAttacher.AttachToProcess(ProcessName);
            if (app is null)
            {
                return;
            }

            Console.WriteLine("Successfully attached to process '{0}' (PID: {1}).", ProcessName, app.ProcessId);

            var loginWorkflow = new LoginWorkflow(desktop, app);
            var postLoginWindow = loginWorkflow.Execute();
            if (postLoginWindow is null)
            {
                return;
            }

            // Navigate to Topology page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next", times: 7);

            var topologyWorkflow = new TopologyWorkflow(config);
            topologyWorkflow.Execute(postLoginWindow);

            // Navigate to Licensing page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next", times: 2);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            var licensingWorkflow = new LicensingWorkflow(config);
            licensingWorkflow.Execute(postLoginWindow);

            // Navigate to System Encryption Combination page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Navigate to Factory Account Passwords page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var encryptionWorkflow = new EncryptionWorkflow();
            encryptionWorkflow.Execute(postLoginWindow);

            // Navigate to Peripheral Configuration page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Navigate to Platform Security page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var platformSecurityWorkflow = new PlatformSecurityWorkflow();
            platformSecurityWorkflow.Execute(postLoginWindow);

            // Navigate to Host Qualification page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");

            var hostQualificationWorkflow = new HostQualificationWorkflow();
            hostQualificationWorkflow.Execute(postLoginWindow);

            // Navigate to Finalization page
            ButtonClicker.Click(postLoginWindow, "_btnNext", "Next");

            var finalizationWorkflow = new FinalizationWorkflow();
            finalizationWorkflow.Execute(postLoginWindow);
        }
    }
}
