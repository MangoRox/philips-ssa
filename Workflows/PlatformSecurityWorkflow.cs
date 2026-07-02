using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.ImageProcessing;

namespace SystemSetupAutomation.Workflows
{
    internal sealed class PlatformSecurityWorkflow : IWorkflowStep
    {
        private const int CorrectionMaxAttempts = 30;
        private const int CorrectionIntervalSeconds = 20;

        public string Name => "Platform Security";

        public void Execute(Window window)
        {
            var correctButton = window
                .FindFirstDescendant(cf =>
                    cf.ByName("Correct").And(cf.ByControlType(ControlType.Button)))
                ?.AsButton();

            if (correctButton is null)
            {
                Console.WriteLine("ERROR: could not find 'Correct' button on Platform Security page.");
                return;
            }

            correctButton.Invoke();
            CorrectionWaiter.WaitForCompletion(window, "Platform Security correction");

            var securityItems = GetQualificationItems(window);
            if (securityItems is null)
            {
                return;
            }

            var outputFolder = Path.Combine(Environment.CurrentDirectory, "Platform_Security_Items");
            Directory.CreateDirectory(outputFolder);

            bool isFullyCorrected = false;
            while (!isFullyCorrected)
            {
                isFullyCorrected = VerifyAllItems(securityItems, outputFolder);

                if (!isFullyCorrected)
                {
                    correctButton.Invoke();
                    CorrectionWaiter.WaitForCompletion(
                        window,
                        "Platform Security correction",
                        CorrectionMaxAttempts,
                        CorrectionIntervalSeconds);
                }
            }
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
                item.ScrollIntoView();

                var fileName = $"{i + 1}-{item.Name}.png";
                var filePath = Path.Combine(outputFolder, fileName);

                using var bitmap = item.Capture();
                bitmap.Save(filePath);
                Thread.Sleep(TimeSpan.FromSeconds(1));

                var status = ScreenshotClassifier.Classify(filePath);
                Console.WriteLine("Saved screenshot of '{0}' to '{1}' — status: {2}.", item.Name, filePath, status);

                if (status != "success")
                {
                    allCorrect = false;
                }
            }

            return allCorrect;
        }
    }
}
