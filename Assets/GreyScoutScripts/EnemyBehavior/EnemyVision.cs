using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    public EnemyController controller;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 dir = other.transform.position - enemy.position;

        // 射线检测：是否有墙挡住
        if (!Physics.Raycast(enemy.position, dir, out RaycastHit hit, 10f, obstacleMask))
        {
            // 没挡住 → 真正看到玩家
            // Debug.Log("Player detected");
            controller.PlayerSeen();
        }
        else
        {
            
            if (controller.currentState != EnemyState.Chase)
            {
                // 有墙挡住 → 玩家在附近但看不到 → 显示 ?
                controller.PlayerNearButNotSeen();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 只有 Patrol 才需要隐藏图标
        if (controller.currentState == EnemyState.Patrol)
        {
            controller.PlayerOutOfVision();
        }
    }

}
