using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;

    public EnemyController controller;

    [Header("Vision Settings")]
    public float viewAngle = 60f;      //  60° left and right, total FOV 120°/ 左右各 60°，总视野角 120°
    public float viewDistance = 10f;   // Max vision distance/ 最大视野距离

    [Header("Ray Origin")]
    public float eyeHeight = 1.6f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Respawn invulnerability: disable all vision check, no alert/chase/ 复活保护期：完全不进行视野判断，不进入警觉/追击
        if (GameManager.Instance != null && GameManager.Instance.isRespawning)
            return;

        Vector3 dir = other.transform.position - enemy.position;
        dir.y = 0f; // Horizontal vision only
        float distance = dir.magnitude;

        //  Beyond vision distance → Alert state (?)
        if (distance > viewDistance)
        {
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // Angle check
        Vector3 enemyForward = enemy.forward;
        enemyForward.y = 0f;

        float angle = Vector3.Angle(enemyForward, dir);
        if (angle > viewAngle)
        {
            // Player in range but not in vision direction → Alert state (?)
            if (controller.currentState != EnemyState.Chase)
                controller.BecomeSuspicious(other.transform);
            return;
        }

        // Raycast: shoot from eye height, detect obstacles
        Vector3 origin = enemy.position + Vector3.up * eyeHeight;
        Vector3 rayDir = dir.normalized;

        bool blocked = Physics.Raycast(origin, rayDir, distance, obstacleMask, QueryTriggerInteraction.Ignore);

        if (!blocked)
        {
            controller.PlayerSeen(); // Double insurance: PlayerSeen contains isRespawning check
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

        // Hide icon only when return to Patrol state
        if (controller.currentState == EnemyState.Patrol)
        {
            controller.PlayerOutOfVision();
            controller.CancelSuspicious();   // Player out of range → Cancel alert state
        }
    }
}
