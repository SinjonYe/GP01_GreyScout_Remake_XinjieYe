using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;

    public EnemyController controller;

    [Header("Vision Settings")]
    public float viewAngle = 60f;      // 左右各 60°，总视野角 120°
    public float viewDistance = 10f;   // 最大视野距离

    [Header("Ray Origin")]
    public float eyeHeight = 1.6f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        // 复活保护期：完全不进行视野判断，不进入警觉/追击
        if (GameManager.Instance != null && GameManager.Instance.isRespawning)
            return;

        Vector3 dir = other.transform.position - enemy.position;
        dir.y = 0f; // 只看水平
        float distance = dir.magnitude;

        // 超出视野距离 → “？”（警觉）
        if (distance > viewDistance)
        {
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // 角度判断
        Vector3 enemyForward = enemy.forward;
        enemyForward.y = 0f;

        float angle = Vector3.Angle(enemyForward, dir);
        if (angle > viewAngle)
        {
            // 玩家在范围里但不在视野方向 → “？”
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // 射线：从眼睛高度发射，检测障碍物
        Vector3 origin = enemy.position + Vector3.up * eyeHeight;
        Vector3 rayDir = dir.normalized;

        bool blocked = Physics.Raycast(origin, rayDir, distance, obstacleMask, QueryTriggerInteraction.Ignore);

        if (!blocked)
        {
            controller.PlayerSeen(); // PlayerSeen 内部也有 isRespawning 判断，双保险
        }
        else
        {
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
