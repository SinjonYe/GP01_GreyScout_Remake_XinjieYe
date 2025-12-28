using UnityEngine;

public class HostageRescue : MonoBehaviour
{
    // 关键：当前正在交互的人质（全局唯一）
    public static HostageRescue Current;

    public HumanRescueTrigger trigger; // 交互触发区域
    public float rescueTime = 3f;      // 按住 E 的时间

    private float rescueProgress = 0f; // 当前进度
    private bool rescued = false;      // 是否已被救

    private void Start()
    {
        // 关键：把自己绑定给 InteractionArea
        if (trigger != null)
            trigger.parentHostage = this;
    }

    private void Update()
    {
        if (rescued) return;

        // 关键：只有 Current 人质才允许控制 UI
        if (Current != this) return;

        // 玩家不在区域内 → 才隐藏 UI（并且只允许 Current 来隐藏）
        if (!trigger.playerInside)
        {
            RescueUIManager.Instance.HideAll();
            rescueProgress = 0;
            Current = null; // 离开范围，清空当前交互对象
            return;
        }

        // 玩家在范围内但是没有按 E → 显示 Press E
        if (!Input.GetKey(KeyCode.E))
        {
            RescueUIManager.Instance.ShowPressE();
            RescueUIManager.Instance.HideProgressBar();
            return;
        }

        // 玩家按住 E 时开始加载进度条
        RescueUIManager.Instance.HidePressE();
        RescueUIManager.Instance.ShowProgressBar();

        rescueProgress += Time.deltaTime;
        RescueUIManager.Instance.SetProgress(rescueProgress / rescueTime);

        // 完成救援
        if (rescueProgress >= rescueTime)
        {
            CompleteRescue();
        }
    }

    // 玩家进入范围（由 Trigger 调用）
    public void PlayerEntered()
    {
        if (rescued) return;

        // 关键：进入哪个人质范围，就把它设为 Current
        Current = this;

        // 进入范围立刻显示提示
        RescueUIManager.Instance.ShowPressE();
        RescueUIManager.Instance.HideProgressBar();
        RescueUIManager.Instance.SetProgress(0);
    }

    // 玩家离开范围（由 Trigger 调用）
    public void PlayerExited()
    {
        // 只有离开的对象是 Current，才允许关 UI（防止别的人质乱关）
        if (Current == this)
        {
            RescueUIManager.Instance.HideAll();
            rescueProgress = 0;
            Current = null;
        }
    }

    private void CompleteRescue()
    {
        rescued = true;

        // 隐藏所有 UI（保持原逻辑）
        RescueUIManager.Instance.HideAll();

        // 通知 GameManager：这个人质“被救下并加入队伍”
        GameManager.Instance.HostageRescuedAndFollow(this);

        // 隐藏人质
        //gameObject.SetActive(false);
    }

    // 被 GameManager 调用：关闭营救交互（防止重复救）
    public void DisableRescueInteraction()
    {
        // 关闭触发器脚本与碰撞器（避免再次触发 PressE）
        if (trigger != null)
        {
            trigger.playerInside = false;
            trigger.enabled = false;

            Collider col = trigger.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // 也可以把自己这个救援脚本关掉（避免 Update 再跑）
        this.enabled = false;
    }


}
