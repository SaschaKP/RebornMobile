using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UOItaliaDownloader : DownloaderBase
{
    private int port;
    internal const string UOITALIA_VERSION_PREF_KEY = "UOItalia_VERSION";
    internal static int PatchVersion = 0;
    private Coroutine CheckerRoutine;

    public override void Initialize(DownloadState downloadState, DownloadPresenter downloadPresenter)
    {
        if (!Directory.Exists(ServerConfiguration.GetPathToSaveFiles()))
            Directory.CreateDirectory(ServerConfiguration.GetPathToSaveFiles());
        int.TryParse(ServerConfiguration.FileDownloadServerPort, out port);
        base.Initialize(downloadState, downloadPresenter);
        //Make version request
        var uri = DownloadState.GetUri($"{ServerConfiguration.FileDownloadServerUrl}", port, "PSTAT");
        var request = UnityWebRequest.Get(uri);
        request.timeout = 10;
        request.SendWebRequest().completed += operation =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                downloadState.StopAndShowError($"Error while making initial request to server ({uri}): {request.error}");
                if (GameState.HasValidUOFiles(out _) == true)
                    StateManager.GoToState<GameState>();
                return;
            }

            if (int.TryParse(request.downloadHandler.text.Trim(), out PatchVersion))
            {
                DebugInternal.Log($"Request result PATCH (version): {PatchVersion}");
                int.TryParse(PlayerPrefs.GetString(UOITALIA_VERSION_PREF_KEY, "0"), out int previousVersion);
                if (previousVersion != PatchVersion || GameState.HasValidUOFiles(out _) == false)
                {
                    _MainDir = ServerConfiguration.GetPathToSaveFiles();
                    GetManifest();
                }
                else
                    StateManager.GoToState<GameState>();
            }
            else
                downloadState.StopAndShowError($"Invalid patch version from server: {request.downloadHandler.text}");
        };
    }

    private string _MainDir;
    private readonly char[] _SplitNL = new char[] { '\n' };
    private string[] split;
    private void GetManifest()
    {
        string baseuri = $"{ServerConfiguration.FileDownloadServerUrl}";
        //Make version request
        var uri = DownloadState.GetUri(baseuri, port, "patcher2.txt");
        var request = UnityWebRequest.Get(uri);
        request.SendWebRequest().completed += operation =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                var error = $"Error while making initial request to server: {request.error}";
                downloadState.StopAndShowError(error);
                return;
            }
            split = request.downloadHandler.text.Replace("\r\n", "\n").Split(_SplitNL, StringSplitOptions.None);
            CheckerRoutine = downloadPresenter.StartCoroutine(FilesChecker());
        };
    }

    private IEnumerator FilesChecker()
    {
        bool deletion = false;
        StringBuilder _sb = new StringBuilder();
        List<(string, long)> filesToDownload = new List<(string, long)>();
        int totalfiles = 0, totalchecked = 0;
        string dirfile = "";
        string filename = "";
        for (int i = 0; i < split.Length; i++)
        {
            if (split[i].Contains("§"))
                break;
            if (i % 2 != 0 && !string.IsNullOrWhiteSpace(split[i]))
            {
                string s = split[i - 1];
                if (DownloadState.NeededUoFileExtensions.Any(s.Contains) && (!s.Contains('/') || s.Contains("Music/Digital")))
                    ++totalfiles;
            }
        }
        using (Crc32 crc = new Crc32())
        {
            for (int i = 0; i < split.Length; i++)
            {
                string s = split[i].Trim();
                if (deletion)
                {
                    if (s.Length > 0)
                    {
                        if (File.Exists(Path.Combine(_MainDir, s)))
                            File.Delete(Path.Combine(_MainDir, s));
                        else if (Directory.Exists(Path.Combine(_MainDir, s)))
                            Directory.Delete(Path.Combine(_MainDir, s), true);
                    }
                }
                else
                {
                    deletion = s.Length > 0 && s.Contains("§");
                    if (!deletion)
                    {
                        if (i % 2 != 0)
                        {
                            dirfile = Path.GetFullPath(Path.Combine(_MainDir, filename));
                            if (s.Length == 0)
                            {
                                if (!Directory.Exists(dirfile) && filename == "Music/Digital")
                                    Directory.CreateDirectory(dirfile);
                            }
                            else if (DownloadState.NeededUoFileExtensions.Any(filename.Contains) && (!filename.Contains('/') || filename.Contains("Music/Digital")))
                            {
                                string[] subsplit = s.Split(' ');

                                bool download;
                                if ((download = !File.Exists(dirfile)) == false)
                                {
                                    using (var fs = File.OpenRead(dirfile))
                                    {
                                        byte[] ba = crc.ComputeHash(fs);
                                        _sb.Clear();
                                        for (int ib = 0; ib < ba.Length; ib++)
                                            _sb.Append(ba[ib].ToString("X2"));
                                        if (_sb.ToString().Replace("-", string.Empty).ToLowerInvariant() != subsplit[0])
                                        {
                                            download = true;
                                        }
                                    }
                                }
                                if (download)
                                {
                                    if (subsplit.Length > 1 && long.TryParse(subsplit[1], out long size))
                                    {
                                        FilesSize += (ulong)size;
                                        filesToDownload.Add((filename, size));
                                    }
                                }
                                ++totalchecked;

                                downloadPresenter.UpdateCheckingView(filesToDownload.Count, totalchecked, totalfiles);
                                yield return null;
                            }
                        }
                        else
                        {
                            filename = s;
                        }
                    }
                }
            }
        }
        downloadState.SetFileListAndDownload(filesToDownload);
    }

    internal ulong FilesSize = 0;

    public override void Dispose()
    {
        if (CheckerRoutine != null)
            downloadPresenter?.StopCoroutine(CheckerRoutine);
        CheckerRoutine = null;
        split = null;
        base.Dispose();
    }
}
