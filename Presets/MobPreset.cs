using SystemSetupAutomation.Configuration;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation.Presets
{
    internal sealed class MobPreset : IPreset
    {
        public string Name => "MOB";

        public IWorkflowStep[] BuildSteps()
        {
            return
            [
                new NavigationStep(times: 8, delaySeconds: 2),
                new EncryptionWorkflow(),
                new NavigationStep(times: 3, delaySeconds: 2),
                new PlatformSecurityWorkflow(),
                new NavigationStep(),
                new HostQualificationWorkflow(),
                new NavigationStep(),
                new FinalizationWorkflow()
            ];
        }
    }
}
