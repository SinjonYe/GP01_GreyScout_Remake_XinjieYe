using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Respawn")]
    public Transform player;
    public Transform playerSpawnPoint;
    public CanvasGroup fadeScreen;

    public EnemyController enemyController;
    public float fadeDuration = 1f;

    [Header("Hostage UI")]
    public TMP_Text hostageCountText;   // HUD 上显示 "1 / 1 rescued"

    public int totalHostages = 1;   // 场景里一共有多少人质
    private int rescuedHostages = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateHostageUI();
    }

    public void PlayerCaught()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // Fade to black
        yield return StartCoroutine(Fade(1));

        // --- 修复复活位置不稳定 ---
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 强制移动到玩家出生点
        player.position = playerSpawnPoint.position;
        player.rotation = playerSpawnPoint.rotation;

        // 重新启用 CharacterController（极其重要）
        if (cc != null) cc.enabled = true;
        // --------------------------------

        // Reset enemy（让敌人回到巡逻状态）
        enemyController.ResetPatrol();

        // Fade back to gameplay
        yield return StartCoroutine(Fade(0));
    }


    IEnumerator Fade(float target)
    {
        float start = fadeScreen.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;   // 支持暂停
            fadeScreen.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }
    }

    // --- Hostage Rescue System ---
    public void HostageRescued()
    {
        rescuedHostages++;
        UpdateHostageUI();

        Debug.Log("Hostage rescued: " + rescuedHostages + " / " + totalHostages);

        if (rescuedHostages >= totalHostages)
        {
            StartCoroutine(VictoryRoutine());
        }
    }

    void UpdateHostageUI()
    {
        if (hostageCountText != null)
        {
            hostageCountText.text = $"{rescuedHostages} / {totalHostages} rescued";
        }
    }

    IEnumerator VictoryRoutine()
    {
        yield return StartCoroutine(Fade(1));

        // 调用 FlowManager 打开胜利界面
        FlowManager.Instance.ShowWin();
    }


}
