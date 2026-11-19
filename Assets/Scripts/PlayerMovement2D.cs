using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
[Header("Input Actions (New Input System)")]
// Requires a 2D Vector InputAction configured with Up (W), Down (S), Left (A), and Right (D) bindings.
[SerializeField] private InputAction moveAction;

// Requires a Button InputAction bound to Left Shift (<Keyboard>/leftShift) to trigger sprinting.
[SerializeField] private InputAction sprintAction;

// Requires a Button InputAction bound to C (<Keyboard>/c) to trigger the fast directional dash.
[SerializeField] private InputAction dashAction;

[Header("Movement Settings")]
// Requires a positive float value representing base top-down movement speed in units per second (e.g., 5).
[SerializeField] private float moveSpeed = 5f;

// Requires a float multiplier applied to moveSpeed while sprinting (1.5 equals 1.5x normal speed).
[SerializeField] private float sprintSpeedMultiplier = 1.5f;

[Header("Sprint Bar Settings")]
// Requires a positive float representing the maximum capacity of the sprint stamina bar (e.g., 100).
[SerializeField] private float maxStamina = 100f;

// Requires a positive float representing how much stamina drains per second while sprinting (e.g., 25).
[SerializeField] private float staminaDrainRate = 25f;

// Requires a positive float representing how much stamina refills per second while walking or idle (e.g., 20).
[SerializeField] private float staminaRegenRate = 20f;

// Requires a reference to a Unity UI Slider component on the Canvas to display the sprint bar on screen.
[SerializeField] private Slider sprintBarSlider;

[Header("Dash Settings")]
// Requires a high positive float representing movement speed in units per second during the dash burst (e.g., 25).
[SerializeField] private float dashSpeed = 25f;

// Requires a short positive float duration in seconds for how long the fast dash burst lasts (e.g., 0.12).
[SerializeField] private float dashDuration = 0.12f;

// Requires a positive float representing the cooldown duration in seconds between dashes (set to 5 seconds).
[SerializeField] private float dashCooldown = 5f;

// Requires a reference to a Unity UI Text component on the Canvas to display the 5-second dash cooldown timer on screen.
[SerializeField] private Text dashCooldownText;

// Cached reference to the Rigidbody2D component on this GameObject.
private Rigidbody2D rb;

// Stores the normalized 2D movement input vector (X for Left/Right, Y for Down/Up).
private Vector2 moveInput;

// Stores the last non-zero movement direction so the player can still dash forward when standing still.
private Vector2 lastMoveDirection = Vector2.right;

// Tracks the player's current available sprint stamina.
private float currentStamina;

// Tracks whether the player is actively sprinting this frame.
private bool isSprinting;

// Prevents sprint flickering when stamina hits zero until a small buffer has refilled.
private bool isStaminaExhausted;

// Tracks whether the player is currently mid-dash.
private bool isDashing;

// Tracks the remaining time in seconds for the active dash burst.
private float dashTimeRemaining;

// Tracks the remaining cooldown time in seconds before the player can dash again.
private float dashCooldownRemaining;

// Stores the locked normalized direction for the active dash burst.
private Vector2 dashDirection;

// Public read-only property allowing other scripts (like PlayerSpriteController) to read movement direction safely.
public Vector2 MoveInput => moveInput;

// Public read-only property indicating whether the player is currently sprinting.
public bool IsSprinting => isSprinting;

// Public read-only property indicating whether the player is currently dashing.
public bool IsDashing => isDashing;

// Automatically populates default WASD, Left Shift, and C bindings in the Inspector when the script is added or reset.
private void Reset()
{
    SetupDefaultMoveBinding();
    SetupDefaultSprintBinding();
    SetupDefaultDashBinding();
}

private void Awake()
{
    InitializeComponents();
    ValidateInputActions();
    InitializeSprintAndDashState();
}

// Enables all InputActions when the Player GameObject becomes active.
private void OnEnable()
{
    EnableInputActions();
}

// Disables all InputActions when the Player GameObject is disabled or destroyed to prevent memory leaks.
private void OnDisable()
{
    DisableInputActions();
}

// Calls single-responsibility methods every frame to process input, sprint stamina, dash timers, and UI updates.
private void Update()
{
    ReadMovementInput();
    TrackLastMoveDirection();
    UpdateSprintState();
    UpdateStamina();
    HandleDashInput();
    UpdateDashTimer();
    UpdateDashCooldownTimer();
    UpdateSprintBarUI();
    UpdateDashCooldownUI();
}

private void FixedUpdate()
{
    ApplyTopDownMovement();
}

// Configures a default 2DVector composite binding (W = Up, S = Down, A = Left, D = Right) on moveAction.
private void SetupDefaultMoveBinding()
{
    moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
    moveAction.AddCompositeBinding("2DVector")
        .With("Up", "<Keyboard>/w")
        .With("Down", "<Keyboard>/s")
        .With("Left", "<Keyboard>/a")
        .With("Right", "<Keyboard>/d");
}

// Configures a default Button binding on sprintAction mapped to Left Shift.
private void SetupDefaultSprintBinding()
{
    sprintAction = new InputAction("Sprint", InputActionType.Button, binding: "<Keyboard>/leftShift");
}

// Configures a default Button binding on dashAction mapped to the C key.
private void SetupDefaultDashBinding()
{
    dashAction = new InputAction("Dash", InputActionType.Button, binding: "<Keyboard>/c");
}

// Caches the Rigidbody2D component, disables gravity for top-down play, and locks Z-axis rotation.
private void InitializeComponents()
{
    rb = GetComponent<Rigidbody2D>();
    rb.gravityScale = 0f;
    rb.freezeRotation = true;
}

