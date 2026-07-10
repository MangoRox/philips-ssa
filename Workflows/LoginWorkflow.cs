using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using SystemSetupAutomation.Automation;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class LoginWorkflow(AutomationElement desktop, Application app)
    {
        private const string PostLoginWindowTitle = "PIC iX System Setup";
        private const int MaxWindowLookupAttempts = 18;
        private const int PostLoginWindowLookupAttempts = 6;
        private static readonly TimeSpan WindowLookupInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PostLoginWindowLookupInterval = TimeSpan.FromSeconds(5);

        private const string DefaultUsername = "PhilipsBD";
        private const string DefaultPassword = "BK|Sup42p0rt!";

        public Window Execute()
        {
            var windowLocator = new WindowLocator(desktop);

            var loginWindow = windowLocator.FindWindowByProcessId(
                app.ProcessId,
                MaxWindowLookupAttempts,
                WindowLookupInterval);

            Console.WriteLine("Attached to: {0}", loginWindow.Title);

            EnterCredentials(loginWindow);

            ButtonClicker.Click(loginWindow, "_btnOk", "OK");

            var postLoginWindow = windowLocator.FindWindowByTitle(
                app.ProcessId,
                PostLoginWindowTitle,
                PostLoginWindowLookupAttempts,
                PostLoginWindowLookupInterval);

            Console.WriteLine("Found window: {0}", postLoginWindow.Title);
            return postLoginWindow;
        }

        private static void EnterCredentials(Window loginWindow)
        {
            var usernameField = loginWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtUserName"))
                ?.AsTextBox();

            if (usernameField is null)
            {
                throw new InvalidOperationException("Could not find element with AutomationId '_txtUserName'.");
            }

            usernameField.Text = DefaultUsername;
            Console.WriteLine("Username field updated.");

            var passwordField = loginWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("_txtPassword"))
                ?.AsTextBox();

            if (passwordField is null)
            {
                throw new InvalidOperationException("Could not find element with AutomationId '_txtPassword'.");
            }

            passwordField.Text = DefaultPassword;
            Console.WriteLine("Password field updated.");
        }
    }
}
