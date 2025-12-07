using UnityEngine;

public class HostageRescue : MonoBehaviour
{
    public HumanRescueTrigger trigger; // 交互触发区域
    public float rescueTime = 3f;      // 按住 E 的时间

    private float rescueProgress = 0f; // 当前进度
    private bool rescued = false;      // 是否已被救

    private void Start()
    {
        // 关键：把自己绑定给 InteractionArea
        trigger.parentHostage = this;
    }

    private void Update()
    {
        if (rescued) return;

        // 玩家不在区域内 → 全部 UI 隐藏
        if (!trigger.playerInside)
        {
            RescueUIManager.Instance.HideAll();
            rescueProgress = 0;
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

    // 玩家进入范围
    public void PlayerEntered()
    {
        if (!rescued)
            RescueUIManager.Instance.ShowPressE();
    }

    // 玩家离开范围
    public void PlayerExited()
    {
        RescueUIManager.Instance.HideAll();
        rescueProgress = 0;
    }

    private void CompleteRescue()
    {
        rescued = true;

        // 隐藏所有 UI
        RescueUIManager.Instance.HideAll();

        // 通知 GameManager
        GameManager.Instance.HostageRescued();

        // 隐藏人质
        gameObject.SetActive(false);
    }
}
