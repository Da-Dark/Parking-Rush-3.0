using UnityEngine;
using System.Collections;

public class OpenSpotFlasherWithMarker : MonoBehaviour
{
    public Color flashColor = Color.yellow;
    public float intensity = 6f;
    public float flashSpeed = 3.5f;
    public float fadeOutDuration = 1.5f;

    private Material mat;
    private Coroutine flashRoutine;
    private bool isFadingOut = false;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        mat = new Material(r.material);
        r.material = mat;
        mat.EnableKeyword("_EMISSION");

        flashRoutine = StartCoroutine(FlashForever());
    }

    private IEnumerator FlashForever()
    {
        while (true)
        {
            if (!isFadingOut)
            {
                float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
                mat.SetColor("_EmissionColor", flashColor * t * intensity);
            }
            yield return null;
        }
    }

    public void FadeOutOnce()
    {
        if (!isFadingOut)
            StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        isFadingOut = true;

        Color start = mat.GetColor("_EmissionColor");
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float lerp = 1f - (t / fadeOutDuration);
            mat.SetColor("_EmissionColor", start * lerp);
            yield return null;
        }

        mat.SetColor("_EmissionColor", Color.black);
        enabled = false;
    }
}
