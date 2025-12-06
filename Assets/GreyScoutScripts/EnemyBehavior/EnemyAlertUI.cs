using UnityEngine;

public class EnemyAlertUI : MonoBehaviour
{
    public SpriteRenderer iconRenderer;
    public Sprite suspiciousSprite;   // ÎÊºÅ ?
    public Sprite alertSprite;        // Ì¾ºÅ !

    public Color alertColor = Color.white;

    private void Start()
    {
        Hide(); // Ä¬ÈÏÒþ²Ø
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
