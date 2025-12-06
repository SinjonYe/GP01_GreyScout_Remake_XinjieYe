using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;

    [Header("UI Roots")]
    public GameObject startScreen;   // StartScreen Panel
    public GameObject hud;           // HUD 根对象
    public GameObject winScreen;     // WinScreen Panel

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowStart();
    }

    // 进入「开始界面」状态
    public void ShowStart()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (hud != null) hud.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);

        Time.timeScale = 0f;  // 暂停游戏逻辑（玩家、敌人不动）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 从开始界面进入游戏
    public void StartGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (hud != null) hud.SetActive(true);
        if (winScreen != null) winScreen.SetActive(false);

        Time.timeScale = 1f;  // 正常时间
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 进入胜利界面（由 GameManager 调用）
    public void ShowWin()
    {
        if (hud != null) hud.SetActive(false);
        if (winScreen != null) winScreen.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 重新开始关卡
    public void RestartGame()
    {
        Time.timeScale = 1f;  // 防止暂停状态带到新场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        // 在胜利界面按 R 也可以重开
        if (winScreen != null && winScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }
    }
}
