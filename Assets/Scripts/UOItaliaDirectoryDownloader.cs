using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;

public class UOItaliaDirectoryDownloader : DownloaderBase
{
    private static bool CanUseCellular;
    private static bool HasMeteredConnection;
    private List<(string, long)> filesToDownload;
    private int numberOfFilesDownloaded;
    private int numberOfFilesToDownload;
    private Coroutine downloadCoroutine;
    private string pathToSaveFiles;
    private int port;
    internal const int MAX_CONCURRENT_DOWNLOADS = 3;
    private Dictionary<string, float[]> activeDownloadProgressAndFileName = new Dictionary<string, float[]>();
    private int concurrentDownloads = 0;
    private bool sentFirstWarning = false;

    public UOItaliaDirectoryDownloader(bool allowmetered)
    {
        CanUseCellular = allowmetered;
    }

    public override void Initialize(DownloadState downloadState, DownloadPresenter downloadPresenter)
    {
        base.Initialize(downloadState, downloadPresenter);
        ServicePointManager.DefaultConnectionLimit = MAX_CONCURRENT_DOWNLOADS * 2;
        pathToSaveFiles = ServerConfiguration.GetPathToSaveFiles();
        port = int.Parse(ServerConfiguration.FileDownloadServerPort);
        filesToDownload = downloadState.FilesToDownload;
        numberOfFilesToDownload = filesToDownload.Count;
        downloadPresenter.SetFileList(filesToDownload);
        downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles());
    }

    private IEnumerator DownloadFiles()
    {
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }

        downloadPresenter.UpdateView(numberOfFilesDownloaded, numberOfFilesToDownload);
        while (filesToDownload.Count > 0 || concurrentDownloads > 0)
        {
            while (concurrentDownloads < MAX_CONCURRENT_DOWNLOADS && filesToDownload.Count > 0)
            {
                var fileName = filesToDownload[0];
                filesToDownload.RemoveAt(0);
                DownloadFile(fileName.Item1, fileName.Item2);
            }
            UpdateProgress();
            if (!sentFirstWarning)
            {
                if (!CanUseCellular && (HasMeteredConnection = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork))
                {
                    sentFirstWarning = true;
                    downloadPresenter.CellularWarningYesButtonPressed += OnCellularWarningYesButtonPressed;
                    downloadPresenter.CellularWarningNoButtonPressed += OnCellularWarningNoButtonPressed;
                    downloadPresenter.ToggleCellularWarning(true);
                }
            }
            else if(downloadPresenter.IsWarningActive())
            {
                if(Application.internetReachability != NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    ResetCellularWarning();
                    sentFirstWarning = false;
                }
            }

            yield return null;
        }

        PlayerPrefs.SetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, UOItaliaDownloader.PatchVersion.ToString());
        PlayerPrefs.Save();
        StateManager.GoToState<GameState>();
    }

    private void OnCellularWarningYesButtonPressed()
    {
        ResetCellularWarning();
        CanUseCellular = true;
    }

    private void OnCellularWarningNoButtonPressed()
    {
        ResetCellularWarning();
    }

    private void ResetCellularWarning()
    {
        downloadPresenter.ToggleCellularWarning(false);
        downloadPresenter.CellularWarningYesButtonPressed -= OnCellularWarningYesButtonPressed;
        downloadPresenter.CellularWarningNoButtonPressed -= OnCellularWarningNoButtonPressed;
    }

    private void UpdateProgress()
    {
        List<string> toRemove = null;
        foreach (KeyValuePair<string, float[]> kvp in activeDownloadProgressAndFileName)
        {
            if(kvp.Value[0] >= 1.0f)
            {
                downloadPresenter.SetFileDownloaded(kvp.Key);
                DebugInternal.Log($"Download finished - {kvp.Key}");
                ++numberOfFilesDownloaded;
                if (toRemove == null)
                    toRemove = new List<string>();
                toRemove.Add(kvp.Key);
                downloadPresenter.UpdateView(numberOfFilesDownloaded, numberOfFilesToDownload);
            }
            else if(kvp.Value[0] < 0.0f)//error in download
            {
                long filesize = (long)-kvp.Value[0];
                filesToDownload.Insert(0, (kvp.Key, filesize));
                DebugInternal.Log($"Download re-queued - file: {kvp.Key} size: {filesize} - concurrent: {concurrentDownloads}");
                if (toRemove == null)
                    toRemove = new List<string>();
                toRemove.Add(kvp.Key);
                downloadPresenter.SetDownloadProgress(kvp.Key, 0.0f);
            }
            else
                downloadPresenter.SetDownloadProgress(kvp.Key, kvp.Value[0]);
        }
        if(toRemove != null)
        {
            foreach(string file in toRemove)
            {
                --concurrentDownloads;
                activeDownloadProgressAndFileName.Remove(file);
            }
        }
    }

    private void DownloadFile(string fileName, long size)
    {
        var uri = DownloadState.GetUri(ServerConfiguration.FileDownloadServerUrl, port, $"{fileName}.gz");
        var filePath = Path.Combine(pathToSaveFiles, $"{fileName}");
        float[] progress = new float[1] { 0.0f };
        activeDownloadProgressAndFileName[fileName] = progress;
        ++concurrentDownloads;
#if UNITY_EDITOR
        Thread t = new Thread(new ThreadStart(delegate { NewWebClient.DownloadFileWithRange(uri.AbsoluteUri, filePath, size, progress); }));
        t.Start();
#else
        Task.Run(() => NewWebClient.DownloadFileWithRange(uri.AbsoluteUri, filePath, size, progress));
#endif
    }

    public override void Dispose()
    {
        if (downloadCoroutine != null)
        {
            downloadPresenter.StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        filesToDownload = null;
        NewWebClient.IsDisposed = true;
        activeDownloadProgressAndFileName?.Clear();
        activeDownloadProgressAndFileName = null;
        numberOfFilesDownloaded = 0;
        numberOfFilesToDownload = 0;
        base.Dispose();
    }

    private static class NewWebClient
    {
        private static int Timeout = 10000;
        private static int ShortTimeout = 250;
        internal static bool IsDisposed = false;

        internal static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static WebResponse CreateWebRequest(string link, long bytesread)
        {
            IWebProxy defaultProxy = WebRequest.DefaultWebProxy;
            if (defaultProxy != null)
            {
                defaultProxy.Credentials = CredentialCache.DefaultCredentials;
            }
            var req = WebRequest.Create(link);
            HttpWebRequest request = (HttpWebRequest)req;
            request.UseDefaultCredentials = true;
            request.Proxy = defaultProxy;
            request.ReadWriteTimeout = Timeout;
            request.Timeout = Timeout / 5;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 3;
            request.UserAgent = "Mozilla/5.1 (Windows NT 6.2) AppleWebKit/537.38 (KHTML, like Gecko) Safari/537.36";
            request.ServerCertificateValidationCallback = ValidateServerCertificate;
            if (bytesread > 0)
                request.AddRange(bytesread);
            return request.GetResponse();
        }


#if UNITY_EDITOR
        internal static void DownloadFileWithRange(string link, string file, long uncompSize, float[] progress)
#else
        internal static async void DownloadFileWithRange(string link, string file, long uncompSize, float[] progress)
#endif
        {
            string _lastException = null;
            if (File.Exists(file))
                File.Delete(file);
            bool compressed = link.EndsWith(".gz");
            long totalBytesRead = 0;
            long MaxContentLength = 0;
            int attempt = 0;
            while ((MaxContentLength == 0 || totalBytesRead < (compressed ? uncompSize : MaxContentLength)) && attempt < 5 && (CanUseCellular || !HasMeteredConnection))
            {
                if (IsDisposed)
                    return;
                //in caso fallisce ed aumenta il counter, mettiamo una breve pausa tra un tentativo ed il successivo.
                if (attempt > 0)
#if UNITY_EDITOR
                    Thread.Sleep(ShortTimeout);
#else

                    await Task.Delay(ShortTimeout);
#endif
                attempt++;
                WebResponse response = null;
                try
                {
                    FileMode mode = FileMode.Append;
                    if (compressed)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                        totalBytesRead = 0;
                        mode = FileMode.Create;
                    }
                    response = CreateWebRequest(link, totalBytesRead);
                    if (response.ContentLength > MaxContentLength)
                        MaxContentLength = response.ContentLength;
                    if (!compressed)
                        uncompSize = (int)MaxContentLength;

                    using (var responseStream = response.GetResponseStream())
                    {
                        using (FileStream fileStream = new FileStream(file, mode))
                        {
                            var buffer = new byte[32768];
                            int bytesRead;
                            if (compressed)
                            {
                                using (GZipStream decompressionStream = new GZipStream(responseStream, CompressionMode.Decompress))
                                {
                                    while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0 && !IsDisposed && (CanUseCellular || !HasMeteredConnection))
                                    {
                                        totalBytesRead += bytesRead;
                                        fileStream.Write(buffer, 0, bytesRead);
                                        progress[0] = ((totalBytesRead * 1000) / uncompSize) / 1000.0f;
                                        attempt = 0;
                                    }
                                }
                            }
                            else
                            {
                                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0 && !IsDisposed && (CanUseCellular || !HasMeteredConnection))
                                {
                                    totalBytesRead += bytesRead;
                                    fileStream.Write(buffer, 0, bytesRead);
                                    progress[0] = ((totalBytesRead * 1000) / uncompSize) / 1000.0f;
                                    attempt = 0;
                                }
                            }
                        }
                    }
                    response?.Dispose();
                }
                catch (Exception e)
                {
                    _lastException = e.Message;
                    try
                    {
                        response?.Dispose();
                    }
                    catch { }
                }
            }

            if (totalBytesRead >= uncompSize || IsDisposed)
            {
                return;
            }
            else if(!CanUseCellular && HasMeteredConnection)
            {
#if UNITY_EDITOR
                Thread.Sleep(Timeout >> 8);
#else

                await Task.Delay(Timeout >> 8);
#endif
                //let's avoid spamming
                _lastException = "We are on cellular network, but we didn't give any authorization for carrier data download, wait for authorization or just wait to return to wifi or cable networks";
            }
            if (File.Exists(file))
                File.Delete(file);
            DebugInternal.Log($"Cannot download file at: {link} - attempt {attempt} - file length = {totalBytesRead}/{uncompSize} -> {file} - Last Exception: {_lastException}");
            progress[0] = (float)-uncompSize;
        }
    }
}
