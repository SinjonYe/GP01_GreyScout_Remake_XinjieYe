using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class EnemyCatch : MonoBehaviour
{
    private bool coolingDown = false;

    private void OnTriggerEnter(Collider other)
    {
        // Return if player is respawning, skip capture trigger
        if (GameManager.Instance != null && GameManager.Instance.isRespawning) return;
        // Return if in cooldown, avoid repeated capture
        if (coolingDown) return;

        if (other.CompareTag("Player")) // Check if the trigger object is player
        {
            Debug.Log("Player CAUGHT by collision!");
            coolingDown = true;
            GameManager.Instance.PlayerCaught();
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown() //Capture cooldown coroutine, prevent continuous trigger
    {
        yield return new WaitForSeconds(1.0f);
        coolingDown = false;
    }
}
