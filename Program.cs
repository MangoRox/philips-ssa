using FlaUI.UIA3;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Configuration;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation
{
    internal class Program
    {
        private const string ProcessName = "Philips.PMP.SystemSetupHost.exe";
        private const string ConfigFilePath = "config.json";

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

            var windowLocator = new WindowLocator(desktop);
            var firstWindow = windowLocator.FindWindowByProcessId(app.ProcessId, 18, TimeSpan.FromSeconds(5));
            var postLoginWindow = firstWindow;
            if (firstWindow.Title.Contains("Login", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Found login window: {0}", firstWindow.Title);
                var loginWorkflow = new LoginWorkflow(desktop, app);
                postLoginWindow = loginWorkflow.Execute();
            }
            else
            {
                Console.WriteLine("Already logged in. Found window: {0}", firstWindow.Title);
            }

            var steps = new List<IWorkflowStep>
            {
                new LanguageSettingsWorkflow(),
                new SystemInformationWorkflow(),
                new ServerConnectionWorkflow(),
                new DatabaseInstallationWorkflow(),
                new SQLServerConnectionWorkflow(),
                new TopologyWorkflow(config),
                new TechnicalOptionAssignmentWorkflow(config),
                new PicCredsWorkflow(),
                new EncryptionWorkflow(config),
                new PlatformSecurityWorkflow(),
                new HostQualificationWorkflow(),
                new LicenseConfigurationWorkflow(),
                new FinalizationWorkflow(),
                new PeripheralConfigurationWorkflow(),
                new DisplayConfigurationWorkflow(),
            };

            new SetupOrchestrator(steps, postLoginWindow).Run();
        }
    }
}
