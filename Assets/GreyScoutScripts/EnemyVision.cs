using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform enemy;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 检查视线是否被挡住
            Vector3 dir = other.transform.position - enemy.position;
            if (!Physics.Raycast(enemy.position, dir, out RaycastHit hit, 10f, obstacleMask))
            {
                Debug.Log(" Player detected!");
            }
        }
    }
}
