using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;

    [Header("UI Roots")]
    public GameObject startScreen;   // StartScreen Panel
    public GameObject hud;           // HUD Root
    public GameObject winScreen;     // WinScreen Panel

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowStart(); // Default enter start screen on game launch
    }

    // Enter Start Screen state
    public void ShowStart()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (hud != null) hud.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);

        Time.timeScale = 0f;  // Pause game logic (Player & enemy freeze)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Enter game from start screen
    public void StartGame()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (hud != null) hud.SetActive(true);
        if (winScreen != null) winScreen.SetActive(false);

        Time.timeScale = 1f;  // Normal time scale
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Enter win screen (Called by GameManager)
    public void ShowWin()
    {
        if (hud != null) hud.SetActive(false);
        if (winScreen != null) winScreen.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Restart level
    public void RestartGame()
    {
        Time.timeScale = 1f;  // Prevent pause state carry to new scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        // Press R to restart in win screen
        if (winScreen != null && winScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }
    }
}
