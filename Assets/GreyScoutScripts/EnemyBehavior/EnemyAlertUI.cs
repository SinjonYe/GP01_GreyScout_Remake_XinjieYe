using UnityEngine;

public class EnemyAlertUI : MonoBehaviour
{
    public SpriteRenderer iconRenderer;
    public Sprite suspiciousSprite;   // QuestionMark ?
    public Sprite alertSprite;        // ExclamationMark !

    public Color alertColor = Color.white;

    private void Start()
    {
        Hide(); // Default hidden state
    }

    public void ShowSuspicious()
    {
        if (iconRenderer == null || suspiciousSprite == null) return;

        iconRenderer.sprite = suspiciousSprite;
        iconRenderer.color = alertColor;
        iconRenderer.enabled = true;
    }

    public void ShowAlert()
    {
        if (iconRenderer == null || alertSprite == null) return;

        iconRenderer.sprite = alertSprite;
        iconRenderer.color = alertColor;
        iconRenderer.enabled = true;
    }

    public void Hide()
    {
        if (iconRenderer != null)
        {
            iconRenderer.enabled = false;
        }
    }
}
