using UnityEngine;

public class VisionConeRotate : MonoBehaviour
{
    public float rotateSpeed = 40f; 
    public float rotateAngle = 45f;

    private float startAngle; // Initial facing angle
    private float currentAngle;
    private bool rotatingRight = true; // Is rotating right

    void Start()
    {
        // Record initial local Y-axis rotation
        startAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        if (rotatingRight) // Swing vision back and forth between left and right limits
        {
            currentAngle += rotateSpeed * Time.deltaTime;
            if (currentAngle >= rotateAngle)
                rotatingRight = false;
        }
        else
        {
            currentAngle -= rotateSpeed * Time.deltaTime;
            if (currentAngle <= -rotateAngle)
                rotatingRight = true;
        }

        // Apply final rotation (offset based on initial angle)
        transform.localRotation = Quaternion.Euler(0, startAngle + currentAngle, 0);
    }
}
