using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Weapon")]
    public GameObject laserPrefab;
    public Transform weaponTip;
    public float laserDamage = 50f;
    public float damageInterval = 0.5f;
    public int bounceMax = 3;
    public int penetrationMax = 2;
    public float maxLaserRange = 50f;

    [Header("Effects")]
    public GameObject laserBeamImpactPrefab;
    public GameObject damageTextPrefab;

    private Camera mainCamera;
    private bool isShooting = false;
    private GameObject currentLaser;

    void Start()
    {
        mainCamera = Camera.main;
        tag = "Player";
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleShooting();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Move in world space directions regardless of rotation
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    void HandleRotation()
    {
        // Get mouse position in world space on the ground plane
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 lookDirection = mouseWorldPos - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0)) // Continuous while held
        {
            if (!isShooting && !GameManager.Instance.IsGamePaused())
            {
                StartShooting();
            }
            else
            {
                // Update laser continuously
                UpdateLaser();
            }
        }
        else if (isShooting)
        {
            StopShooting();
        }
    }

    void StartShooting()
    {
        isShooting = true;
        CreateLaser();
    }

    void UpdateLaser()
    {
        if (currentLaser != null)
        {
            Laser laserScript = currentLaser.GetComponent<Laser>();
            if (laserScript != null)
            {
                laserScript.UpdateLaserBeam(weaponTip.position, transform.forward);
            }
        }
    }

    void StopShooting()
    {
        isShooting = false;
        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }
    }

    void CreateLaser()
    {
        if (currentLaser != null)
            Destroy(currentLaser);

        currentLaser = Instantiate(laserPrefab, weaponTip.position, transform.rotation);
        Laser laserScript = currentLaser.GetComponent<Laser>();
        laserScript.Initialize(laserDamage, damageInterval, bounceMax, penetrationMax, maxLaserRange, this, laserBeamImpactPrefab, damageTextPrefab);
    }
}