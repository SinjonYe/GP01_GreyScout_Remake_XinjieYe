using UnityEngine;

public class EnemyMiniMapIcon : MonoBehaviour
{
    public Transform icon;       // Red dot
    public Transform direction;  // Small triangle

    private void Update()
    {
        // icon Icon follows enemy position
        icon.position = transform.position + Vector3.up * 2;

        // Direction follows enemy facing (Y-axis rotation only)
        direction.rotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
    }
}
