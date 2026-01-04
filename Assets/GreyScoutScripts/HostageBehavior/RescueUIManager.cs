using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RescueUIManager : MonoBehaviour
{
    public static RescueUIManager Instance;

    [Header("UI ÔªËØ")]
    public CanvasGroup uiGroup;      // UI container
    public Slider progressBar;       // Progress bar
    public TextMeshProUGUI pressEText; // Press E text

    private void Awake()
    {
        Instance = this;
        HideAll();
    }

    // Show Press E
    public void ShowPressE()
    {
        uiGroup.alpha = 1;
        SetAlpha(pressEText, 1);
    }

    // Hide Press E
    public void HidePressE()
    {
        SetAlpha(pressEText, 0);
    }

    // Show progress bar
    public void ShowProgressBar()
    {
        uiGroup.alpha = 1;
        if (progressBar != null)
            progressBar.gameObject.SetActive(true);
    }

    // Hide progress bar
    public void HideProgressBar()
    {
        if (progressBar != null)
            progressBar.gameObject.SetActive(false);
    }

    // Set progress bar value
    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = value;
    }

    // Hide all UI completely
    public void HideAll()
    {
        uiGroup.alpha = 0;
        HideProgressBar();
        HidePressE();
        SetProgress(0);
    }

    // Common func : Set alpha
    private void SetAlpha(TMP_Text txt, float a)
    {
        var c = txt.color;
        c.a = a;
        txt.color = c;
    }
}
