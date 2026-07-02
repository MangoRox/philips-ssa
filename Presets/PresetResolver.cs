namespace SystemSetupAutomation.Presets
{
    internal static class PresetResolver
    {
        private static readonly Dictionary<string, Func<IPreset>> Registry = new(StringComparer.OrdinalIgnoreCase)
        {
            ["PRIM"] = () => new PrimPreset(),
            ["PHY"] = () => new PhyPreset(),
            ["WEB"] = () => new WebPreset(),
            ["PIC"] = () => new PicPreset(),
            ["MOB"] = () => new MobPreset()
        };

        public static IPreset? Resolve(string presetName)
        {
            if (Registry.TryGetValue(presetName, out var factory))
            {
                return factory();
            }

            Console.WriteLine("ERROR: unknown preset '{0}'. Available presets: {1}.",
                presetName,
                string.Join(", ", Registry.Keys));
            return null;
        }
    }
}
