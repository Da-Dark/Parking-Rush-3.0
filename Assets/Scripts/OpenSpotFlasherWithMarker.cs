using UnityEngine;
using System.Collections;

public class OpenSpotFlasherWithMarker : MonoBehaviour
{
    [Header("Flashing Settings")]
    public Color flashColor = Color.yellow;
    public float flashSpeed = 2f;
    public float emissionIntensity = 2f;

    [HideInInspector]
    public bool shouldStopOnFirstInput = false; // set by SpawnManager

    private Material flashMat;
    private Coroutine flashRoutine;
    private bool isFlashing = true;

    void Awake()
    {
        // Ensure the object has a Renderer
        Renderer r = GetComponent<Renderer>();
        if (r == null)
        {
            Debug.LogWarning($"⚠️ No Renderer found on {gameObject.name}, flashing disabled.");
            isFlashing = false;
            return;
        }

        // Use a unique material instance
        flashMat = new Material(r.material);
        r.material = flashMat;

        // Start flashing
        flashRoutine = StartCoroutine(FlashEffect());
    }

    void Update()
    {
        // Stop flashing on first player input
        if (isFlashing && shouldStopOnFirstInput &&
            (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
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
            flashMat.SetColor("_EmissionColor", currentColor * emissionIntensity);
            yield return null;
        }
    }

    public void StopFlashing()
    {
        if (!isFlashing) return;

        isFlashing = false;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (flashMat != null)
            flashMat.SetColor("_EmissionColor", Color.black);

        // Destroy the marker completely
        Destroy(gameObject);

        Debug.Log($"🛑 Flashing stopped and marker destroyed for {gameObject.name}");
    }
}
