
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Rigidbody2D rb;

    private bool movementEnabled = true;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;

    private float dashCooldownTimer = 0f;
    private float dashTimer = 0f;
    private bool isDashing = false;
    private bool invertedControls = false;

    [Header("Direction")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;

    [Header("Weapon")]
    [SerializeField] private WeaponBase currentWeapon;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool isMoving = false;
    private bool isShooting = false;

    [Header("Modifiers (Powerups)")]
    [SerializeField] private bool invertControlsX = false;
    [SerializeField] private bool invertControlsY = false;

    private float moveSpeedMultiplier = 1f;
    private float dashSpeedMultiplier = 1f;
    private float dashCooldownMultiplier = 1f;

    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<WeaponBase>();
        }
    }

    private void Update()
    {
        HandleInput();
        HandleFire();
        UpdateAnimation();

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!movementEnabled)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        if (rb == null)
            return;

        if (isDashing)
        {
            rb.linearVelocity =
                FacingDirection * (dashSpeed * dashSpeedMultiplier);
        }
        else
        {
            rb.linearVelocity =
                moveDirection * (moveSpeed * moveSpeedMultiplier);
        }
    }

    private void HandleInput()
    {
        if (!movementEnabled)
        {
            moveDirection = Vector2.zero;
            isMoving = false;
            isDashing = false;
            dashTimer = 0f;
            return;
        }

        if (playerInput == null ||
            playerInput.actions["Move"] == null)
            return;

        Vector2 input =
            playerInput.actions["Move"].ReadValue<Vector2>();

        float horizontalInput = input.x;
        float verticalInput = input.y;

        if (invertControlsX)
            horizontalInput *= -1f;

        if (invertControlsY)
            verticalInput *= -1f;

        moveDirection =
            new Vector2(horizontalInput, verticalInput).normalized;

        if (moveDirection.magnitude > 0f)
        {
            FacingDirection = moveDirection;
            lastDirection = moveDirection;
            isMoving = true;

            if (spriteRenderer != null)
            {
                if (horizontalInput < 0f)
                    spriteRenderer.flipX = false;
                else if (horizontalInput > 0f)
                    spriteRenderer.flipX = true;
            }
        }
        else
        {
            isMoving = false;
            FacingDirection = lastDirection;
        }

        if (playerInput.actions["Dash"].WasPressedThisFrame() &&
            dashCooldownTimer <= 0f &&
            !isDashing)
        {
            StartDash();
        }
    }

    private void HandleFire()
    {
        // No shooting while the ending has frozen the player.
        if (!movementEnabled)
        {
            isShooting = false;
            return;
        }

        if (playerInput != null &&
            playerInput.actions["Fire"].IsPressed() &&
            currentWeapon != null)
        {
            currentWeapon.Fire();
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsShooting", isShooting);

        if (!isMoving)
        {
            animator.speed = 0f;
            return;
        }

        animator.speed = 1f;

        float x = moveDirection.x;
        float y = moveDirection.y;

        if (y > 0.5f && Mathf.Abs(y) >= Mathf.Abs(x))
        {
            animator.Play("Player_Up");
        }
        else if (y < -0.5f && Mathf.Abs(y) >= Mathf.Abs(x))
        {
            animator.Play("Player_Down");
        }
        else
        {
            animator.Play("Player_Left");
        }
    }

    private void StartDash()
    {
        if (!movementEnabled)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.PlayerDash);

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown * dashCooldownMultiplier;
    }

    #region Movement Control

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            moveDirection = Vector2.zero;
            isMoving = false;

            isDashing = false;
            dashTimer = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    public bool IsMovementEnabled()
    {
        return movementEnabled;
    }

    #endregion

    public void SwapWeapon(WeaponBase newWeapon)
    {
        if (newWeapon == null)
            return;

        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
    }

    public void SwapWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
            return;

        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        GameObject weaponObj = Instantiate(weaponPrefab, transform);
        currentWeapon = weaponObj.GetComponent<WeaponBase>();
    }

    #region Powerup Modification Methods

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier = multiplier;
    }

    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier += amount;
    }

    public void SetDashSpeedMultiplier(float multiplier)
    {
        dashSpeedMultiplier = multiplier;
    }

    public void AddDashSpeedMultiplier(float amount)
    {
        dashSpeedMultiplier += amount;
    }

    public void SetDashCooldownMultiplier(float multiplier)
    {
        dashCooldownMultiplier = multiplier;
    }

    public void AddDashCooldownMultiplier(float amount)
    {
        dashCooldownMultiplier += amount;
    }

    public void SetDashDuration(float duration)
    {
        dashDuration = duration;
    }

    public void SetInvertControlsX(bool invert)
    {
        invertControlsX = invert;
    }

    public void SetInvertControlsY(bool invert)
    {
        invertControlsY = invert;
    }

    public void SetInvertedControls(bool inverted)
    {
        invertedControls = inverted;
        invertControlsX = inverted;
        invertControlsY = inverted;

        Debug.Log("Inverted Controls: " + inverted);
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void ResetDashCooldown()
    {
        dashCooldownTimer = 0f;
    }

    public void ForceDash()
    {
        if (!isDashing)
            StartDash();
    }

    #endregion

    #region Getters

    public float GetDashCooldownPercent()
    {
        float totalCooldown = dashCooldown * dashCooldownMultiplier;

        if (totalCooldown <= 0f)
            return 1f;

        return 1f - (dashCooldownTimer / totalCooldown);
    }

    public bool CanDash()
    {
        return dashCooldownTimer <= 0f && !isDashing;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetMoveSpeedMultiplier()
    {
        return moveSpeedMultiplier;
    }

    public float GetCurrentMoveSpeed()
    {
        return moveSpeed * moveSpeedMultiplier;
    }

    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }

    #endregion
}
