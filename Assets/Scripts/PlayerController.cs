using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    private float dashCooldownTimer = 0f;
    private float dashTimer = 0f;
    private bool isDashing = false;
    
    [Header("Direction")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2 FacingDirection { get; private set; } = Vector2.right;
    
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;
    
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
    }

    private void Update()
    {
        HandleInput();
        
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
        if (isDashing)
        {
            rb.linearVelocity = FacingDirection * (dashSpeed * dashSpeedMultiplier);
        }
        else
        {
            rb.linearVelocity = moveDirection * (moveSpeed * moveSpeedMultiplier);
        }
    }

    private void HandleInput()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
        
        float horizontalInput = input.x;
        float verticalInput = input.y;
        
        // Apply inverted controls if powerup active
        if (invertControlsX) horizontalInput *= -1f;
        if (invertControlsY) verticalInput *= -1f;
        
        moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        
        if (moveDirection.magnitude > 0)
        {
            FacingDirection = moveDirection;
            lastDirection = moveDirection;
            
            if (horizontalInput < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (horizontalInput > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            FacingDirection = lastDirection;
        }
        
        // Handle dash input
        if (playerInput.actions["Dash"].WasPressedThisFrame() && dashCooldownTimer <= 0 && !isDashing)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown * dashCooldownMultiplier;
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
        return 1f - (dashCooldownTimer / (dashCooldown * dashCooldownMultiplier));
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
    
    #endregion
}
