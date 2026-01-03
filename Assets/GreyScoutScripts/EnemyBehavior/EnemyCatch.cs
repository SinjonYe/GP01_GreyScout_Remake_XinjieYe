using UnityEngine;
using System.Collections;

public class EnemyCatch : MonoBehaviour
{
    private bool coolingDown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.isRespawning) return;

        if (coolingDown) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player CAUGHT by collision!");
            coolingDown = true;
            GameManager.Instance.PlayerCaught();
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1.0f);
        coolingDown = false;
    }
}
