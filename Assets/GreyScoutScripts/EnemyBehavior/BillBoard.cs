using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // 让图标的朝向始终和相机朝向一致
            transform.forward = Camera.main.transform.forward;
        }
    }
}
