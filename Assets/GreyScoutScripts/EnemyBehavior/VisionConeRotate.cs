using UnityEngine;

public class VisionConeRotate : MonoBehaviour
{
    public float rotateSpeed = 40f;
    public float rotateAngle = 45f;

    private float startAngle;
    private float currentAngle;
    private bool rotatingRight = true;

    void Start()
    {
        startAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        if (rotatingRight)
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

        transform.localRotation = Quaternion.Euler(0, startAngle + currentAngle, 0);
    }
}
