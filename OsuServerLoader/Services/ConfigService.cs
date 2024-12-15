using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace OsuServerLoader.Services
{

    public class UiSettings
    {
        public int configVersion { get; set; }
        public int serverIndex { get; set; }
        public bool usePatcher { get; set; }
        public bool useAccount { get; set; }
        public bool showWelcomePage { get; set; }
        public bool showExitDialog { get; set; }
        public string osuPath { get; set; }
        public string selectedLabel { get; set; }
        public string currentLabel { get; set; }
        public string currentDevflag { get; set; }
        public string currentNickname { get; set; }
        public string currentPassword { get; set; }
    }

    public class ConfigService
    {
        const int reqVerisonConfig = 306;
        DataService dataService = new DataService();

        public void CreateConfigFile()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathConfigFolder = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader");
            string pathConfigFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\UiSettings.cfg");

            if (Directory.Exists(pathConfigFolder))
            {
                Directory.Delete(pathConfigFolder, true);
            }

            var uiSettings = new UiSettings
            {
                configVersion = reqVerisonConfig,
                serverIndex = 0,
                usePatcher = false,
                useAccount = false,
                showWelcomePage = true,
                showExitDialog = true,
                osuPath = "",
                selectedLabel = "",
                currentLabel = "",
                currentDevflag = "",
                currentNickname = "",
                currentPassword = ""
            };
            string jsonConfig = JsonSerializer.Serialize<UiSettings>(uiSettings);

            Directory.CreateDirectory(pathConfigFolder);

            File.WriteAllText(pathConfigFile, jsonConfig);
            dataService.CreateDataFile();
            using (var client = new WebClient())
            {
                client.DownloadFile("http://osuokayu.moe/static/Osu!Patcher.zip", userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip");
                System.IO.Compression.ZipFile.ExtractToDirectory(userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip", userFolderPath + "\\.OsuServerLoader");
                File.Delete(userFolderPath + "\\.OsuServerLoader\\Osu!Patcher.zip");
            }
        }

        public UiSettings Load()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathConfigFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\UiSettings.cfg");

            bool fileExists = File.Exists(pathConfigFile);
            if (fileExists == false)
            {
                CreateConfigFile();
            }

            var config = JsonSerializer.Deserialize<UiSettings>(File.ReadAllText(pathConfigFile));
            if (config.configVersion < reqVerisonConfig)
            {
                CreateConfigFile();
                return JsonSerializer.Deserialize<UiSettings>(File.ReadAllText(pathConfigFile));
            }
            return config;
        }

        public void Save(UiSettings currentSettings)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathConfigFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\UiSettings.cfg");

            File.Delete(pathConfigFile);
            string jsonConfig = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(pathConfigFile, jsonConfig);
        }
    }
}