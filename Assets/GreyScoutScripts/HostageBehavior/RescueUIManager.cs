using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RescueUIManager : MonoBehaviour
{
    public static RescueUIManager Instance;

    [Header("UI 元素")]
    public CanvasGroup uiGroup;      // 整个 UI 的容器
    public Slider progressBar;       // 进度条
    public TextMeshProUGUI pressEText; // Press E 文本

    private void Awake()
    {
        Instance = this;
        HideAll();
    }

    // 显示 Press E
    public void ShowPressE()
    {
        uiGroup.alpha = 1;
        SetAlpha(pressEText, 1);
    }

    // 隐藏 Press E
    public void HidePressE()
    {
        SetAlpha(pressEText, 0);
    }

    // 显示进度条
    public void ShowProgressBar()
    {
        uiGroup.alpha = 1;
        progressBar.gameObject.SetActive(true);
    }

    // 隐藏进度条
    public void HideProgressBar()
    {
        progressBar.gameObject.SetActive(false);
    }

    // 设置进度条数值
    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = value;
    }

    // 完整隐藏全部 UI（最重要）
    public void HideAll()
    {
        uiGroup.alpha = 0;
        HideProgressBar();
        HidePressE();
        SetProgress(0);
    }

    // 通用函数：修改透明度
    private void SetAlpha(TMP_Text txt, float a)
    {
        var c = txt.color;
        c.a = a;
        txt.color = c;
    }
}
