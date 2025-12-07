using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;

    public EnemyController controller;

    [Header("Vision Settings")]
    public float viewAngle = 60f;      // 左右各 60°，总视野角 120°
    public float viewDistance = 10f;   // 最大视野距离

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 dir = other.transform.position - enemy.position;
        float distance = dir.magnitude;

        // 超出视野距离 → “？”（警觉）
        if (distance > viewDistance)
        {
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // 判断玩家是否在前方视野角内
        Vector3 enemyForward = enemy.forward;
        dir.y = 0;         // 忽略上下角度，只考虑水平角度
        enemyForward.y = 0;

        float angle = Vector3.Angle(enemyForward, dir);

        if (angle > viewAngle)
        {
            // 玩家在范围里但不在视野方向 → “？”
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // 射线检测：视线是否被遮挡
        if (!Physics.Raycast(enemy.position, dir.normalized, out RaycastHit hit, viewDistance, obstacleMask))
        {
            // 视线无遮挡 → 真正看到玩家 → !+ 追击
            controller.PlayerSeen();
        }
        else
        {
            // 视线被墙挡住 → “？”
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 只有回到 Patrol 状态才需要隐藏图标
        if (controller.currentState == EnemyState.Patrol)
        {
            controller.PlayerOutOfVision();
            controller.CancelSuspicious();   // 玩家离开范围 → 取消警觉
        }
    }
}
