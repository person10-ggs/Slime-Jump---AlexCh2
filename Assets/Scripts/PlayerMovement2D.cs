using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
[Header("Input Actions (New Input System)")]
// Requires a 2D Vector InputAction configured with Up (W), Down (S), Left (A), and Right (D) bindings.
[SerializeField] private InputAction moveAction;

[Header("Movement Settings")]
// Requires a positive float value representing top-down movement speed in units per second (e.g., 5).
[SerializeField] private float moveSpeed = 5f;

// Cached reference to the Rigidbody2D component on this GameObject.
private Rigidbody2D rb;

// Stores the normalized 2D movement input vector (X for Left/Right, Y for Down/Up).
private Vector2 moveInput;

// Public read-only property allowing other scripts (like PlayerSpriteController) to read movement direction safely.
public Vector2 MoveInput => moveInput;

// Automatically populates default W/S/A/D 2DVector bindings in the Inspector when the script is added or reset.
private void Reset()
{
    SetupDefaultInputBindings();
}

private void Awake()
{
    InitializeComponents();
    ValidateInputActions();
}

// Enables the Move InputAction when the Player GameObject becomes active so Unity listens for WASD input.
private void OnEnable()
{
    EnableInputActions();
}

// Disables the Move InputAction when the Player GameObject is disabled or destroyed to prevent memory leaks.
private void OnDisable()
{
    DisableInputActions();
}

private void Update()
{
    ReadMovementInput();
}

private void FixedUpdate()
{
    ApplyTopDownMovement();
}

// Configures a default 2DVector composite binding (W = Up, S = Down, A = Left, D = Right) on moveAction.
private void SetupDefaultInputBindings()
{
    moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
    moveAction.AddCompositeBinding("2DVector")
        .With("Up", "<Keyboard>/w")
        .With("Down", "<Keyboard>/s")
        .With("Left", "<Keyboard>/a")
        .With("Right", "<Keyboard>/d");
}

// Caches the Rigidbody2D component, disables gravity for top-down play, and locks Z-axis rotation.
private void InitializeComponents()
{
    rb = GetComponent<Rigidbody2D>();
    rb.gravityScale = 0f;
    rb.freezeRotation = true;
}

// Ensures moveAction is configured as a Vector2 action with a composite binding before OnEnable activates it.
private void ValidateInputActions()
{
    if (moveAction == null || moveAction.expectedControlType != "Vector2" || !HasCompositeBinding(moveAction))
    {
        SetupDefaultInputBindings();
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

// Activates the moveAction so it begins reading hardware input.
private void EnableInputActions()
{
    if (moveAction != null)
    {
        moveAction.Enable();
    }
}

// Deactivates the moveAction when no longer needed.
private void DisableInputActions()
{
    if (moveAction != null)
    {
        moveAction.Disable();
    }
}

// Reads the 2D vector from moveAction and clamps its magnitude to 1 so diagonal movement is not faster than straight movement.
private void ReadMovementInput()
{
    Vector2 rawInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    moveInput = Vector2.ClampMagnitude(rawInput, 1f);
}

// Applies 2D velocity directly to the Rigidbody2D in all directions based on moveInput and moveSpeed.
private void ApplyTopDownMovement()
{
    rb.linearVelocity = moveInput * moveSpeed;
}
}