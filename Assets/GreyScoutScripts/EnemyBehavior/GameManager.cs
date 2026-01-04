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
    public TMP_Text hostageCountText;   // Display "1 / 1 rescued" on HUD

    public int totalHostages = 1;   // Total hostages in scene
    //private int rescuedHostages = 0;

    [Header("Hostage Win Condition")]
    public int requiredDeliveredHostages = 6;     // Number of people need to send to the boat
    public int deliveredHostages = 0;             // Number of people already delivered

    [Header("Hostage Follow List")]
    public Transform followTarget;                // Follow target ( player)
    private readonly System.Collections.Generic.List<HostageFollower> followers
        = new System.Collections.Generic.List<HostageFollower>();

    [Header("Carry Hostage")]
    public Transform carryPoint;   // Drag to PlayerArmature/CarryPoint
    private HostageFollower carriedFollower;
    public bool isCarryMode = false;

    public Animator playerAnimator; // Player Animator (drag Animator on PlayerArmature)

    [Header("Carry Hand Points")]
    public Transform playerHandGripPoint; // HandGripPoint in your layer

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

            // Rebind carryPoint forcibly if already carried (ensure correct chain)
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


    // Prevent repeated victory trigger/ 防止重复触发胜利（作用：按E不会触发多次，也避免StopAllCoroutines误伤）
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
        // Stop holding state first if active (release carried target and try to return to NavMesh)
        if (isCarryMode)
            StopCarry();

        // Dismiss follow team without moving their positions
        for (int i = 0; i < followers.Count; i++)
        {
            var f = followers[i];
            if (f == null) continue;

            // Stop following (no longer stick to player automatically)
            f.enabled = false;

            // Stop agent (make them stand still)
            if (f.agent != null)
            {
                if (f.agent.enabled && f.agent.isOnNavMesh)
                {
                    f.agent.ResetPath();
                    f.agent.isStopped = true;
                }
            }

            // Restore hostage to rescuable state, enable instant rescue
            HostageRescue rescue = f.GetComponent<HostageRescue>();
            if (rescue != null)
            {
                rescue.ResetForReRescueInstant();
            }
        }

        followers.Clear();

        // Reset player animation state
        isCarryMode = false;
        if (playerAnimator != null) playerAnimator.SetBool("Carry", false);

        // Safe clean up UI
        if (RescueUIManager.Instance != null)
            RescueUIManager.Instance.HideAll();
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // 1) Fade to black
        yield return StartCoroutine(Fade(1));

        // 2) Dismiss hostage team before respawn (they stay at death position)
        DropFollowersForRespawn();

        // 3) Teleport player
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = playerSpawnPoint.position;
        player.rotation = playerSpawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        // 4) Enemies disengage combat
        if (enemyController != null)
            enemyController.HardResetAfterRespawn();

        // 5) Fade back
        yield return StartCoroutine(Fade(0));

        // 6) Invulnerability period
        yield return new WaitForSeconds(0.5f);
        isRespawning = false;
    }



    IEnumerator Fade(float target)
    {
        float start = fadeScreen.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;   // Support pause
            fadeScreen.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator RefreshCarryNextFrame()
    {
        yield return null; // Wait for one frame to stabilize NavMeshAgent/Transform state
        RefreshCarryChain();
    }


    // --- Hostage Rescue System ---
    public void HostageRescuedAndFollow(HostageRescue hostage)
    {
        if (hostage == null) return;

        // Disable rescue interaction, avoid repeated trigger
        hostage.DisableRescueInteraction();

        // Add HostageFollower component for hostage
        HostageFollower follower = hostage.GetComponent<HostageFollower>();
        if (follower == null)
            follower = hostage.gameObject.AddComponent<HostageFollower>();

        // Ensure NavMeshAgent is attached (hostage must be movable)
        if (follower.agent == null)
            follower.agent = hostage.GetComponent<NavMeshAgent>();

        if (follower.agent == null)
        {
            Debug.LogError($"[{hostage.name}] 缺少 NavMeshAgent，无法跟随。");
            return;
        }

        // Configure follow target and team index
        follower.followTarget = followTarget;
        follower.followIndex = followers.Count;

        // Enable follow script
        follower.enabled = true;

        // Ensure agent is usable
        follower.agent.isStopped = false;

        // Join the team
        followers.Add(follower);

        Debug.Log($"Hostage join follow: {followers.Count} followers now.");

        if (isCarryMode)
            StartCoroutine(RefreshCarryNextFrame());

    }

    public void DeliverAllFollowersToShip(Transform shipStandPoint = null)
    {
        if (followers.Count == 0) return;

        // Deliver all following hostages
        int count = followers.Count;
        deliveredHostages += count;

        // Make hostages board the boat (Method1: Hide directly; Method2: Move to inside-boat position)
        foreach (var f in followers)
        {
            if (f == null) continue;

            if (shipStandPoint != null)
            {
                // Teleport to a point inside the boat (optional)
                f.transform.position = shipStandPoint.position;
                f.transform.rotation = shipStandPoint.rotation;
            }

            // Hide after delivery (represent boarding the boat)
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
        // Prevent repeated trigger (Effect: Only enter victory process once)
        if (isWinning) return;
        isWinning = true;
        // Prevent duplicate trigger
        StopAllCoroutines();
        StartCoroutine(VictoryRoutine());
    }


    IEnumerator VictoryRoutine()
    {
        // Fade out first
        yield return StartCoroutine(Fade(1));

        // Call FlowManager to open victory UI
        if (FlowManager.Instance != null)
            FlowManager.Instance.ShowWin();
        else
            Debug.LogError("FlowManager.Instance is NULL，无法显示胜利界面。");
    }


}
