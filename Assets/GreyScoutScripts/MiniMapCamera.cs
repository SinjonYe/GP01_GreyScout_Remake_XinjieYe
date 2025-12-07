using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        Vector3 pos = target.position;
        pos.y += 20f;      // ¿Îµÿ∏ﬂ∂»
        transform.position = pos;

        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
