using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.ImageProcessing;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class HostQualificationWorkflow : IWorkflowStep
    {
        private const int CorrectionMaxAttempts = 30;
        private const int CorrectionIntervalSeconds = 20;

        private static readonly string[] SkippableItemNames = { "Windows Activation", "Link Speed and Duplex" };

        public string Name => "Host Qualification";

        public void Execute(Window window)
        {
            CorrectionWaiter.WaitForCompletion(window, "Host Qualification");

            var correctButton = window
                .FindFirstDescendant(cf =>
                    cf.ByName("Correct").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            if (correctButton is null)
            {
                Console.WriteLine("ERROR: could not find host qualification correct button.");
                return;
            }

            Console.WriteLine("Found host qualification correct button.");

            var qualificationItems = GetQualificationItems(window);
            if (qualificationItems is null)
            {
                return;
            }

            var outputFolder = Path.Combine(Environment.CurrentDirectory, "Qualification_Items");
            Directory.CreateDirectory(outputFolder);

            bool isFullyCorrected = false;
            while (!isFullyCorrected)
            {
                isFullyCorrected = VerifyAllItems(qualificationItems, outputFolder);

                if (!isFullyCorrected)
                {
                    correctButton.Invoke();
                    CorrectionWaiter.WaitForCompletion(
                        window,
                        "Host Qualification correction",
                        CorrectionMaxAttempts,
                        CorrectionIntervalSeconds);
                }
            }

            window.SetForeground();
            window.Focus();
            window.Click();
        }

        private static List<ListBoxItem>? GetQualificationItems(Window window)
        {
            var items = window
                .FindFirstDescendant(cf => cf.ByAutomationId("_lvwQualificationItems"))
                ?.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem))
                .Select(item => item.AsListBoxItem())
                .ToList();

            if (items is null)
            {
                Console.WriteLine("ERROR: could not find qualification items list.");
            }

            return items;
        }

        private static bool VerifyAllItems(List<ListBoxItem> items, string outputFolder)
        {
            bool allCorrect = true;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (ShouldSkipItem(item))
                {
                    DisableSkippableItem(item);
                    continue;
                }

                item.ScrollIntoView();

                var fileName = $"{i + 1}-{item.Name}.png";
                var filePath = Path.Combine(outputFolder, fileName);

                using var bitmap = item.Capture();
                bitmap.Save(filePath);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                var status = ScreenshotClassifier.Classify(filePath);
                Console.WriteLine("Saved screenshot of '{0}' to '{1}' — status: {2}.", item.Name, filePath, status);

                if (status == "error")
                {
                    allCorrect = false;
                }
            }

            return allCorrect;
        }

        private static bool ShouldSkipItem(ListBoxItem item)
        {
            return SkippableItemNames.Any(name => item.Name.Contains(name));
        }

        private static void DisableSkippableItem(ListBoxItem item)
        {
            var togglePattern = item.Patterns.Toggle.Pattern;
            var toggleState = togglePattern.ToggleState.Value;

            if (toggleState != ToggleState.On)
            {
                return;
            }

            Console.WriteLine("Skipping '{0}'.", item.Name);
            togglePattern.Toggle();
            Thread.Sleep(TimeSpan.FromSeconds(1));

            while (togglePattern.ToggleState.Value != ToggleState.Off)
            {
                togglePattern.Toggle();
                Console.WriteLine("Waiting for '{0}' to toggle off...", item.Name);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
