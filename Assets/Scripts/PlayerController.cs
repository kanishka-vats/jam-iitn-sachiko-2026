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

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // Completely stop movement when movement is disabled
        if (!movementEnabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity =
                FacingDirection *
                (dashSpeed * dashSpeedMultiplier);
        }
        else
        {
            rb.linearVelocity =
                moveDirection *
                (moveSpeed * moveSpeedMultiplier);
        }
    }

    private void HandleInput()
    {
        // Don't process movement or dash input
        // while movement is disabled
        if (!movementEnabled)
        {
            moveDirection = Vector2.zero;
            isMoving = false;

            // Cancel an active dash
            isDashing = false;
            dashTimer = 0f;

            return;
        }

        Vector2 input =
            playerInput.actions["Move"].ReadValue<Vector2>();

        float horizontalInput = input.x;
        float verticalInput = input.y;

        // Apply inverted controls if power-up is active
        if (invertControlsX)
            horizontalInput *= -1f;

        if (invertControlsY)
            verticalInput *= -1f;

        moveDirection =
            new Vector2(
                horizontalInput,
                verticalInput
            ).normalized;

        if (moveDirection.magnitude > 0)
        {
            FacingDirection = moveDirection;
            lastDirection = moveDirection;
            isMoving = true;

            // Flip sprite based on horizontal movement.
            // The base Player_Left sprite faces LEFT,
            // so flip it only when moving RIGHT.
            if (horizontalInput < 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (horizontalInput > 0)
            {
                spriteRenderer.flipX = true;
            }
        }
        else
        {
            isMoving = false;
            FacingDirection = lastDirection;
        }

        // Handle dash input
        if (playerInput.actions["Dash"].WasPressedThisFrame() &&
            dashCooldownTimer <= 0 &&
            !isDashing)
        {
            StartDash();
        }
    }

    private void HandleFire()
    {
        if (playerInput.actions["Fire"].IsPressed() &&
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

        // Keep existing animation parameters
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsShooting", isShooting);

        // Freeze animation when not moving
        if (!isMoving)
        {
            animator.speed = 0f;
            return;
        }

        animator.speed = 1f;

        float x = moveDirection.x;
        float y = moveDirection.y;

        // UP
        if (y > 0.5f && Mathf.Abs(y) >= Mathf.Abs(x))
        {
            animator.Play("Player_Up");
        }
        // DOWN
        else if (y < -0.5f && Mathf.Abs(y) >= Mathf.Abs(x))
        {
            animator.Play("Player_Down");
        }
        // LEFT / RIGHT
        else
        {
            animator.Play("Player_Left");
        }
    }

    private void StartDash()
    {
        // Don't allow dash while movement is disabled
        if (!movementEnabled)
            return;

        AudioManager.Instance.PlaySFX(SFXType.PlayerDash);

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer =
            dashCooldown * dashCooldownMultiplier;
    }

    #region Movement Control

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            // Immediately stop current movement
            moveDirection = Vector2.zero;
            isMoving = false;

            // Cancel dash
            isDashing = false;
            dashTimer = 0f;

            // Stop Rigidbody movement immediately
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
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

        // Disable old weapon if exists
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        // Enable new weapon
        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
    }

    public void SwapWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
            return;

        // Destroy old weapon
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        // Instantiate new weapon as child
        GameObject weaponObj =
            Instantiate(weaponPrefab, transform);

        currentWeapon =
            weaponObj.GetComponent<WeaponBase>();
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

        Debug.Log(
            "Inverted Controls: " +
            inverted
        );
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
        {
            StartDash();
        }
    }

    #endregion

    #region Getters

    public float GetDashCooldownPercent()
    {
        return 1f -
            (dashCooldownTimer /
            (dashCooldown * dashCooldownMultiplier));
    }

    public bool CanDash()
    {
        return dashCooldownTimer <= 0 && !isDashing;
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