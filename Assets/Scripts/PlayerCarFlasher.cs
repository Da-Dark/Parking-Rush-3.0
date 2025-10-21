using UnityEngine;
using System.Collections;

public class PlayerCarFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;
    public float initialIntensity = 5f;  // Strong start
    public float flashSpeed = 3f;        // Speed of flashing
    public float fadeOutDuration = 2f;   // Duration to fade out after first input

    private Material[] flashMats;
    private Coroutine flashRoutine;
    private bool isFlashing = true;
    private bool isFadingOut = false;

    void Start()
    {
        // Only flash on Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() != 1)
        {
            isFlashing = false;
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        flashMats = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            flashMats[i] = new Material(renderers[i].material);
            flashMats[i].EnableKeyword("_EMISSION");
            renderers[i].material = flashMats[i];
        }

        if (flashMats.Length > 0)
            flashRoutine = StartCoroutine(FlashEffect());
    }

    void Update()
    {
        if (isFlashing && !isFadingOut && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            StartCoroutine(FadeOutFlashing());
        }
    }

    private IEnumerator FlashEffect()
    {
        float intensity = initialIntensity;

        while (isFlashing)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = flashColor * t * intensity;

            foreach (Material mat in flashMats)
            {
                mat.SetColor("_EmissionColor", currentColor);
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutFlashing()
    {
        isFadingOut = true;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        // Record current emission colors
        Color[] initialColors = new Color[flashMats.Length];
        for (int i = 0; i < flashMats.Length; i++)
            initialColors[i] = flashMats[i].GetColor("_EmissionColor");

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);

            for (int i = 0; i < flashMats.Length; i++)
            {
                flashMats[i].SetColor("_EmissionColor", initialColors[i] * t);
            }

            yield return null;
        }

        foreach (Material mat in flashMats)
        {
            mat.SetColor("_EmissionColor", Color.black);
        }

        isFlashing = false;
        isFadingOut = false;
    }
}
