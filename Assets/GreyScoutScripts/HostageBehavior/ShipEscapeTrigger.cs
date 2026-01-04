using UnityEngine;
using TMPro;

public class ShipEscapeTrigger : MonoBehaviour
{
    [Header("Delivery settings")]
    public Transform shipStandPoint;          // Hostage board point
    public bool deliverAllFollowers = true;   // Deliver all hostages in team once

    [Header("Prompt UI")]
    public TextMeshProUGUI shipHintText;      // ¡°Press E to Start Engine¡±

    private bool playerInside = false;

    // Prevent repeat win trigger (E only trigger once)
    private bool winTriggered = false;

    private void Start()
    {
        SetHintAlpha(0);
    }

    private void Update()
    {
        if (!playerInside) return;

        // Win condition: Delivered count >=6
        bool readyToWin = GameManager.Instance.deliveredHostages >= GameManager.Instance.requiredDeliveredHostages;

        if (readyToWin)
        {
            SetHintAlpha(1);

            if (Input.GetKeyDown(KeyCode.E) && !winTriggered)
            {
                winTriggered = true; // Mark win triggered
                GameManager.Instance.TriggerWin(); // Trigger win via GameManager uniformly
            }
        }
        else
        {
            SetHintAlpha(0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Player near boat ¡ú Deliver hostages
        if (deliverAllFollowers)
        {
            GameManager.Instance.DeliverAllFollowersToShip(shipStandPoint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        SetHintAlpha(0);
    }

    private void SetHintAlpha(float a)
    {
        if (shipHintText == null) return;

        var c = shipHintText.color;
        c.a = a;
        shipHintText.color = c;
    }
}
