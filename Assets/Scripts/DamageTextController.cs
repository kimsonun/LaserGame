using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageTextController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float lifetime = 1f;
    public float fadeSpeed = 2f;
    public float moveSpeed = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.5f);

    private TextMeshProUGUI tmpText;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 initialScale;
    private Vector3 direction;

    void Start()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();

        // Add or get CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Store initial scale for animation
        initialScale = transform.localScale;

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    public void Initialize(float damage)
    {
        string damageText = damage.ToString("F0");

        if (tmpText != null)
            tmpText.text = damageText;

        Canvas canvas = transform.GetChild(0).GetComponent<Canvas>();
        canvas.worldCamera = Camera.allCameras[1]; // 0 is main camera, 1 is UI camera
    }

    void Update()
    {
        timer += Time.deltaTime;
        float normalizedTime = timer / lifetime;

        // Move upward
        transform.Translate(moveSpeed * Time.deltaTime * Vector3.up);

        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - (normalizedTime * fadeSpeed);
        }

        // Scale animation
        float scaleMultiplier = scaleCurve.Evaluate(normalizedTime);
        transform.localScale = initialScale * scaleMultiplier;

        // Make sure it faces the camera
        if (Camera.main != null)
        {
            direction = Camera.main.transform.position - transform.position;
            direction.x = 0f;
            transform.rotation = Quaternion.LookRotation(direction);
            
            transform.Rotate(0, 180, 0); // Flip to face camera properly
        }
    }
}