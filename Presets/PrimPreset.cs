using SystemSetupAutomation.Configuration;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation.Presets
{
    internal sealed class PrimPreset : IPreset
    {
        public string Name => "PRIM";

        public IWorkflowStep[] BuildSteps()
        {
            var config = new SetupConfiguration
            {
                TopologyItemNameChange = true,
                HostLicensingConfiguration = new Dictionary<string, Dictionary<string, Dictionary<string, LicenseOption>>>
                {
                    ["NX11PRIM"] = new()
                    {
                        ["Enterprise"] = new()
                        {
                            ["Alarm Panel"] = new() { Enabled = true, Row = 13 },
                            ["Event Notification"] = new() { Enabled = false, Row = 12 }
                        }
                    },
                    ["NX11WEB-1"] = new()
                    {
                        ["Host"] = new()
                        {
                            ["Patient Center View"] = new() { Enabled = true, Row = 8 }
                        }
                    },
                    ["NX11WEB-2"] = new()
                    {
                        ["Host"] = new()
                        {
                            ["Web Proxy Server"] = new() { Enabled = false, Row = 10 },
                            ["Web Server"] = new() { Enabled = true, Row = 9 }
                        }
                    }
                }
            };

            return
            [
                // new NavigationStep(times: 7),
                // new TopologyWorkflow(config),
                // new NavigationStep(times: 2, delaySeconds: 2),
                // new LicensingWorkflow(config),
                // new NavigationStep(),
                // new EncryptionWorkflow(),
                // new NavigationStep(),
                // new PicCredsWorkflow(),
                // new NavigationStep(),
                // new NavigationStep(),
                // new PlatformSecurityWorkflow(),
                // new NavigationStep(),
                // new HostQualificationWorkflow(),
                // new NavigationStep(),
                // new FinalizationWorkflow()
            ];
        }
    }
}
