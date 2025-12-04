using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RescueUIManager : MonoBehaviour
{
    public static RescueUIManager Instance;

    public CanvasGroup uiGroup; //Rescue Bar
    public Slider progressBar; //Slider

    public TextMeshProUGUI pressEText;   // Press E Text

    private void Awake()
    {
        Instance = this;
        Hide();
        HidePressE();
    }

    public void Show() // 显示进度条 UI
    {
        uiGroup.alpha = 1;
    }

    public void Hide() // 隐藏进度条 UI
    {
        uiGroup.alpha = 0;
        if (progressBar != null)
            progressBar.value = 0;
    }

    public void ShowPressE() // 显示 Press E
    {
        SetAlpha(pressEText, 1);
    }

    public void HidePressE() // 隐藏 Press E
    {
        SetAlpha(pressEText, 0);
    }

    void SetAlpha(TMP_Text txt, float a)
    {
        var c = txt.color;
        c.a = a;
        txt.color = c;
    }

    public void SetProgress(float value) // 设置进度条
    {
        if (progressBar != null)
            progressBar.value = value;
    }
}
