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

    private LineRenderer lineRenderer;
    private List<Vector3> laserPoints;
    private Dictionary<Enemy, float> lastDamageTime;
    private List<Enemy> enemiesInBeam;
    private Transform beamStart;

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
    }

    public void Initialize(float damage, float damageInterval, int bounceMax, int penetrationMax, float maxRange, Player owner)
    {
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.bounceMax = bounceMax;
        this.penetrationMax = penetrationMax;
        this.maxRange = maxRange;
        this.owner = owner;
    }

    void Update()
    {
        DamageEnemiesInBeam();
    }

    public void UpdateLaserBeam(Vector3 startPosition, Vector3 direction)
    {
        CalculateLaserPath(startPosition, direction);
        UpdateLineRenderer();
        UpdateEnemiesInBeam();
    }

    void CalculateLaserPath(Vector3 startPos, Vector3 direction)
    {
        laserPoints.Clear();
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
                    // Hit something else, stop laser
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

                // Show damage text
                DamageText.ShowDamage(enemy.transform.position + Vector3.up * 2f, damage);
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

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
        beamStart.position = laserPoints[0];
    }
}
