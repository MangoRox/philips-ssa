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

            var topologyWorkflow = new TopologyWorkflow(postLoginWindow, config);
            topologyWorkflow.Execute();

            var licensingWorkflow = new LicensingWorkflow(postLoginWindow, config);
            licensingWorkflow.Execute();

            var encryptionWorkflow = new EncryptionWorkflow(postLoginWindow);
            encryptionWorkflow.Execute();

            var platformSecurityWorkflow = new PlatformSecurityWorkflow(postLoginWindow);
            platformSecurityWorkflow.Execute();

            var hostQualificationWorkflow = new HostQualificationWorkflow(postLoginWindow);
            hostQualificationWorkflow.Execute();

            var finalizationWorkflow = new FinalizationWorkflow(postLoginWindow);
            finalizationWorkflow.Execute();
        }
    }
}
