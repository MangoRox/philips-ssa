using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace SystemSetupAutomation.Automation
{
    internal sealed class ProcessAttacher
    {
        private readonly AutomationElement _desktop;

        public ProcessAttacher(AutomationElement desktop)
        {
            _desktop = desktop;
        }

        public Application? AttachToProcess(string processName)
        {
            try
            {
                return Application.Attach(processName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: failed to attach to process '{0}': {1}", processName, ex.Message);
                return null;
            }
        }
    }
}
