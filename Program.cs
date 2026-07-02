using FlaUI.UIA3;
using SystemSetupAutomation.Automation;
using SystemSetupAutomation.Presets;
using SystemSetupAutomation.Workflows;

namespace SystemSetupAutomation
{
    internal class Program
    {
        private const string ProcessName = "Philips.PMP.SystemSetupHost.exe";

        static void Main(string[] args)
        {
            var presetName = ParsePresetArgument(args);
            if (presetName is null)
            {
                Console.WriteLine("Usage: SysSetupAutomation --preset <PHY|WEB|PIC|MOB>");
                return;
            }

            var preset = PresetResolver.Resolve(presetName);
            if (preset is null)
            {
                return;
            }

            Console.WriteLine("Running preset '{0}'.", preset.Name);

            Console.WriteLine("Attempting to attach to process '{0}'...", ProcessName);
            using var automation = new UIA3Automation();
            var desktop = automation.GetDesktop();

            var processAttacher = new ProcessAttacher(desktop);
            var app = processAttacher.AttachToProcess(ProcessName);
            if (app is null)
            {
                return;
            }

            Console.WriteLine("Successfully attached to process '{0}' (PID: {1}).", ProcessName, app.ProcessId);

            var loginWorkflow = new LoginWorkflow(desktop, app);
            var window = loginWorkflow.Execute();
            if (window is null)
            {
                return;
            }

            var steps = preset.BuildSteps();
            foreach (var step in steps)
            {
                Console.WriteLine("--- Executing step: {0} ---", step.Name);
                step.Execute(window);
            }

            Console.WriteLine("Preset '{0}' completed successfully.", preset.Name);
        }

        private static string? ParsePresetArgument(string[] args)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("--preset", StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
