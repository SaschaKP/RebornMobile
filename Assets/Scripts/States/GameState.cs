using System.IO;
using System.Linq;
using UnityEngine;

public class GameState : IState
{
    private readonly ClientRunner clientRunner;
    private readonly ErrorPresenter errorPresenter;
    private readonly Canvas inGameDebugConsoleCanvas;

    public GameState(ClientRunner clientRunner, ErrorPresenter errorPresenter, Canvas inGameDebugConsoleCanvas)
    {
        this.clientRunner = clientRunner;
        this.errorPresenter = errorPresenter;
        this.inGameDebugConsoleCanvas = inGameDebugConsoleCanvas;
    }

    internal static bool HasValidUOFiles(out string configPath)
    {
        if (Application.isMobilePlatform || string.IsNullOrEmpty(ServerConfiguration.ClientPathForUnityEditor))
        {
            configPath = ServerConfiguration.GetPathToSaveFiles();
            var configurationDirectory = new DirectoryInfo(configPath);
            var files = configurationDirectory.GetFiles().Select(x => x.Name).ToList();
            var hasInstallFiles = UtilityMethods.EssentialUoFilesExist(files);
            if (hasInstallFiles == false)
            {
                PlayerPrefs.SetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, "0");
                PlayerPrefs.Save();
                return false;
            }
            return true;
        }
        configPath = string.Empty;
        return false;
    }

    private static bool _Attempted = false;
    public void Enter()
    {
        errorPresenter.BackButtonClicked += Exit;
        clientRunner.OnExiting += Exit;
        clientRunner.OnError += OnError;
        if (!HasValidUOFiles(out string configPath))
        {
            if (_Attempted == false)
            {
                _Attempted = true;
                StateManager.GoToState<DownloadState>();
            }
            else
            {
                var error = $"Server configuration directory does not contain UO files such as anim.mul or animationFrame1.uop. Make sure that the UO files have been downloaded or transferred properly.\nPath: {configPath}";
                OnError(error);
            }
            return;
        }

        clientRunner.enabled = true;
        clientRunner.StartGame();
    }

    private void OnError(string error)
    {
        clientRunner.enabled = false;
        errorPresenter.gameObject.SetActive(true);
        errorPresenter.SetErrorText(error);
        inGameDebugConsoleCanvas.enabled = true;
    }

    public void Exit()
    {
        clientRunner.enabled = false;
        errorPresenter.gameObject.SetActive(false);
        
        errorPresenter.BackButtonClicked -= Exit;
        clientRunner.OnExiting -= Exit;
        clientRunner.OnError -= OnError;
        Application.Quit();
    }
}