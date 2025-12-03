using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    public EnemyController controller;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 视线射线检测：是否被墙挡住
            Vector3 dir = other.transform.position - enemy.position;

            if (!Physics.Raycast(enemy.position, dir, out RaycastHit hit, 10f, obstacleMask))
            {
                Debug.Log("Player detected");
                controller.ChasePlayer(); // 触发追击

                /*
                // 玩家被抓
                float distance = Vector3.Distance(other.transform.position, enemy.position);
                if (distance < 1.2f)
                {
                    GameManager.Instance.PlayerCaught();
                }
                */
            }

        }
    }

}
