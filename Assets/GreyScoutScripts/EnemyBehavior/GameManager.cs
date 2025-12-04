using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform player;
    public Transform playerSpawnPoint;
    public CanvasGroup fadeScreen;

    public EnemyController enemyController;
    public float fadeDuration = 1f;

    public int totalHostages = 1;   // 场景里一共有多少人质
    private int rescuedHostages = 0;

    public TextMeshProUGUI victoryText;

    public void ShowVictory()
    {
        var c = victoryText.color;
        c.a = 1;
        victoryText.color = c;
    }

    private void Awake()
    {
        Instance = this;
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
            time += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }
    }

    public void HostageRescued()
    {
        rescuedHostages++;

        Debug.Log("Hostage rescued: " + rescuedHostages + " / " + totalHostages);

        if (rescuedHostages >= totalHostages)
        {
            StartCoroutine(VictorySequence());
        }
    }

    IEnumerator VictorySequence()
    {
        // 简单做法：直接淡出黑屏，代表通关
        yield return StartCoroutine(Fade(1));

        Debug.Log("YOU WIN! All hostages rescued!");

        // TODO：这里你后面可以做：
        // - 切换场景
        // - 显示胜利UI
        // - 返回主菜单 等等

        // 显示胜利文字
        ShowVictory();
    }


}
