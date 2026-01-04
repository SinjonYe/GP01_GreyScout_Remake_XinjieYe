using UnityEngine;

public class HostageRescue : MonoBehaviour
{
    // Key: Current interacting hostage (Global Unique)
    public static HostageRescue Current;

    public HumanRescueTrigger trigger; // Interaction trigger zone
    public float rescueTime = 3f;      // Hold E duration

    private float rescueProgress = 0f; // Current progress
    private bool rescued = false;      // Rescued status

    public bool instantRescue = false; // Enable for rescued hostages after respawn/ 重生后对“已救过的人质”开启

    private void Start()
    {
        // Key: Bind self to InteractionArea/ 关键：把自己绑定给 InteractionArea
        if (trigger != null)
            trigger.parentHostage = this;
    }

    private void Update()
    {
        if (rescued) return;

        // Key: Only current hostage can control UI/ 关键：只有 Current 人质才允许控制 UI
        if (Current != this) return;

        // Hide UI only when player out of range (Current only)
        if (!trigger.playerInside)
        {
            RescueUIManager.Instance.HideAll();
            rescueProgress = 0;
            Current = null; // Clear current interact target when leave range
            return;
        }

        // --- Instant Rescue : Press E once to rescue ---
        if (instantRescue)
        {
            // Show Press E when player in range
            if (!Input.GetKeyDown(KeyCode.E))
            {
                RescueUIManager.Instance.ShowPressE();
                RescueUIManager.Instance.HideProgressBar();
                return;
            }

            // Complete rescue on E pressed
            CompleteRescue();
            return;
        }

        // Show Press E when player in range but E not pressed
        if (!Input.GetKey(KeyCode.E))
        {
            RescueUIManager.Instance.ShowPressE();
            RescueUIManager.Instance.HideProgressBar();
            return;
        }

        // Fill progress bar when holding E
        RescueUIManager.Instance.HidePressE();
        RescueUIManager.Instance.ShowProgressBar();

        rescueProgress += Time.deltaTime;
        RescueUIManager.Instance.SetProgress(rescueProgress / rescueTime);

        // Rescue complete
        if (rescueProgress >= rescueTime)
        {
            CompleteRescue();
        }
    }

    // Player enter range (Called by Trigger)/ 玩家进入范围（由 Trigger 调用）
    public void PlayerEntered()
    {
        if (rescued) return;

        // Key: Set self as Current when player enter
        Current = this;

        // Show tip immediately
        RescueUIManager.Instance.ShowPressE();
        RescueUIManager.Instance.HideProgressBar();
        RescueUIManager.Instance.SetProgress(0);
    }

    // Player leave range (Called by Trigger)/ 玩家离开范围（由 Trigger 调用）
    public void PlayerExited()
    {
        // Only close UI if self is Current (Avoid wrong close)
        if (Current == this)
        {
            RescueUIManager.Instance.HideAll();
            rescueProgress = 0;
            Current = null;
        }
    }

    private void CompleteRescue()
    {
        instantRescue = false; // Reset normal state (Instant rescue by DropFollowersForRespawn)/ 回到正常状态（下一次是否瞬救由 DropFollowersForRespawn 决定）

        rescued = true;

        // Hide all UI
        RescueUIManager.Instance.HideAll();

        // Notify GameManager: Hostage rescued and join team
        GameManager.Instance.HostageRescuedAndFollow(this);

        // Hide hostage
        //gameObject.SetActive(false);
    }

    // Called by GameManager: Disable rescue interact (Prevent repeat rescue)/ 被 GameManager 调用：关闭营救交互（防止重复救）
    public void DisableRescueInteraction()
    {
        // Disable trigger script & collider
        if (trigger != null)
        {
            trigger.playerInside = false;
            trigger.enabled = false;

            Collider col = trigger.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // Disable this rescue script
        this.enabled = false;
    }

    public void ResetForReRescueInstant()
    {
        rescued = false;
        rescueProgress = 0f;
        instantRescue = true; // Enable instant rescue

        // Re-enable self script
        this.enabled = true;

        // Restore trigger & collider
        if (trigger != null)
        {
            trigger.enabled = true;
            trigger.playerInside = false;

            Collider col = trigger.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            // key: Ensure trigger bind self
            trigger.parentHostage = this;
        }

        // Clear Current (Avoid UI/Interact residue)
        if (Current == this) Current = null;
    }


}
