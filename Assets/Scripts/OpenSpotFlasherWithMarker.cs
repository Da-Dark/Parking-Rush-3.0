using System.Collections;
using UnityEngine;

public class OpenSpotFlasherWithMarker : MonoBehaviour
{
    [Header("Flashing Settings")]
    public Color flashColor = Color.yellow;
    public float initialIntensity = 5f;
    public float flashSpeed = 3f;
    public float fadeOutDuration = 2f;

    private Material markerMat;
    private Coroutine flashRoutine;

    PlayerCarFlasher playerCarFlasher;
    private Transform playerTransform;

    private bool isFadingOut = false;

    void Start()
    {
        // Try to find the player and its flasher
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCarFlasher = player.GetComponent<PlayerCarFlasher>();
            playerTransform = player.transform;
        }

        // Setup material
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            markerMat = new Material(rend.material);
            markerMat.EnableKeyword("_EMISSION");
            rend.material = markerMat;
        }

        // Start flashing
        flashRoutine = StartCoroutine(FlashEffect());
    }

    void Update()
    {
        if (playerCarFlasher.isFlashing && !isFadingOut && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            StartCoroutine(FadeOutFlashing());
        }
    }

    private IEnumerator FlashEffect()
    {
        float intensity = initialIntensity;

        while (!isFadingOut)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = flashColor * t * intensity;
            if (markerMat != null)
                markerMat.SetColor("_EmissionColor", currentColor);
            yield return null;
        }
    }
    private IEnumerator FadeOutFlashing()
    {
        isFadingOut = true;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        // Record current emission colors
        Color[] initialColors = new Color[playerCarFlasher.flashMats.Length];
        for (int i = 0; i < playerCarFlasher.flashMats.Length; i++)
            initialColors[i] = playerCarFlasher.flashMats[i].GetColor("_EmissionColor");

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);

            for (int i = 0; i < playerCarFlasher.flashMats.Length; i++)
            {
                playerCarFlasher.flashMats[i].SetColor("_EmissionColor", initialColors[i] * t);
            }

            yield return null;
        }

        foreach (Material mat in playerCarFlasher.flashMats)
        {
            mat.SetColor("_EmissionColor", Color.black);
        }

        playerCarFlasher.isFlashing = false;
        isFadingOut = false;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        Color initialColor = markerMat.GetColor("_EmissionColor");
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            markerMat.SetColor("_EmissionColor", initialColor * t);
            yield return null;
        }

        markerMat.SetColor("_EmissionColor", Color.black);
        Destroy(gameObject);
    }
}
