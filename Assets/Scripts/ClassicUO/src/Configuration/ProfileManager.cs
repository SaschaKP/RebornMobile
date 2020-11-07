

using System.IO;

using ClassicUO.Utility;

using Microsoft.Xna.Framework;

using TinyJson;

namespace ClassicUO.Configuration
{
    internal static class ProfileManager
    {
        public static Profile Current { get; private set; }
        public static System.Action ProfileLoaded;

        public static void Load(string servername, string username, string charactername)
        {
            string path = FileSystemHelper.CreateFolderIfNotExists(CUOEnviroment.ExecutablePath, "Data", "Profiles", username, servername, charactername);
            string fileToLoad = Path.Combine(path, "profile.json");

            Current = ConfigurationResolver.Load<Profile>(fileToLoad) ?? new Profile();

            Current.Username = username;
            Current.ServerName = servername;
            Current.CharacterName = charactername;

            ValidateFields(Current);
            
            ProfileLoaded?.Invoke();
        }


        private static void ValidateFields(Profile profile)
        {
            if (profile == null)
                return;

            if (profile.WindowClientBounds.X < 600)
                profile.WindowClientBounds = new Point(600, profile.WindowClientBounds.Y);
            if (profile.WindowClientBounds.Y < 480)
                profile.WindowClientBounds = new Point(profile.WindowClientBounds.X, 480);
            
        }

        public static void UnLoadProfile()
        {
            Current = null;
        }
    }
}