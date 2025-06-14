using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    public static void ShowDamage(Vector3 worldPosition, float damage)
    {
        GameObject damageTextObj = new GameObject("DamageText");
        damageTextObj.transform.position = worldPosition;

        Canvas canvas = damageTextObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        Text text = damageTextObj.AddComponent<Text>();
        text.text = damage.ToString("F0");
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.red;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform rectTransform = damageTextObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 50);

        // Add animation
        DamageTextAnimator animator = damageTextObj.AddComponent<DamageTextAnimator>();
        animator.Initialize();
    }
}

public class DamageTextAnimator : MonoBehaviour
{
    private float lifetime = 1f;
    private float fadeSpeed = 2f;
    private float moveSpeed = 2f;
    private Text text;

    public void Initialize()
    {
        text = GetComponent<Text>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move up
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // Fade out
        Color color = text.color;
        color.a -= fadeSpeed * Time.deltaTime;
        text.color = color;
    }
}
