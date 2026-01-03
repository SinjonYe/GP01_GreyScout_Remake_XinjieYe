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
    //private int rescuedHostages = 0;

    [Header("Hostage Win Condition")]
    public int requiredDeliveredHostages = 6;     // 需要送达船上的人数
    public int deliveredHostages = 0;             // 已送达人数

    [Header("Hostage Follow List")]
    public Transform followTarget;                // 跟随目标（一般就是 player）
    private readonly System.Collections.Generic.List<HostageFollower> followers
        = new System.Collections.Generic.List<HostageFollower>();

    [Header("Carry Hostage")]
    public Transform carryPoint;   // 拖到 PlayerArmature/CarryPoint
    private HostageFollower carriedFollower;
    public bool isCarryMode = false;

    public Animator playerAnimator; // 新增：玩家Animator（拖 PlayerArmature 上的 Animator）

    [Header("Carry Hand Points")]
    public Transform playerHandGripPoint; // 你层级里的 HandGripPoint

    public bool isRespawning = false;

    private void RefreshCarryChain()
    {
        if (followers.Count == 0) return;
        if (playerHandGripPoint == null) return;

        followers.Sort((a, b) => a.followIndex.CompareTo(b.followIndex));

        Transform currentCarryFrom = playerHandGripPoint;

        for (int i = 0; i < followers.Count; i++)
        {
            var f = followers[i];
            if (f == null) continue;

            f.playerRoot = player;

            // 如果已经 carried，也强制重新绑定 carryPoint（保证链条正确）
            f.SetCarried(currentCarryFrom);

            currentCarryFrom = (f.handGripPoint != null) ? f.handGripPoint : f.transform;
        }
    }

    public void StartCarry()
    {
        if (isCarryMode) return;
        if (followers.Count == 0) return;
        if (playerHandGripPoint == null) { Debug.LogError("playerHandGripPoint is null"); return; }

        isCarryMode = true;
        RefreshCarryChain();

        if (playerAnimator != null)
            playerAnimator.SetBool("Carry", true);

        Debug.Log("Carry chain started.");
    }

    public void StopCarry()
    {
        if (!isCarryMode) return;

        for (int i = 0; i < followers.Count; i++)
        {
            var f = followers[i];
            if (f == null) continue;
            f.ReleaseCarried();
        }

        isCarryMode = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("Carry", false);

        Debug.Log("Carry chain stopped.");
    }




    // 新增：防止重复触发胜利（作用：按E不会触发多次，也避免StopAllCoroutines误伤）
    private bool isWinning = false;

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

    private void DropFollowersForRespawn()
    {
        // 如果在拉手状态，先停止（会把 carried 释放并尝试回 NavMesh）
        if (isCarryMode)
            StopCarry();

        // 把跟随队伍解散，但不移动他们位置
        for (int i = 0; i < followers.Count; i++)
        {
            var f = followers[i];
            if (f == null) continue;

            // 停止跟随（不再自动贴着玩家走）
            f.enabled = false;

            // 停止 agent（让他们站在原地）
            if (f.agent != null)
            {
                if (f.agent.enabled && f.agent.isOnNavMesh)
                {
                    f.agent.ResetPath();
                    f.agent.isStopped = true;
                }
            }

            // 让这个人质恢复为“可救援状态”，并启用瞬间救援
            HostageRescue rescue = f.GetComponent<HostageRescue>();
            if (rescue != null)
            {
                rescue.ResetForReRescueInstant();
            }
        }

        followers.Clear();

        // 玩家动画状态也复位
        isCarryMode = false;
        if (playerAnimator != null) playerAnimator.SetBool("Carry", false);

        // UI 安全清理
        if (RescueUIManager.Instance != null)
            RescueUIManager.Instance.HideAll();
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // 1) Fade to black
        yield return StartCoroutine(Fade(1));

        // 2) 重生前解散人质队伍（他们留在死亡位置）
        DropFollowersForRespawn();

        // 3) 传送玩家
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = playerSpawnPoint.position;
        player.rotation = playerSpawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        // 4) 敌人脱战
        if (enemyController != null)
            enemyController.HardResetAfterRespawn();

        // 5) Fade back
        yield return StartCoroutine(Fade(0));

        // 6) 保护期
        yield return new WaitForSeconds(0.5f);
        isRespawning = false;
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

    private IEnumerator RefreshCarryNextFrame()
    {
        yield return null; // 等一帧，让 NavMeshAgent/Transform 状态稳定
        RefreshCarryChain();
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

        if (isCarryMode)
            StartCoroutine(RefreshCarryNextFrame());

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

        UpdateHostageUI();
    }

    void UpdateHostageUI()
    {
        if (hostageCountText != null)
        {
            hostageCountText.text = $"{deliveredHostages} / {requiredDeliveredHostages} rescued";
        }
    }

    // ---------- Win ----------
    public void TriggerWin()
    {
        // 新增：防重复触发（作用：只会进入一次胜利流程）
        if (isWinning) return;
        isWinning = true;
        // 防止重复触发
        StopAllCoroutines();
        StartCoroutine(VictoryRoutine());
    }


    IEnumerator VictoryRoutine()
    {
        //先淡出
        yield return StartCoroutine(Fade(1));

        // 调用 FlowManager 打开胜利界面
        if (FlowManager.Instance != null)
            FlowManager.Instance.ShowWin();
        else
            Debug.LogError("FlowManager.Instance is NULL，无法显示胜利界面。");
    }


}
