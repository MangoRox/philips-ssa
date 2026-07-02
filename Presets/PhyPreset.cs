using SystemSetupAutomation.Configuration;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation.Presets
{
    internal sealed class PhyPreset : IPreset
    {
        public string Name => "PHY";

        public IWorkflowStep[] BuildSteps()
        {
            return
            [
                // new NavigationStep(times: 11, delaySeconds: 2),
                // new EncryptionWorkflow(),
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
