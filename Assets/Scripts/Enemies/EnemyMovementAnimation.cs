using UnityEngine;

public class EnemyMovementAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visual;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.001f;

    [Header("Bob")]
    [SerializeField] private float bobHeight = 0.05f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("Squash & Stretch")]
    [SerializeField] private float squashAmount = 0.04f;
    [SerializeField] private float stretchAmount = 0.04f;

    [Header("Rotation")]
    [SerializeField] private bool useRotation = true;
    [SerializeField] private float rotationAmount = 3f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;

    private Vector3 lastPosition;
    private float animationTime;

    private void Awake()
    {
        if (visual == null)
        {
            Debug.LogError($"{name}: EnemyMovementAnimation has no Visual assigned!");
            enabled = false;
            return;
        }

        startPosition = visual.localPosition;
        startScale = visual.localScale;
        startRotation = visual.localRotation;

        lastPosition = transform.position;
    }

    private void Update()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        bool isMoving = distanceMoved > movementThreshold * Time.deltaTime;

        if (isMoving)
        {
            Animate();
        }
        else
        {
            ReturnToIdle();
        }

        lastPosition = transform.position;
    }

    private void Animate()
    {
        animationTime += Time.deltaTime * bobSpeed;

        float bob = Mathf.Sin(animationTime) * bobHeight;

        visual.localPosition = new Vector3(
            startPosition.x,
            startPosition.y + bob,
            startPosition.z
        );

        float squashWave = Mathf.Sin(animationTime * 2f);

        float scaleX = 1f + squashWave * squashAmount;
        float scaleY = 1f - squashWave * stretchAmount;

        visual.localScale = new Vector3(
            startScale.x * scaleX,
            startScale.y * scaleY,
            startScale.z
        );

        if (useRotation)
        {
            float rotation = Mathf.Sin(animationTime) * rotationAmount;

            visual.localRotation =
                startRotation * Quaternion.Euler(0f, 0f, rotation);
        }
    }

    private void ReturnToIdle()
    {
        visual.localPosition = Vector3.Lerp(
            visual.localPosition,
            startPosition,
            Time.deltaTime * 12f
        );

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            startScale,
            Time.deltaTime * 12f
        );

        visual.localRotation = Quaternion.Lerp(
            visual.localRotation,
            startRotation,
            Time.deltaTime * 12f
        );
    }
}