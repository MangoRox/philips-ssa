using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation.Presets
{
    internal interface IPreset
    {
        string Name { get; }
        IWorkflowStep[] BuildSteps();
    }
}
