using UnityEngine;

public class HumanRescueTrigger : MonoBehaviour
{
    [HideInInspector] public HostageRescue parentHostage;
    // ↑ 在 HostageRescue.Start() 里会自动赋值

    public bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // 防止 parentHostage 没绑定导致报错
        if (parentHostage != null)
            // 通知对应的人质：玩家进入
            parentHostage.PlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (parentHostage != null)
            // 通知对应的人质：玩家离开
            parentHostage.PlayerExited();
    }
}
