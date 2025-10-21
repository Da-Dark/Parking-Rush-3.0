using UnityEngine;
using System.Collections;

public class OpenSpotFlasherWithMarker : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;
    public float flashSpeed = 2f;
    public float emissionIntensity = 2f;
    public float fadeOutDuration = 1f; // How long to fade out after first input

    private Material flashMat;
    private Coroutine flashRoutine;
    private bool isFlashing = true;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null)
        {
            Debug.LogWarning($"No Renderer found on {gameObject.name}, cannot flash!");
            return;
        }

        // Make a unique material instance for this marker
        flashMat = new Material(r.material);
        r.material = flashMat;

        // Start flashing
        flashRoutine = StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        while (isFlashing)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = Color.Lerp(Color.black, flashColor, t);
            flashMat.SetColor("_EmissionColor", currentColor * emissionIntensity);
            yield return null;
        }
    }

    /// <summary>
    /// Call this when player makes their first input.
    /// </summary>
    public void StopFlashing()
    {
        if (!isFlashing) return;

        isFlashing = false;

        // Stop the flashing coroutine
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        // Start fading out smoothly
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        Color initialEmission = flashMat.GetColor("_EmissionColor");

        while (timer < fadeOutDuration)
        {
            float t = timer / fadeOutDuration;
            flashMat.SetColor("_EmissionColor", Color.Lerp(initialEmission, Color.black, t));
            timer += Time.deltaTime;
            yield return null;
        }

        flashMat.SetColor("_EmissionColor", Color.black);

        // Destroy the marker GameObject after fading
        Destroy(gameObject);
    }
}
