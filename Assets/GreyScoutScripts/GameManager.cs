using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform player;
    public Transform playerSpawnPoint;
    public CanvasGroup fadeScreen;

    public EnemyController enemyController;
    public float fadeDuration = 1f;

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
}
