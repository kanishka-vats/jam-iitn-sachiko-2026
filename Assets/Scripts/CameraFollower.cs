using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 0.1f;
    [SerializeField] private float orthographicSize = 6f;

    private void Start()
    {
        GetComponent<Camera>().orthographic = true;
        GetComponent<Camera>().orthographicSize = orthographicSize;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = playerTransform.position;
        targetPosition.z = -10f; // Keep camera in front
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }
}