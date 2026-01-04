using UnityEngine;

public class HumanRescueTrigger : MonoBehaviour
{
    [HideInInspector] public HostageRescue parentHostage;
    // ↑ Auto assigned in HostageRescue.Start()/ 在 HostageRescue.Start() 里会自动赋值

    public bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Prevent error when parentHostage unbound
        if (parentHostage != null)
            // Notify target hostage: Player enter
            parentHostage.PlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (parentHostage != null)
            // Notify target hostage: Player leave
            parentHostage.PlayerExited();
    }
}
