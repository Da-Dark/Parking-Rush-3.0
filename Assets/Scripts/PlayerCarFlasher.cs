using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class PlayerCarFlasherMulti : MonoBehaviour
{
    [Header("Flashing Settings")]
    public Color flashColor = Color.cyan;      // Color of the flashing overlay
    public float flashSpeed = 2f;              // Speed of the flash
    public float emissionIntensity = 2f;       // Intensity of the emission

    private Material[] flashMats;              // Store a separate material for each renderer
    private Coroutine flashRoutine;
    private bool isFlashing = false;

    void Start()
    {
        // Only flash if Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            flashMats = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                // Instantiate a unique material for each renderer
                flashMats[i] = new Material(renderers[i].material);
                renderers[i].material = flashMats[i];

                // Enable emission for each material
                flashMats[i].EnableKeyword("_EMISSION");
            }

            isFlashing = true;
            flashRoutine = StartCoroutine(FlashEffect());
        }
    }

    void Update()
    {
        // Stop flashing when the player makes the first input
        if (isFlashing && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            StopFlashing();
        }
    }

    private IEnumerator FlashEffect()
    {
        while (isFlashing)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            Color currentColor = Color.Lerp(Color.black, flashColor, t);

            foreach (Material mat in flashMats)
            {
                mat.SetColor("_EmissionColor", currentColor * emissionIntensity);
            }

            yield return null;
        }
    }

    public void StopFlashing()
    {
        if (!isFlashing) return;

        isFlashing = false;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (flashMats != null)
        {
            foreach (Material mat in flashMats)
            {
                if (mat != null)
                    mat.SetColor("_EmissionColor", Color.black);
            }
        }

        Debug.Log("🛑 Player car flashing stopped.");
    }
}
