using UnityEngine;
using TMPro;

public class ShipEscapeTrigger : MonoBehaviour
{
    [Header("送达设置")]
    public Transform shipStandPoint;          // 人质上船点（可选）
    public bool deliverAllFollowers = true;   // 是否一次送达当前队伍全部人质

    [Header("提示UI（可选）")]
    public TextMeshProUGUI shipHintText;      // “Press E to Start Engine”

    private bool playerInside = false;

    // 修改点：防止重复触发胜利（作用：按E只触发一次）
    private bool winTriggered = false;

    private void Start()
    {
        SetHintAlpha(0);
    }

    private void Update()
    {
        if (!playerInside) return;

        // 达成条件：送达满 6 个
        bool readyToWin = GameManager.Instance.deliveredHostages >= GameManager.Instance.requiredDeliveredHostages;

        if (readyToWin)
        {
            SetHintAlpha(1);

            if (Input.GetKeyDown(KeyCode.E) && !winTriggered)
            {
                winTriggered = true; // 标记已触发胜利
                GameManager.Instance.TriggerWin(); // 统一从 GameManager 触发胜利
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

        // 玩家回到船附近 → 送达人质
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
