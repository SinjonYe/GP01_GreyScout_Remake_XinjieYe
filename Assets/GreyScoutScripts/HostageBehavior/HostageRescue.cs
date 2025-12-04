using UnityEngine;

public class HostageRescue : MonoBehaviour
{
    public HumanRescueTrigger trigger;
    public float rescueTime = 3f;  // 按住 E 多久完成营救

    private float rescueProgress = 0f;
    public bool rescued = false;

    private void Update()
    {
        if (rescued) return;

        if (trigger.playerInside)
        {
            // ----------- 玩家进入范围，显示 Press E -----------
            if (!Input.GetKey(KeyCode.E))
            {
                RescueUIManager.Instance.ShowPressE();
                RescueUIManager.Instance.Hide();   // 不显示进度条
            }

            // ----------- 玩家按住 E 进行解救 -----------
            if (Input.GetKey(KeyCode.E))
            {
                RescueUIManager.Instance.HidePressE(); // 按下E隐藏提示
                RescueUIManager.Instance.Show();       // 显示进度条

                rescueProgress += Time.deltaTime;
                RescueUIManager.Instance.SetProgress(rescueProgress / rescueTime);

                if (rescueProgress >= rescueTime)
                {
                    CompleteRescue();
                }
            }
            else
            {
                // 松手时进度条开始倒退
                rescueProgress = Mathf.Max(0, rescueProgress - Time.deltaTime * 1.2f);
                RescueUIManager.Instance.SetProgress(rescueProgress / rescueTime);
            }
        }
        else
        {
            // 不在范围内 → 全部隐藏
            RescueUIManager.Instance.Hide();
            RescueUIManager.Instance.HidePressE();
            rescueProgress = 0;
        }
    }

    void CompleteRescue()
    {
        rescued = true;
        RescueUIManager.Instance.Hide();
        RescueUIManager.Instance.HidePressE();

        // 告诉 GameManager：救到一个人质
        GameManager.Instance.HostageRescued();

        // 暂时直接让人质消失
        gameObject.SetActive(false);
    }
}
