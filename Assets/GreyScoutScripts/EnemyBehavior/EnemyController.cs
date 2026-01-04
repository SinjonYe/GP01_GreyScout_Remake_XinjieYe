using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState
{
    Patrol,
    Chase,
    Lost,
    Search
}

public class EnemyController : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Patrol;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public VisionConeRotate visionRotate;
    public EnemyPatrol patrol;

    [Header("Speeds")]
    public float patrolSpeed = 2f;   // Patrol Speed
    public float chaseSpeed = 4f;    // Chase Speed

    [Header("Chase Settings")]
    public float chaseTimeBeforeLost = 2f;
    private float chaseTimer = 0f;

    public EnemyAlertUI alertUI;

    [Header("Suspicious Settings")]
    public float suspiciousTime = 4f; // Enemy turns to face player when player stays in proximity for a certain duration
    private float suspiciousTimer = 0f;
    private bool isSuspicious = false;
    private Transform suspiciousTarget; // Player Position

    private Vector3 lastKnownPlayerPos; // Record player position


    private void Start()
    {
        ResetPatrol();
    }

    private void Update()
    {
        // ---------------- Chase ----------------
        if (currentState == EnemyState.Chase)
        {
            agent.SetDestination(player.position);

            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeBeforeLost)
            {
                ChangeState(EnemyState.Search); // Search state
            }

        }
        // ---------------- Search state (go to player's last position) ----------------
        else if (currentState == EnemyState.Search)
        {
            // 1. Move to the player's last seen position
            agent.speed = patrolSpeed; // Move slowly during search state
            agent.SetDestination(lastKnownPlayerPos);

            float dist = Vector3.Distance(transform.position, lastKnownPlayerPos);

            if (dist < 0.5f)
            {
                // 2. Look left and right on the spot after arrival (search animation)
                StartCoroutine(SearchRoutine());
                ChangeState(EnemyState.Lost); // Avoid repeated activation
            }
        }
        // ---------------- Return to patrol after losing player ----------------
        else if (currentState == EnemyState.Lost)
        {
            ResetPatrol();
            alertUI.Hide();   // Return to patrol and hide icon
        }

        // ---------------- Alert timer logic (player nearby but not spotted) ----------------
        if (isSuspicious && currentState == EnemyState.Patrol)
        {
            suspiciousTimer += Time.deltaTime;

            // Turn to player after alert time (4s) is reached
            if (suspiciousTimer >= suspiciousTime)
            {
                Vector3 lookDir = suspiciousTarget.position - transform.position;
                lookDir.y = 0;

                // Enemy turns to player (smooth rotation)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 3f
                );

                //  If player is within 30° ahead after rotation → Consider spotted by rule/ 如果转向后玩家已经在前方 30° 内 → 直接认为“被看到了”
                float angle = Vector3.Angle(transform.forward, lookDir);

                if (angle < 30f)
                {
                    // End alert state
                    isSuspicious = false;
                    alertUI.Hide();

                    // Player is truly spotted
                    PlayerSeen();
                }
            }
        }
        
    }

    // Called when player enters vision range/ 玩家进入视野时调用
    public void ChasePlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.isRespawning) return;
        if (currentState == EnemyState.Chase) return; // Avoid repeated calls

        currentState = EnemyState.Chase;
        chaseTimer = 0;

        // Stop patrol script and vision rotation
        patrol.enabled = false;
        visionRotate.enabled = false;

        agent.speed = chaseSpeed;

        //  Force display on chase start
        alertUI.ShowAlert();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    //Search Action
    IEnumerator SearchRoutine()
    {
        float searchTime = 2f;
        float timer = 0f;

        while (timer < searchTime)
        {
            timer += Time.deltaTime;

            // Look left and right
            float angle = Mathf.Sin(Time.time * 3f) * 45f; // Look left and right by 45°
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + angle * Time.deltaTime, 0);

            yield return null;
        }

        // End search → Return to patrol
        ResetPatrol();
    }


    // -------------------- Priority: Reset Patrol  --------------------
    public void ResetPatrol()
    {
        // Enable patrol script and vision rotation
        patrol.enabled = true;
        visionRotate.enabled = true;

        // Set patrol movement speed
        agent.speed = patrolSpeed;

        // Restart patrol from the first waypoint (currentIndex auto managed)/ 让巡逻脚本从第一个点重新开始（你的脚本用 currentIndex 自动管理）
        agent.Warp(patrol.patrolPoints[0].position);
        agent.SetDestination(patrol.patrolPoints[0].position);

        // Disable chase timer
        chaseTimer = 0;

        // Switch state
        ChangeState(EnemyState.Patrol);

        if (alertUI != null)
        {
            alertUI.Hide();
        }

    }

    public void BecomeSuspicious(Transform player)
    {
        if (currentState == EnemyState.Chase) return; // Do not enter alert state while chasing
        if (isSuspicious) return; // Alert timer is already active

        isSuspicious = true;
        suspiciousTimer = 0f;
        suspiciousTarget = player;

        alertUI.ShowSuspicious(); // Show question mark ?
    }

    // Cancel alert state
    public void CancelSuspicious()
    {
        if (isSuspicious)
        {
            isSuspicious = false;         // Stop timer
            suspiciousTimer = 0f;
            alertUI.Hide();               // Hide question mark
            suspiciousTarget = null;      // Clear target
        }
    }


    public void PlayerNearButNotSeen()
    {
        // Only show question mark? in patrol state 
        if (currentState == EnemyState.Patrol && alertUI != null)
        {
            alertUI.ShowSuspicious();
        }
    }

    public void PlayerSeen()
    {
        if (GameManager.Instance != null && GameManager.Instance.isRespawning) return;

        if (alertUI != null)
            alertUI.ShowAlert();

        // Save the player's last seen position
        lastKnownPlayerPos = player.position;

        // Enter chase state
        ChasePlayer();
    }


    public void PlayerOutOfVision()
    {
        if (alertUI != null)
        {
            alertUI.Hide();
        }
    }

    public void HardResetAfterRespawn()
    {
        // 1) Stop all search coroutines/behaviors
        StopAllCoroutines();

        // 2) Clear alert state
        isSuspicious = false;
        suspiciousTimer = 0f;
        suspiciousTarget = null;

        // 3) Clear chase state
        chaseTimer = 0f;
        lastKnownPlayerPos = transform.position; // Assign a safe default value

        // 4) Hide UI immediately
        if (alertUI != null) alertUI.Hide();

        // 5) Force return to patrol state
        ResetPatrol();
    }

}
