using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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

    [Header("Hostage Win Condition")]
    public int requiredDeliveredHostages = 6;     // 需要送达船上的人数
    public int deliveredHostages = 0;             // 已送达人数

    [Header("Hostage Follow List")]
    public Transform followTarget;                // 跟随目标（一般就是 player）
    private readonly System.Collections.Generic.List<HostageFollower> followers
        = new System.Collections.Generic.List<HostageFollower>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateHostageUI();

        if (followTarget == null && player != null)
            followTarget = player;
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
    public void HostageRescuedAndFollow(HostageRescue hostage)
    {
        if (hostage == null) return;

        // 关闭营救交互，避免重复触发
        hostage.DisableRescueInteraction();

        // 给人质加/获取 HostageFollower
        HostageFollower follower = hostage.GetComponent<HostageFollower>();
        if (follower == null)
            follower = hostage.gameObject.AddComponent<HostageFollower>();

        // 确保有 NavMeshAgent（人质必须能走）
        if (follower.agent == null)
            follower.agent = hostage.GetComponent<NavMeshAgent>();

        if (follower.agent == null)
        {
            Debug.LogError($"[{hostage.name}] 缺少 NavMeshAgent，无法跟随。");
            return;
        }

        // 配置跟随目标与队伍序号
        follower.followTarget = followTarget;
        follower.followIndex = followers.Count;

        // 开启跟随脚本
        follower.enabled = true;

        // 确保 agent 可用
        follower.agent.isStopped = false;

        // 加入队伍
        followers.Add(follower);

        Debug.Log($"Hostage join follow: {followers.Count} followers now.");
    }

    public void DeliverAllFollowersToShip(Transform shipStandPoint = null)
    {
        if (followers.Count == 0) return;

        // 把所有跟随的人质送达
        int count = followers.Count;

        deliveredHostages += count;

        // 让人质进入船（做法1：直接隐藏；做法2：移动到船内位置）
        foreach (var f in followers)
        {
            if (f == null) continue;

            if (shipStandPoint != null)
            {
                // 传送到船内某个点（可选）
                f.transform.position = shipStandPoint.position;
                f.transform.rotation = shipStandPoint.rotation;
            }

            // 送达后隐藏（代表上船）
            f.gameObject.SetActive(false);
        }

        followers.Clear();

        Debug.Log($"Delivered hostages: {deliveredHostages}/{requiredDeliveredHostages}");
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
