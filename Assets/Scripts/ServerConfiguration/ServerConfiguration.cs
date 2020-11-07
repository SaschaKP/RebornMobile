using System;
using System.IO;
using UnityEngine;

[Serializable]
public static class ServerConfiguration
{
    public static readonly string Name = "UOItalia Reborn";
    public static readonly string UoServerUrl = "srv.uoitalia.net";
    public static readonly string UoServerPort = "2590";
    public static readonly string FileDownloadServerUrl = "www.uoitalia.net/host/webinst";
    public static readonly string FileDownloadServerPort = "80";
    public static string ClientVersion = "6.0.14.2";
    public static readonly bool UseEncryption = false;
    public static string ClientPathForUnityEditor;
    //public bool AllFilesDownloaded;
    //public bool PreferExternalStorage;

    public static string GetPathToSaveFiles()
    {
        return Application.persistentDataPath;
        
        /*if (PreferExternalStorage && string.IsNullOrEmpty(Init.ExternalStoragePath) == false)
        {
            dataPath = Init.ExternalStoragePath;
        }
        
        return Path.Combine(dataPath, Name);*/
    }
}