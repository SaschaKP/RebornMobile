

using System.IO;

using Microsoft.Xna.Framework;

using TinyJson;

namespace ClassicUO.Configuration
{
    internal sealed class Settings
    {
        public static Settings GlobalSettings = new Settings();

        public Settings()
        {
        }


        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("ip")] public string IP { get; set; } = "127.0.0.1";

        [JsonProperty("port")] public ushort Port { get; set; } = 2593;

        [JsonProperty("ultimaonlinedirectory")]
        public string UltimaOnlineDirectory { get; set; } = "";

        [JsonProperty("clientversion")]
        public string ClientVersion { get; set; } = string.Empty;

        [JsonProperty("lastcharactername")]
        public string LastCharacterName { get; set; } = string.Empty;

        [JsonProperty("cliloc")]
        public string ClilocFile { get; set; } = "Cliloc.enu";

        [JsonProperty("lastservernum")]
        public ushort LastServerNum { get; set; } = 1;

        [JsonProperty("fps")]
        public int FPS { get; set; } = 60;
        [JsonProperty("window_position")] public Point? WindowPosition { get; set; }
        [JsonProperty("window_size")] public Point? WindowSize { get; set; }

        [JsonProperty("is_win_maximized")]
        public bool IsWindowMaximized { get; set; } = true;

        [JsonProperty("saveaccount")]
        public bool SaveAccount { get; set; }

        [JsonProperty("autologin")]
        public bool AutoLogin { get; set; }

        [JsonProperty("reconnect")]
        public bool Reconnect { get; set; }

        [JsonProperty("reconnect_time")]
        public int ReconnectTime { get; set; }

        [JsonProperty("login_music")]
        public bool LoginMusic { get; set; } = true;

        [JsonProperty("login_music_volume")]
        public int LoginMusicVolume { get; set; } = 70;

        [JsonProperty("shard_type")]
        public int ShardType { get; set; } // 0 = normal (no customization), 1 = old, 2 = outlands??

        [JsonProperty("fixed_time_step")]
        public bool FixedTimeStep { get; set; } = true;

        [JsonProperty("run_mouse_in_separate_thread")]
        public bool RunMouseInASeparateThread { get; set; } = true;

        [JsonProperty("force_driver")]
        public byte ForceDriver { get; set; }

        [JsonProperty("use_verdata")]
        public bool UseVerdata { get; set; }

        [JsonProperty("encryption")]
        public byte Encryption { get; set; }

        [JsonProperty("plugins")]
        public string[] Plugins { get; set; } = { @"./Assistant/Razor.dll" };

        [JsonProperty("internal_assistant")]
        public bool EnableInternalAssistant { get; set; } = true;

        public const string SETTINGS_FILENAME = "settings.json";
        public static string CustomSettingsFilepath = null;

        public static string GetSettingsFilepath()
        {
            if (CustomSettingsFilepath != null)
            {
                if (Path.IsPathRooted(CustomSettingsFilepath))
                    return CustomSettingsFilepath;
                else
                    return Path.Combine(CUOEnviroment.ExecutablePath, CustomSettingsFilepath);
            }

            return Path.Combine(CUOEnviroment.ExecutablePath, SETTINGS_FILENAME);
        }



        public void Save()
        {
            // Make a copy of the settings object that we will use in the saving process
            string json = this.Encode(true);
            Settings settingsToSave = json.Decode<Settings>();  // JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(this));

            // Make sure we don't save username and password if `saveaccount` flag is not set
            // NOTE: Even if we pass username and password via command-line arguments they won't be saved
            if (!settingsToSave.SaveAccount)
            {
                settingsToSave.Username = string.Empty;
                settingsToSave.Password = string.Empty;
            }

            // NOTE: We can do any other settings clean-ups here before we save them

            ConfigurationResolver.Save(settingsToSave, GetSettingsFilepath());
        }
    }
}