using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class PeripheralConfigurationWorkflow : IWorkflowStep
    {
        public string Name => "Peripheral Configuration";

        public void Execute(Window window)
        {
            // page sometimes has checkbox for sound verification that is required to be checked
            CheckBox? soundVerificationCheckbox = window
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_chkSoundVerification").And(cf.ByControlType(ControlType.CheckBox)))
                ?.AsCheckBox();

            if (soundVerificationCheckbox == null)
            {
                return;
            }

            if (soundVerificationCheckbox.IsChecked == false)
            {
                soundVerificationCheckbox.IsChecked = true;
                Console.WriteLine("'Sound Verification' checkbox checked.");
            }
        }
    }
}