using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public float height = 15f;
    public float distance = 10f;
    public float smoothSpeed = 2f;

    void Start()
    {
        // Position camera for top-down view
        transform.position = new Vector3(0, height, -distance);
        transform.rotation = Quaternion.Euler(60f, 0, 0);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + new Vector3(0, height, -distance);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