// Ensures moveAction, sprintAction, and dashAction all have valid bindings before OnEnable activates them.
private void ValidateInputActions()
{
    if (moveAction == null || moveAction.expectedControlType != "Vector2" || !HasCompositeBinding(moveAction))
    {
        SetupDefaultMoveBinding();
    }

    if (sprintAction == null || sprintAction.bindings.Count == 0)
    {
        SetupDefaultSprintBinding();
    }

    if (dashAction == null || dashAction.bindings.Count == 0)
    {
        SetupDefaultDashBinding();
    }
}

// Sets starting stamina to full, clears dash cooldown, and configures the sprint bar UI slider range.
private void InitializeSprintAndDashState()
{
    currentStamina = maxStamina;
    dashCooldownRemaining = 0f;

    if (sprintBarSlider != null)
    {
        sprintBarSlider.minValue = 0f;
        sprintBarSlider.maxValue = maxStamina;
        sprintBarSlider.value = currentStamina;
    }
}

// Checks whether the provided InputAction contains at least one composite binding (such as 2DVector).
private bool HasCompositeBinding(InputAction action)
{
    for (int i = 0; i < action.bindings.Count; i++)
    {
        if (action.bindings[i].isComposite)
        {
            return true;
        }
    }

    return false;
}

// Activates moveAction, sprintAction, and dashAction so they begin reading hardware input.
private void EnableInputActions()
{
    if (moveAction != null)
    {
        moveAction.Enable();
    }

    if (sprintAction != null)
    {
        sprintAction.Enable();
    }

    if (dashAction != null)
    {
        dashAction.Enable();
    }
}

// Deactivates moveAction, sprintAction, and dashAction when no longer needed.
private void DisableInputActions()
{
    if (moveAction != null)
    {
        moveAction.Disable();
    }

    if (sprintAction != null)
    {
        sprintAction.Disable();
    }

    if (dashAction != null)
    {
        dashAction.Disable();
    }
}

// Reads the 2D vector from moveAction and clamps its magnitude to 1 so diagonal movement is not faster than straight movement.
private void ReadMovementInput()
{
    Vector2 rawInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    moveInput = Vector2.ClampMagnitude(rawInput, 1f);
}

// Saves the normalized movement direction whenever the player is actively moving.
private void TrackLastMoveDirection()
{
    if (moveInput.sqrMagnitude > 0.01f)
    {
        lastMoveDirection = moveInput.normalized;
    }
}

// Determines whether the player can sprint based on Left Shift input, movement, dash state, and available stamina.
private void UpdateSprintState()
{
    bool isHoldingSprint = sprintAction != null && sprintAction.IsPressed();
    bool isMoving = moveInput.sqrMagnitude > 0.01f;
    isSprinting = isHoldingSprint && isMoving && !isDashing && !isStaminaExhausted && currentStamina > 0f;
}

// Drains stamina while sprinting and refills stamina when the player is walking or standing still.
private void UpdateStamina()
{
    if (isSprinting)
    {
        currentStamina -= staminaDrainRate * Time.deltaTime;
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            isStaminaExhausted = true;
            isSprinting = false;
        }
    }
    else
    {
        currentStamina += staminaRegenRate * Time.deltaTime;
        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
        }

        if (isStaminaExhausted && currentStamina >= maxStamina * 0.2f)
        {
            isStaminaExhausted = false;
        }
    }
}

// Checks if C was pressed this frame and starts a dash if the player is not dashing and the 5-second cooldown is finished.
private void HandleDashInput()
{
    bool dashPressed = dashAction != null && dashAction.WasPressedThisFrame();
    if (dashPressed && !isDashing && dashCooldownRemaining <= 0f)
    {
        StartDash();
    }
}

// Initiates the high-speed directional burst dash and resets the 5-second cooldown timer.
private void StartDash()
{
    isDashing = true;
    isSprinting = false;
    dashTimeRemaining = dashDuration;
    dashCooldownRemaining = dashCooldown;
    dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastMoveDirection;
}

// Counts down the short burst duration of an active dash and ends the dash when time expires.
private void UpdateDashTimer()
{
    if (!isDashing)
    {
        return;
    }

    dashTimeRemaining -= Time.deltaTime;
    if (dashTimeRemaining <= 0f)
    {
        isDashing = false;
    }
}

// Counts down the 5-second dash cooldown timer every frame until it reaches zero.
private void UpdateDashCooldownTimer()
{
    if (dashCooldownRemaining > 0f)
    {
        dashCooldownRemaining -= Time.deltaTime;
        if (dashCooldownRemaining < 0f)
        {
            dashCooldownRemaining = 0f;
        }
    }
}

// Updates the on-screen UI Slider to reflect the player's current sprint stamina.
private void UpdateSprintBarUI()
{
    if (sprintBarSlider != null)
    {
        sprintBarSlider.value = currentStamina;
    }
}

// Updates the on-screen UI Text to show either the remaining dash cooldown in seconds or that the dash is ready.
private void UpdateDashCooldownUI()
{
    if (dashCooldownText != null)
    {
        if (dashCooldownRemaining > 0f)
        {
            dashCooldownText.text = "Dash: " + dashCooldownRemaining.ToString("F1") + "s";
        }
        else
        {
            dashCooldownText.text = "Dash: Ready";
        }
    }
}

// Applies 2D physics velocity for either the fast burst dash or normal/sprinting top-down movement.
private void ApplyTopDownMovement()
{
    if (isDashing)
    {
        rb.linearVelocity = dashDirection * dashSpeed;
        return;
    }

    float activeMoveSpeed = isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
    rb.linearVelocity = moveInput * activeMoveSpeed;
}
}