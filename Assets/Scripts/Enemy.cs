using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 2f;
    public float changeDirectionTime = 2f;

    [Header("UI")]
    public Canvas healthCanvas;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private float currentHP;
    private Vector3 moveDirection;
    private float lastDirectionChange;

    void Start()
    {
        tag = "Enemy";
        currentHP = maxHP;
        ChangeDirection();
        UpdateHealthUI();

        // Setup health bar
        if (healthCanvas != null)
        {
            healthCanvas.worldCamera = Camera.main;
        }
    }

    void Update()
    {
        Move();

        if (Time.time - lastDirectionChange > changeDirectionTime)
        {
            ChangeDirection();
        }

        // Make health bar face camera
        if (healthCanvas != null)
        {
            healthCanvas.transform.LookAt(Camera.main.transform);
            healthCanvas.transform.Rotate(0, 180, 0);
        }
    }

    void Move()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Bounce off boundaries
        if (Mathf.Abs(transform.position.x) > 18f || Mathf.Abs(transform.position.z) > 18f)
        {
            ChangeDirection();
        }
    }

    void ChangeDirection()
    {
        moveDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        lastDirectionChange = Time.time;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        UpdateHealthUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHP / maxHP;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHP:F0}/{maxHP:F0}";
        }
    }

    void Die()
    {
        GameManager.Instance.EnemyDestroyed(this);
        Destroy(gameObject);
    }
}