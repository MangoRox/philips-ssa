using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LoginWorkflow
    {
        private const string PostLoginWindowTitle = "PIC iX System Setup";
        private const int MaxWindowLookupAttempts = 18;
        private const int PostLoginWindowLookupAttempts = 6;
        private static readonly TimeSpan WindowLookupInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PostLoginWindowLookupInterval = TimeSpan.FromSeconds(5);

        private const string DefaultUsername = "PhilipsBD";
        private const string DefaultPassword = "BK|Sup42p0rt!";

        private readonly AutomationElement _desktop;
        private readonly Application _app;

        public LoginWorkflow(AutomationElement desktop, Application app)
        {
            _desktop = desktop;
            _app = app;
        }

        public Window? Execute()
        {
            var windowLocator = new WindowLocator(_desktop);

            var loginWindow = windowLocator.FindWindowByProcessId(
                _app.ProcessId,
                MaxWindowLookupAttempts,
                WindowLookupInterval);

            if (loginWindow is null)
            {
                return null;
            }

            Console.WriteLine("Attached to: {0}", loginWindow.Title);

            if (!EnterCredentials(loginWindow))
            {
                return null;
            }

            ButtonClicker.Click(loginWindow, "_btnOk", "OK");

            var postLoginWindow = windowLocator.FindWindowByTitle(
                _app.ProcessId,
                PostLoginWindowTitle,
                PostLoginWindowLookupAttempts,
                PostLoginWindowLookupInterval);

            if (postLoginWindow is null)
            {
                return null;
            }

            Console.WriteLine("Found window: {0}", postLoginWindow.Title);
            return postLoginWindow;
        }

        private static bool EnterCredentials(Window loginWindow)
        {
            var usernameField = loginWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtUserName"))
                ?.AsTextBox();

            if (usernameField is null)
            {
                Console.WriteLine("ERROR: could not find element with AutomationId '_txtUserName'.");
                return false;
            }

            usernameField.Text = DefaultUsername;
            Console.WriteLine("Username field updated.");

            var passwordField = loginWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtPassword"))
                ?.AsTextBox();

            if (passwordField is null)
            {
                Console.WriteLine("ERROR: could not find element with AutomationId '_txtPassword'.");
                return false;
            }

            passwordField.Text = DefaultPassword;
            Console.WriteLine("Password field updated.");

            return true;
        }
    }
}
