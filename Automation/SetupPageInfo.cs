using FlaUI.Core.AutomationElements;

namespace SystemSetupAutomation.Automation
{
    internal sealed class SetupPageInfo
    {
        public Window Window { get; }
        public SetupPageInfo(Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window), "Window cannot be null.");
            }
            Window = window;
        }

        public string CurrentSetupPageName()
        {
            var pageNameElement = Window
                .FindFirstDescendant(cf =>
                    cf.ByAutomationId("_bannerControl").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Pane)))
                ?.FindFirstDescendant(cf =>
                    cf.ByAutomationId("_lblMessage").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)))
                ?.AsLabel();

            if (pageNameElement is null)
            {
                Console.WriteLine("ERROR: could not find page name label on setup page.");
                return string.Empty;
            }

            return pageNameElement.Name;
        }
    }
}