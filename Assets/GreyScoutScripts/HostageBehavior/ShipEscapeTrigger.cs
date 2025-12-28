using UnityEngine;
using TMPro;

public class ShipEscapeTrigger : MonoBehaviour
{
    [Header("引用")]
    public Transform shipStandPoint;        // 可选：把人质传送到船里哪个点
    public TextMeshProUGUI shipHintText;    // “Press E to Start” 的提示（可选）

    private bool playerInside = false;

    private void Start()
    {
        if (shipHintText != null)
        {
            var c = shipHintText.color;
            c.a = 0;
            shipHintText.color = c;
        }
    }

    private void Update()
    {
        if (!playerInside) return;

        // 如果已经送达够 6 个 → 在船上提示按 E 启动离开
        if (GameManager.Instance.deliveredHostages >= GameManager.Instance.requiredDeliveredHostages)
        {
            ShowHint(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                // 触发胜利（你已有 VictorySequence，可复用）
                GameManager.Instance.StartCoroutine("VictorySequence");
            }
        }
        else
        {
            ShowHint(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        // 玩家回到船上：把跟随的人质全部送达（计数 + 隐藏）
        GameManager.Instance.DeliverAllFollowersToShip(shipStandPoint);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        ShowHint(false);
    }

    private void ShowHint(bool show)
    {
        if (shipHintText == null) return;

        var c = shipHintText.color;
        c.a = show ? 1 : 0;
        shipHintText.color = c;
    }
}
