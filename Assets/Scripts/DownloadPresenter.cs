using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPresenter : MonoBehaviour
{
    [SerializeField]
    private Text counterText;

    [SerializeField]
    private Text errorText;

    [SerializeField]
    private GameObject filesScrollView;

    [SerializeField]
    private FileNameView fileNameViewInstance;

    [SerializeField]
    private Button backOrCancelButton;

    [SerializeField]
    private Text backOrCancelButtonText;

    [SerializeField]
    private GameObject cellularWarningParent;

    [SerializeField]
    private Text cellularWarningText;

    [SerializeField]
    private Button cellularWarningYesButton;

    [SerializeField]
    private Text cellularWarningYesButtonText;

    [SerializeField]
    private Button cellularWarningNoButton;

    public Action BackButtonPressed;
    public Action CellularWarningYesButtonPressed;
    public Action CellularWarningNoButtonPressed;

    private readonly Dictionary<string, FileNameView> fileNameToFileNameView = new Dictionary<string, FileNameView>();

    private void OnEnable()
    {
        backOrCancelButton.onClick.AddListener(() => BackButtonPressed?.Invoke());
        cellularWarningYesButton.onClick.AddListener(() => CellularWarningYesButtonPressed?.Invoke());
        cellularWarningNoButton.onClick.AddListener(() => CellularWarningNoButtonPressed?.Invoke());

        cellularWarningParent.gameObject.SetActive(false);
        backOrCancelButton.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
        fileNameViewInstance.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        backOrCancelButton.onClick.RemoveAllListeners();
        counterText.text = "Checking patch version...";
    }

    public void ShowError(string error)
    {
        backOrCancelButton.gameObject.SetActive(true);
        backOrCancelButtonText.text = "Back";
        errorText.text = error;
        errorText.gameObject.SetActive(true);
    }

    public void UpdateCheckingView(int todownload, int numberchecked, int numbertocheck)
    {
        if (counterText != null)
        {
            counterText.text = $"Files [update/check/all]: {todownload}/{numberchecked}/{numbertocheck}";
        }
    }

    public void UpdateView(int numberOfFilesDownloaded, int numberOfFilesToDownload)
    {
        if (counterText != null)
        {
            counterText.text = $"Files [downloaded/total]: {numberOfFilesDownloaded}/{numberOfFilesToDownload}";
        }
    }

    public void SetFileDownloaded(string file)
    {
        if(fileNameToFileNameView.TryGetValue(file, out var fileNameView))
        {
            fileNameView.SetDownloaded();
        }
    }

    public void SetFileList(List<(string, long)> filesList)
    {
        var filenameTextInstanceGameObject = fileNameViewInstance.gameObject;
        var filenameTextInstanceTransformParent = fileNameViewInstance.transform.parent;
        filesList.ForEach(file =>
        {
            var newFileNameText = Instantiate(filenameTextInstanceGameObject, filenameTextInstanceTransformParent).GetComponent<FileNameView>();
            newFileNameText.SetText(file.Item1);
            fileNameToFileNameView.Add(file.Item1, newFileNameText);
            newFileNameText.gameObject.SetActive(true);
        });
        filesScrollView.SetActive(true);
        backOrCancelButton.gameObject.SetActive(true);
        backOrCancelButtonText.text = "Cancel";
    }

    public void ClearFileList()
    {
        foreach (var keyValuePair in fileNameToFileNameView)
        {
            if (keyValuePair.Value != null)
            {
                Destroy(keyValuePair.Value.gameObject);
            }
        }
        
        fileNameToFileNameView.Clear();
        filesScrollView.SetActive(false);
    }

    public void SetDownloadProgress(string file, float progress)
    {
        if (fileNameToFileNameView.TryGetValue(file, out var fileNameView))
        {
            fileNameView?.SetProgress(progress);
        }
    }

    public bool IsWarningActive()
    {
        return cellularWarningParent.activeSelf;
    }

    public void ToggleCellularWarning(bool enabled)
    {
        cellularWarningText.text = "You are not connected to a Wi-Fi network.\n\nDo you want to continue downloading files?";
        cellularWarningParent.SetActive(enabled);
    }

    public void ToggleDownloaderWarning(bool enabled, string text = "")
    {
        cellularWarningText.text = text;
        cellularWarningParent.SetActive(enabled);
    }

    public void InsufficientDiskSpaceWarning(string message)
    {
        cellularWarningNoButton.gameObject.SetActive(false);
        cellularWarningText.text = message;
        cellularWarningYesButtonText.text = "Quit";
    }
}