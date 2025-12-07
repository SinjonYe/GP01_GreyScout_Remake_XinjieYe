using UnityEngine;

public class EnemyMiniMapIcon : MonoBehaviour
{
    public Transform icon;       // 小红点
    public Transform direction;  // 小三角形

    private void Update()
    {
        // icon 跟随敌人位置
        icon.position = transform.position + Vector3.up * 2;

        // direction 跟随敌人方向（只旋转Y）
        direction.rotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
    }
}
