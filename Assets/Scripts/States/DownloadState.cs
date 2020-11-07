using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadState : IState
{
    public List<(string, long)> FilesToDownload;
    public string ResourcePathForFilesToDownload;
    
    public static readonly List<string> NeededUoFileExtensions = new List<string>{".def", ".mul", ".idx", ".uop", ".enu", ".rle", ".txt", ".mp3"};

    private readonly DownloadPresenter downloadPresenter;
    private readonly Canvas inGameDebugConsoleCanvas;
    
    private DownloaderBase downloader;
    private const string H_REF_PATTERN = @"<a\shref=[^>]*>([^<]*)<\/a>";

    public DownloadState(DownloadPresenter downloadPresenter, Canvas inGameDebugConsoleCanvas)
    {
        this.downloadPresenter = downloadPresenter;
        this.inGameDebugConsoleCanvas = inGameDebugConsoleCanvas;

        downloadPresenter.BackButtonPressed += Quit;
    }

    internal bool AllowMetered = false;

    private void OnCellularWarningYesButtonPressed()
    {
        ResetCellularWarning();
        AllowMetered = true;
        StartDirectoryDownloader();
    }
    
    private void OnCellularWarningNoButtonPressed()
    {
        ResetCellularWarning();
        //StateManager.GoToState<ServerConfigurationState>();
        Quit();
    }

    private void ResetCellularWarning()
    {
        downloadPresenter.ToggleCellularWarning(false);
        downloadPresenter.CellularWarningYesButtonPressed -= OnCellularWarningYesButtonPressed;
        downloadPresenter.CellularWarningNoButtonPressed -= OnCellularWarningNoButtonPressed;
    }

    private void OnDownloadWarningYesButtonPressed()
    {
        ResetDownloadWarning();
        StartDirectoryDownloader(true);
    }

    private void OnDownloadWarningNoButtonPressed()
    {
        ResetDownloadWarning();
        Quit();
    }

    private void ResetDownloadWarning()
    {
        downloadPresenter.ToggleDownloaderWarning(false);
        downloadPresenter.CellularWarningYesButtonPressed -= OnDownloadWarningYesButtonPressed;
        downloadPresenter.CellularWarningNoButtonPressed -= OnDownloadWarningNoButtonPressed;
    }

    public void Enter()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DebugInternal.Log($"Downloading files to {ServerConfiguration.GetPathToSaveFiles()}");

        downloadPresenter.gameObject.SetActive(true);
        downloader = new UOItaliaDownloader();
        downloader.Initialize(this, downloadPresenter);
    }

    public void SetFileListAndDownload(List<(string, long)> filesList, string resourcePathForFilesToDownload = null)
    {
        FilesToDownload = filesList;
        ResourcePathForFilesToDownload = resourcePathForFilesToDownload;

        //Check that some of the essential UO files exist
        var hasValidFiles = GameState.HasValidUOFiles(out _);

        if (FilesToDownload.Count == 0)
        {
            if (hasValidFiles == false)
            {
                var error = "Download directory does not contain UO files and download server isn't offering any file!";
                StopAndShowError(error);
            }
            else
            {
                PlayerPrefs.SetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, UOItaliaDownloader.PatchVersion.ToString());
                PlayerPrefs.Save();
                StateManager.GoToState<GameState>();
            }
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            ShowCellularWarning();
        }
        else
        {
            StartDirectoryDownloader();
        }
    }

    private void StartDirectoryDownloader(bool force = false)
    {
        if (!force && !GameState.HasValidUOFiles(out _))
        {
            ShowDownloadWarning(downloader as UOItaliaDownloader);
        }
        else
        {
            downloader = new UOItaliaDirectoryDownloader(AllowMetered);
            downloader.Initialize(this, downloadPresenter);
        }
    }

    private void ShowCellularWarning()
    {
        downloadPresenter.CellularWarningYesButtonPressed += OnCellularWarningYesButtonPressed;
        downloadPresenter.CellularWarningNoButtonPressed += OnCellularWarningNoButtonPressed;
        downloadPresenter.ToggleCellularWarning(true);
    }

    private void ShowDownloadWarning(UOItaliaDownloader uoidownloader)
    {
        ulong avail = SimpleDiskUtils.DiskUtils.CheckAvailableSpace();
        if((avail - (uoidownloader.FilesSize / 99)) > uoidownloader.FilesSize || (avail == 0 && SimpleDiskUtils.DiskUtils.CheckTotalSpace() == 0))//there is enough space or there is an error in reading the free space, let's go on
        {
            downloadPresenter.CellularWarningYesButtonPressed += OnDownloadWarningYesButtonPressed;
            downloadPresenter.CellularWarningNoButtonPressed += OnDownloadWarningNoButtonPressed;
            if(avail > 0)
                downloadPresenter.ToggleDownloaderWarning(true, $"WARNING, the game requires some assets, they will be downloaded and installed if you PRESS YES!\nThe assets will occupy {uoidownloader.FilesSize / 0x100000}Megabytes (available {avail / 0x100000}Megabytes)");
            else
                downloadPresenter.ToggleDownloaderWarning(true, $"WARNING, the game requires some assets, they will be downloaded and installed if you PRESS YES!\nThe assets will occupy {uoidownloader.FilesSize / 0x100000}Megabytes");
        }
        else
        {
            downloadPresenter.InsufficientDiskSpaceWarning($"You don't have enough free space in your device! Space required {uoidownloader.FilesSize / 0x100000}Megabytes / Available {avail / 0x100000}Megabytes");
        }
    }

    public static Uri GetUri(string serverUrl, int port, string fileName = null)
    {
        var httpPort = port == 80;
        var httpsPort = port == 443;
        var defaultPort = httpPort || httpsPort;
        var scheme = httpsPort ? "https" : "http";
        var uriBuilder = new UriBuilder(scheme, serverUrl, defaultPort ? - 1 : port, fileName);
        return uriBuilder.Uri;
    }

    public void StopAndShowError(string error)
    {
        PlayerPrefs.SetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, "0");
        PlayerPrefs.Save();
        DebugInternal.LogError(error);
        //Stop downloads
        downloadPresenter.ShowError(error);
        downloadPresenter.ClearFileList();
        inGameDebugConsoleCanvas.enabled = true;
    }

    public void Exit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        downloadPresenter.ClearFileList();
        downloadPresenter.gameObject.SetActive(false);
        downloader?.Dispose();
        FilesToDownload = null;
    }

    public void Quit()
    {
        Exit();
        PlayerPrefs.SetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, "0");
        PlayerPrefs.Save();
        Application.Quit();
    }
}