using UnityEngine;
using System.Collections.Generic;

public class Laser : MonoBehaviour
{
    private float damage;
    private float damageInterval;
    private int bounceMax;
    private int penetrationMax;
    private float maxRange;
    private Player owner;
    private GameObject impactPrefab;
    private GameObject damageTextPrefab;

    private LineRenderer lineRenderer;
    private List<Vector3> laserPoints;
    private Dictionary<Enemy, float> lastDamageTime;
    private List<Enemy> enemiesInBeam;
    private Transform beamStart;

    // Impact effect management
    private List<GameObject> activeImpacts;
    private List<Vector3> impactPositions;

    void Start()
    {
        tag = "Laser";
        beamStart = gameObject.transform.GetChild(0);
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = true;

        laserPoints = new List<Vector3>();
        lastDamageTime = new Dictionary<Enemy, float>();
        enemiesInBeam = new List<Enemy>();
        activeImpacts = new List<GameObject>();
        impactPositions = new List<Vector3>();
    }

    public void Initialize(float damage, float damageInterval, int bounceMax, int penetrationMax, float maxRange, Player owner, GameObject impactPrefab, GameObject damageTextPrefab)
    {
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.bounceMax = bounceMax;
        this.penetrationMax = penetrationMax;
        this.maxRange = maxRange;
        this.owner = owner;
        this.impactPrefab = impactPrefab;
        this.damageTextPrefab = damageTextPrefab;
    }

    void Update()
    {
        DamageEnemiesInBeam();
    }

    void OnDestroy()
    {
        // Clean up impact effects when laser is destroyed
        foreach (GameObject impact in activeImpacts)
        {
            if (impact != null)
                Destroy(impact);
        }
        activeImpacts.Clear();
    }

    public void UpdateLaserBeam(Vector3 startPosition, Vector3 direction)
    {
        CalculateLaserPath(startPosition, direction);
        UpdateLineRenderer();
        UpdateEnemiesInBeam();
        UpdateImpactEffects();
    }

    void CalculateLaserPath(Vector3 startPos, Vector3 direction)
    {
        laserPoints.Clear();
        impactPositions.Clear();
        laserPoints.Add(startPos);

        Vector3 currentPos = startPos;
        Vector3 currentDirection = direction;
        int bounces = 0;
        int penetrations = 0;
        float remainingRange = maxRange;

        while (remainingRange > 0)
        {
            RaycastHit hit;
            float rayDistance = Mathf.Min(remainingRange, 100f);

            if (Physics.Raycast(currentPos, currentDirection, out hit, rayDistance))
            {
                laserPoints.Add(hit.point);
                remainingRange -= hit.distance;

                if (hit.collider.CompareTag("Enemy"))
                {
                    // Add impact effect at enemy hit point
                    impactPositions.Add(hit.point);

                    if (penetrations < penetrationMax)
                    {
                        penetrations++;
                        // Continue through enemy
                        currentPos = hit.point + currentDirection * 0.01f; // Small offset to avoid re-hitting
                        continue;
                    }
                    else
                    {
                        // Stop at enemy, can't penetrate anymore
                        break;
                    }
                }
                else if (hit.collider.CompareTag("Obstacle"))
                {
                    // Add impact effect at obstacle hit point
                    impactPositions.Add(hit.point);

                    if (bounces < bounceMax)
                    {
                        bounces++;
                        currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                        currentPos = hit.point + hit.normal * 0.01f; // Small offset from surface
                        continue;
                    }
                    else
                    {
                        // Stop at obstacle, can't bounce anymore
                        break;
                    }
                }
                else
                {
                    // Hit something else, add impact and stop laser
                    impactPositions.Add(hit.point);
                    break;
                }
            }
            else
            {
                // No hit, extend to max range
                Vector3 endPoint = currentPos + currentDirection * remainingRange;
                laserPoints.Add(endPoint);
                break;
            }
        }
    }

    void UpdateImpactEffects()
    {
        // Clean up existing impacts
        foreach (GameObject impact in activeImpacts)
        {
            if (impact != null)
                Destroy(impact);
        }
        activeImpacts.Clear();

        // Create new impacts at current positions
        if (impactPrefab != null)
        {
            foreach (Vector3 position in impactPositions)
            {
                GameObject impact = Instantiate(impactPrefab, position, Quaternion.identity);
                activeImpacts.Add(impact);
            }
        }
    }

    void UpdateEnemiesInBeam()
    {
        enemiesInBeam.Clear();

        // Check each segment of the laser path
        for (int i = 0; i < laserPoints.Count - 1; i++)
        {
            Vector3 segmentStart = laserPoints[i];
            Vector3 segmentEnd = laserPoints[i + 1];
            Vector3 segmentDirection = (segmentEnd - segmentStart).normalized;
            float segmentLength = Vector3.Distance(segmentStart, segmentEnd);

            // Raycast along this segment to find enemies
            RaycastHit[] hits = Physics.RaycastAll(segmentStart, segmentDirection, segmentLength);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null && !enemiesInBeam.Contains(enemy))
                    {
                        enemiesInBeam.Add(enemy);
                    }
                }
            }
        }
    }

    void DamageEnemiesInBeam()
    {
        float currentTime = Time.time;

        foreach (Enemy enemy in enemiesInBeam)
        {
            if (enemy == null) continue;

            if (!lastDamageTime.ContainsKey(enemy) || currentTime - lastDamageTime[enemy] >= damageInterval)
            {
                enemy.TakeDamage(damage);
                lastDamageTime[enemy] = currentTime;

                // Show damage text using prefab
                ShowDamageText(enemy.transform.position + Vector3.up * 2f, damage);
            }
        }

        // Clean up destroyed enemies from damage tracking
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (Enemy enemy in lastDamageTime.Keys)
        {
            if (enemy == null)
                enemiesToRemove.Add(enemy);
        }
        foreach (Enemy enemy in enemiesToRemove)
        {
            lastDamageTime.Remove(enemy);
        }
    }

    void ShowDamageText(Vector3 worldPosition, float damageAmount)
    {
        if (damageTextPrefab != null)
        {
            GameObject damageTextObj = Instantiate(damageTextPrefab, worldPosition, Quaternion.identity);

            // If the prefab has a DamageTextController component, initialize it
            DamageTextController controller = damageTextObj.GetComponent<DamageTextController>();
            if (controller != null)
            {
                controller.Initialize(damageAmount);
            }
        }
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
        beamStart.position = laserPoints[0];
    }
}