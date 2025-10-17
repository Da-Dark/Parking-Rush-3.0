using UnityEngine;
using System.Collections; // ✅ Needed for IEnumerator

public class OpenSpotFlasherWithMarker : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;   // The glow color
    public float flashSpeed = 2f;             // Speed of the flashing
    public float emissionIntensity = 2f;      // How bright the glow is

    private Material flashingMaterial;
    private Coroutine flashRoutine;
    private bool isFlashing = true;

    private void Start()
    {
        // Clone material so we don’t affect all objects using it
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            flashingMaterial = new Material(rend.material);
            rend.material = flashingMaterial;
        }

        // Start flashing automatically at game start
        if (flashingMaterial != null)
        {
            flashRoutine = StartCoroutine(FlashEffect());
        }
        else
        {
            Debug.LogWarning($"⚠️ No Renderer found on {gameObject.name} for flashing!");
        }
    }

    private void Update()
    {
        // Stop flashing if player makes their first input
        if (isFlashing && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            StopFlashing();
        }
    }

    private IEnumerator FlashEffect()
    {
        while (true)
        {
            // Pulse between off (black) and bright color
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = Color.Lerp(Color.black, flashColor, t);
            flashingMaterial.SetColor("_EmissionColor", currentColor * emissionIntensity);
            yield return null;
        }
    }

    /// <summary>
    /// Stops the flashing and disables emission glow.
    /// </summary>
    public void StopFlashing()
    {
        if (!isFlashing) return;

        isFlashing = false;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        if (flashingMaterial != null)
        {
            flashingMaterial.SetColor("_EmissionColor", Color.black);
        }

        Debug.Log($"🛑 Flashing stopped on {gameObject.name}");
    }
}
