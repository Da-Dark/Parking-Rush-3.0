using UnityEngine;
using System.Collections;

public class PlayerCarFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;
    public float initialIntensity = 5f;
    public float flashSpeed = 3f;
    public float fadeOutDuration = 2f;

    [HideInInspector] public Material[] flashMats;
    [HideInInspector] public Coroutine flashRoutine;
    public bool isFlashing = true;
    public bool isFadingOut = false;

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
            Material m = new Material(renderers[i].material);
            m.EnableKeyword("_EMISSION");
            renderers[i].material = m;
            flashMats[i] = m;
        }

        if (flashMats.Length > 0)
            flashRoutine = StartCoroutine(FlashEffect());
    }

    void Update()
    {
        // The PlayerController handles first input and calls TriggerFadeOut, we keep a fallback here
        if (isFlashing && !isFadingOut && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            TriggerFadeOut();
        }
    }

    public IEnumerator FlashEffect()
    {
        while (isFlashing)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = flashColor * t * initialIntensity;

            foreach (Material mat in flashMats)
            {
                mat.SetColor("_EmissionColor", currentColor);
            }

            yield return null;
        }
    }

    // Public method other scripts call to start fade-out
    public void TriggerFadeOut()
    {
        if (isFadingOut || !isFlashing) return;
        StartCoroutine(FadeOutFlashing());
    }

    private IEnumerator FadeOutFlashing()
    {
        isFadingOut = true;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        // record current emission
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

        for (int i = 0; i < flashMats.Length; i++)
            flashMats[i].SetColor("_EmissionColor", Color.black);

        isFlashing = false;
        isFadingOut = false;
    }
}
