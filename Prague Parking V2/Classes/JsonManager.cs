using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;

namespace Prague_Parking_V2.Classes
{
    public static class JsonManager
    {
        /// <summary>
        /// Combines a path of json folder and a filename then returns as a string
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string FilepathAndName(string filename)
            => Path.Combine(JsonFolderFilepath(), filename);
        public static string Filepath()
            => JsonFolderFilepath();
        /// <summary>
        /// Only returns the path to the JsonFiles folder
        /// </summary>
        /// <returns></returns>
        private static string JsonFolderFilepath() => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "JsonFiles");

        /// <summary>
        /// Returns an array of all filenames in the json folder
        /// </summary>
        /// <returns></returns>
        private static string[] GetJsonFileNames() => Directory.GetFiles(JsonFolderFilepath(), "*.json")
            .Select(Path.GetFileName)
            .ToArray()!;

        public static string GarageConfigFileName() => "GarageConfig.json";
        public static string ParkingEntriesFileName() => "ParkingEntries.json";
        public static string ReadmeFileName() => "ReadMe.json";
        /// <summary>
        /// Run on startup to serialize or load all the json files
        /// </summary>
        public static void StartUp()
        {
            string configName = GarageConfigFileName();

            if (File.Exists(FilepathAndName(configName)))
                GarageConfig.LoadExistingConfig(JsonDeserialize<GarageConfig>(configName));
            else
                File.WriteAllText(FilepathAndName(configName), JsonSerialize(GarageConfig.CurrentValues));

            string readMeName = ReadmeFileName();
            if (File.Exists(readMeName))
                File.WriteAllText(FilepathAndName(readMeName), JsonSerialize(new HowToConfig()));

            string entriesName = ParkingEntriesFileName();

            if (File.Exists(FilepathAndName(entriesName)))
            {


                var entries = JsonDeserialize<List<ParkingEntry>>(entriesName) ?? new();
                foreach (var entry in entries)
                {
                    if (entry == null) continue;
                    if (entry.Vehicle == null) continue;

                    var realSpot = ParkingGarage.ParkingSpots[entry.ParkingSpot.DesignatedSpot];
                    ParkingGarage.CreateEntryAndParkVehicle(entry.Vehicle, realSpot);

                }
            }
        }
        /// <summary>
        /// Serializes the object, overwriting any existing json file with the same name
        /// </summary>
        public static void Serialize<T>(T config, string filename) => File.WriteAllText(FilepathAndName(filename), JsonSerialize(config));

        private static string JsonSerialize<T>(T config)
        {

            return JsonSerializer.Serialize(config, Program.JsonOptions);
        }

        private static T? JsonDeserialize<T>(string fileName)
        {
            string path = FilepathAndName(fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException("No file with that name found", path);

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Deserialization error! Could not use {fileName} for {typeof(T).Name}", ex);
            }
        }
        public static void OpenConfig()
        {
            while (true)
            {

                var options = new[] { "Go Back" }.Concat(GetJsonFileNames()).ToArray();
                string file = Program.PromptSelectionWithChoices("Which config do you want to open?", options);
                if (file == "Go Back")
                    return;

                string filepath = Path.GetFullPath(JsonManager.FilepathAndName(file));
                Process.Start(new ProcessStartInfo(filepath) { UseShellExecute = true });
            }
        }
    }
}
