using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Make the orientation of the icon always consistent with the orientation of the camera.
            transform.forward = Camera.main.transform.forward;
        }
    }
}
