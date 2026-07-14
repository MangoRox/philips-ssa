using System.Text.Json;

namespace SystemSetupAutomation.Configuration
{
    internal sealed class SetupConfiguration
    {
        public bool TopologyItemNameChange { get; set; }
        public string? PrimaryServerName { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, LicenseOption>>>? HostLicensingConfiguration { get; set; }

        public static SetupConfiguration? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("ERROR: configuration file '{0}' not found.", filePath);
                return null;
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<SetupConfiguration>(json, options);
        }
    }
}
