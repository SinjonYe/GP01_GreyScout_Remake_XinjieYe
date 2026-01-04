using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    private void Update()
    {
        // Press F to toggle Carry/Uncarry mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (GameManager.Instance.isCarryMode)
                GameManager.Instance.StopCarry(); // Exit carry state
            else
                GameManager.Instance.StartCarry(); // Enter carry state
        }
    }
}
