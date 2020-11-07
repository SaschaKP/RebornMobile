using UnityEngine;
using UnityEngine.UI;

public class FileNameView : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Image fillGreenImage;

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetDownloaded()
    {
        text.color = Color.green;
        fillGreenImage.enabled = false;
    }

    public void SetProgress(float progress)
    {
        if(fillGreenImage.enabled == false)
            fillGreenImage.enabled = true;
        fillGreenImage.fillAmount = progress;
    }
}
