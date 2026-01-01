using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (GameManager.Instance.isCarryMode)
                GameManager.Instance.StopCarry();
            else
                GameManager.Instance.StartCarry();
        }
    }
}
