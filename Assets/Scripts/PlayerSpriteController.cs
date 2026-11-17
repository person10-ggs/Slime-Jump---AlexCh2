using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement2D))]
public class PlayerSpriteController : MonoBehaviour
{
[Header("Top-Down Directional Sprites")]
// Requires a Sprite asset displayed when the player is standing still.
[SerializeField] private Sprite idleSprite;

// Requires a Sprite asset displayed when the player is moving upward (W key), including upward diagonals.
[SerializeField] private Sprite moveUpSprite;

// Requires a Sprite asset displayed when the player is moving downward (S key), including downward diagonals.
[SerializeField] private Sprite moveDownSprite;

// Requires a Sprite asset displayed when the player is moving strictly left (A key).
[SerializeField] private Sprite moveLeftSprite;

// Requires a Sprite asset displayed when the player is moving strictly right (D key).
[SerializeField] private Sprite moveRightSprite;

[Header("Detection Thresholds")]
// Requires a small positive float (e.g., 0.1) to ignore tiny floating-point input values near zero.
[SerializeField] private float inputThreshold = 0.1f;

// Cached reference to the SpriteRenderer component that draws the player's sprite.
private SpriteRenderer spriteRenderer;

// Cached reference to the PlayerMovement2D script that provides movement direction data.
private PlayerMovement2D playerMovement;

private void Awake()
{
    InitializeComponents();
}

private void Update()
{
    UpdateDirectionalSprite();
}

// Retrieves and caches references to the required components on the Player GameObject.
private void InitializeComponents()
{
    spriteRenderer = GetComponent<SpriteRenderer>();
    playerMovement = GetComponent<PlayerMovement2D>();
}

// Evaluates the 2D movement input vector and selects a static sprite, prioritizing Up/Down over Left/Right.
private void UpdateDirectionalSprite()
{
    Vector2 currentInput = playerMovement.MoveInput;

    if (currentInput.y > inputThreshold)
    {
        ApplySprite(moveUpSprite);
    }
    else if (currentInput.y < -inputThreshold)
    {
        ApplySprite(moveDownSprite);
    }
    else if (currentInput.x > inputThreshold)
    {
        ApplySprite(moveRightSprite);
    }
    else if (currentInput.x < -inputThreshold)
    {
        ApplySprite(moveLeftSprite);
    }
    else
    {
        ApplySprite(idleSprite);
    }
}

// Safely assigns the target sprite to the SpriteRenderer if it is valid and not already active.
private void ApplySprite(Sprite targetSprite)
{
    if (targetSprite != null && spriteRenderer.sprite != targetSprite)
    {
        spriteRenderer.sprite = targetSprite;
    }
}
}