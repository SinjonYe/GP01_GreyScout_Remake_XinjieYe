using UnityEngine;

public class EnemyCatch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player CAUGHT by collision!");
            GameManager.Instance.PlayerCaught();
        }
    }
}
