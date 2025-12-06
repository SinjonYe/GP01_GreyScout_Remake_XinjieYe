using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Lost
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
                ChangeState(EnemyState.Lost);
            }
        }
        // ---------------- 失去玩家后回到巡逻 ----------------
        else if (currentState == EnemyState.Lost)
        {
            ResetPatrol();
            alertUI.Hide();   // 回到巡逻隐藏图标
        }
    }

    // 玩家进入视野时调用
    public void ChasePlayer()
    {
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
        if (alertUI != null)
        {
            alertUI.ShowAlert();
        }

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

}
