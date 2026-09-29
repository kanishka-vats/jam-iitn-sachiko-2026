using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] protected float aimDistance = 30f;
    [SerializeField] protected Transform firePoint;


    [Header("Weapon Visual")]
    [SerializeField] protected Transform weaponVisual;


    protected Vector2 aimDirection = Vector2.right;

    [Header("Aiming Visual")]
    [SerializeField] protected bool showAimLine = true;
    [SerializeField] protected Color aimLineColor = new Color(1f, 0.9f, 0.7f, 0.6f);
    [SerializeField] protected float aimLineWidth = 0.08f;

    protected LineRenderer aimLine;
    protected Camera mainCamera;

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        if (firePoint == null)
        {
            firePoint = transform;
        }

        SetupAimLine();
    }

    protected virtual void Update()
    {
        UpdateAim();
        UpdateAimLineVisual();
    }

    protected virtual void UpdateAim()
    {
        if (mainCamera == null || firePoint == null || Mouse.current == null)
            return;

        // Get mouse position on screen
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        // Convert mouse position into world position
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        // Direction from weapon to mouse
        aimDirection =
            (mouseWorldPosition - firePoint.position).normalized;

        RotateWeapon(aimDirection);
    }

    protected virtual void RotateWeapon(Vector2 direction)
    {
        if (weaponVisual == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        weaponVisual.rotation =
            Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected virtual void SetupAimLine()
    {
        aimLine = GetComponent<LineRenderer>();

        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
        }

        aimLine.positionCount = 2;
        aimLine.startWidth = aimLineWidth;
        aimLine.endWidth = aimLineWidth * 0.5f;

        aimLine.material =
            new Material(Shader.Find("Sprites/Default"));

        aimLine.startColor = aimLineColor;

        aimLine.endColor =
            new Color(
                aimLineColor.r,
                aimLineColor.g,
                aimLineColor.b,
                0.2f
            );

        aimLine.sortingOrder = 5;
    }

    protected virtual void UpdateAimLineVisual()
    {
        if (!showAimLine || aimLine == null || firePoint == null)
            return;

        Vector3 startPos = firePoint.position;

        Vector3 endPos =
            startPos +
            (Vector3)aimDirection * aimDistance;

        aimLine.SetPosition(0, startPos);
        aimLine.SetPosition(1, endPos);
    }

    public virtual void SetAimLineColor(Color color)
    {
        aimLineColor = color;

        if (aimLine != null)
        {
            aimLine.startColor = color;

            aimLine.endColor =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.2f
                );
        }
    }

    public virtual void SetAimDistance(float distance)
    {
        aimDistance = distance;
    }

    public virtual void ShowAimLine(bool show)
    {
        showAimLine = show;

        if (aimLine != null)
        {
            aimLine.enabled = show;
        }
    }

    public Vector2 GetAimDirection()
    {
        return aimDirection;
    }

    public abstract void Fire();
}