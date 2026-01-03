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
    public float patrolSpeed = 2f;   // 巡逻速度
    public float chaseSpeed = 4f;    // 追击速度

    [Header("Chase Settings")]
    public float chaseTimeBeforeLost = 2f;
    private float chaseTimer = 0f;

    public EnemyAlertUI alertUI;

    [Header("Suspicious Settings")]
    public float suspiciousTime = 4f; // 玩家在旁边多久后敌人转头
    private float suspiciousTimer = 0f;
    private bool isSuspicious = false;
    private Transform suspiciousTarget; // 玩家位置

    private Vector3 lastKnownPlayerPos; //记录玩家位置


    private void Start()
    {
        ResetPatrol();
    }

    private void Update()
    {
        // ---------------- 追击 ----------------
        if (currentState == EnemyState.Chase)
        {
            agent.SetDestination(player.position);

            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeBeforeLost)
            {
                ChangeState(EnemyState.Search); // 搜索状态
            }

        }
        // ---------------- 搜索状态（去玩家最后位置） ----------------
        else if (currentState == EnemyState.Search)
        {
            // 1. 移动到玩家最后出现的位置
            agent.speed = patrolSpeed; // 搜索时用慢速
            agent.SetDestination(lastKnownPlayerPos);

            float dist = Vector3.Distance(transform.position, lastKnownPlayerPos);

            if (dist < 0.5f)
            {
                // 2. 到达后原地左右环顾（搜索动画）
                StartCoroutine(SearchRoutine());
                ChangeState(EnemyState.Lost); // 避免重复启动
            }
        }
        // ---------------- 失去玩家后回到巡逻 ----------------
        else if (currentState == EnemyState.Lost)
        {
            ResetPatrol();
            alertUI.Hide();   // 回到巡逻隐藏图标
        }

        // ---------------- 警觉计时逻辑（玩家在附近但没被看到） ----------------
        if (isSuspicious && currentState == EnemyState.Patrol)
        {
            suspiciousTimer += Time.deltaTime;

            // 达到警觉时间（4秒）后转向玩家
            if (suspiciousTimer >= suspiciousTime)
            {
                Vector3 lookDir = suspiciousTarget.position - transform.position;
                lookDir.y = 0;

                // 敌人转向玩家（平滑旋转）
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 3f
                );

                // 如果转向后玩家已经在前方 30° 内 → 江湖规矩：直接认为“被看到了”
                float angle = Vector3.Angle(transform.forward, lookDir);

                if (angle < 30f)
                {
                    // 结束警觉
                    isSuspicious = false;
                    alertUI.Hide();

                    // 真正看到玩家
                    PlayerSeen();
                }
            }
        }
        
    }

    // 玩家进入视野时调用
    public void ChasePlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.isRespawning) return;
        if (currentState == EnemyState.Chase) return; // 避免重复调用

        currentState = EnemyState.Chase;
        chaseTimer = 0;

        // 停止巡逻脚本与视野旋转
        patrol.enabled = false;
        visionRotate.enabled = false;

        agent.speed = chaseSpeed;

        // 在追击开始时，强制显示
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

            // 左右摆头
            float angle = Mathf.Sin(Time.time * 3f) * 45f; // 左右45°
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + angle * Time.deltaTime, 0);

            yield return null;
        }

        // 搜索结束 → 回到巡逻
        ResetPatrol();
    }


    // -------------------- 最重要：重置巡逻 --------------------
    public void ResetPatrol()
    {
        // 开启巡逻脚本与视觉旋转
        patrol.enabled = true;
        visionRotate.enabled = true;

        // 设置巡逻速度
        agent.speed = patrolSpeed;

        // 让巡逻脚本从第一个点重新开始（你的脚本用 currentIndex 自动管理）
        agent.Warp(patrol.patrolPoints[0].position);
        agent.SetDestination(patrol.patrolPoints[0].position);

        // 关闭追击计时
        chaseTimer = 0;

        // 切换状态
        ChangeState(EnemyState.Patrol);

        if (alertUI != null)
        {
            alertUI.Hide();
        }

    }

    public void BecomeSuspicious(Transform player)
    {
        if (currentState == EnemyState.Chase) return; // 追击时不进入警觉
        if (isSuspicious) return; // 已经在警觉计时

        isSuspicious = true;
        suspiciousTimer = 0f;
        suspiciousTarget = player;

        alertUI.ShowSuspicious(); // 显示 ?
    }

    //取消警觉
    public void CancelSuspicious()
    {
        if (isSuspicious)
        {
            isSuspicious = false;         // 停止计时
            suspiciousTimer = 0f;
            alertUI.Hide();               // 隐藏问号
            suspiciousTarget = null;      // 清空目标
        }
    }


    public void PlayerNearButNotSeen()
    {
        // 只有在巡逻状态下才显示 ?
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

        // 保存玩家最后被看到的位置
        lastKnownPlayerPos = player.position;

        // 进入追击
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
        // 1) 停掉所有搜索协程/行为
        StopAllCoroutines();

        // 2) 清空警觉
        isSuspicious = false;
        suspiciousTimer = 0f;
        suspiciousTarget = null;

        // 3) 清空追击
        chaseTimer = 0f;
        lastKnownPlayerPos = transform.position; // 随便给一个安全值

        // 4) 立刻隐藏 UI
        if (alertUI != null) alertUI.Hide();

        // 5) 强制回到巡逻
        ResetPatrol();
    }

}
