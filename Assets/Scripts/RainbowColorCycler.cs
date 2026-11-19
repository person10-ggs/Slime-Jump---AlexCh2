using UnityEngine;

// Ensures Unity automatically attaches a Renderer component if the GameObject does not already have one.
[RequireComponent(typeof(Renderer))]
public class RainbowColorCycler : MonoBehaviour
{
// REQUIRED DATA: A positive decimal (float) number representing how many full rainbow cycles occur per second (e.g., 0.25 takes 4 seconds per cycle).
[SerializeField] private float cycleSpeed = 0.25f;

// Stores a cached reference to the GameObject's Renderer component for efficient access.
private Renderer objectRenderer;

// Tracks the current color position (hue) on a scale from 0.0 to 1.0.
private float currentHue;

// Awake is called by Unity when the script instance is first loaded.
private void Awake()
{
    // Calls our setup method to find and store the Renderer component.
    InitializeRenderer();
}

// Update is called once per frame by Unity and only delegates work to other methods.
private void Update()
{
    // Calls the method responsible for updating the GameObject's color each frame.
    ApplyRainbowColor();
}

// Finds and caches the Renderer component attached to this same GameObject.
private void InitializeRenderer()
{
    // Retrieves the Renderer component and saves it in our private variable.
    objectRenderer = GetComponent<Renderer>();
}

// Coordinates advancing the hue value and applying the resulting color to the material.
private void ApplyRainbowColor()
{
    // Moves the hue forward based on elapsed frame time.
    AdvanceHue();

    // Converts the updated hue into an RGB Color and applies it to the object's material.
    objectRenderer.material.color = ConvertHueToRainbowColor(currentHue);
}

// Increases the hue value smoothly over time and loops it back to 0 once it reaches 1.
private void AdvanceHue()
{
    // Multiplies speed by Time.deltaTime for frame-rate independence, then wraps the total between 0.0 and 1.0.
    currentHue = Mathf.Repeat(currentHue + (cycleSpeed * Time.deltaTime), 1f);
}

// Takes a hue value (0.0 to 1.0) and returns a vibrant RGB Color.
private Color ConvertHueToRainbowColor(float hueValue)
{
    // Uses Hue, Saturation (1 = full color), and Value/Brightness (1 = full brightness) to generate a rainbow color.
    return Color.HSVToRGB(hueValue, 1f, 1f);
}
}