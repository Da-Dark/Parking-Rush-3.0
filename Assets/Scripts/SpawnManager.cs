using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning")]
    [Tooltip("Assign all car spawner transforms (with tags: Slow, Medium, Fast)")]
    public GameObject movingCarPrefab;
    public GameObject[] carSpawnPoints; // Each spawner should be tagged Slow / Medium / Fast

    [Header("Spawn Timing (per tag)")]
    [Tooltip("Average time between spawns for each speed category")]
    public float slowSpawnInterval = 4f;
    public float mediumSpawnInterval = 2.5f;
    public float fastSpawnInterval = 1.2f;

    [Tooltip("How much randomness to add to spawn intervals (seconds)")]
    public float spawnIntervalVariance = 0.4f;

    public float firstSpawnDelay = 0f;

    [Header("Parked Cars System")]
    public GameObject[] parkedCars;
    public GameObject openSpotPrefab;
    public GameObject flashMarkerPrefab;

    private GameObject currentOpenSpotInstance;
    private GameObject previousOpenSpot;
    private GameObject activeMarker;
    private Coroutine markerFlashRoutine;
    private bool levelOpenSpotSpawned = false;

    [Header("Car Speeds (by tag)")]
    public float slowCarSpeed = 3f;
    public float mediumCarSpeed = 5f;
    public float fastCarSpeed = 8f;

    void Start()
    {
        // Randomly open one parking spot
        OpenSpot(true);

        // Start spawn loops for each tag
        StartCoroutine(SpawnCarsByTag("Slow", slowSpawnInterval));
        StartCoroutine(SpawnCarsByTag("Medium", mediumSpawnInterval));
        StartCoroutine(SpawnCarsByTag("Fast", fastSpawnInterval));
    }

    // -------------------------------
    // OPEN SPOT SYSTEM
    // -------------------------------
    public void OpenSpot(bool showMarker = false)
    {
        if (levelOpenSpotSpawned) return;
        levelOpenSpotSpawned = true;

        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        foreach (var car in parkedCars)
            if (car != null) car.SetActive(true);

        GameObject chosen = parkedCars[Random.Range(0, parkedCars.Length)];
        chosen.SetActive(false);
        previousOpenSpot = chosen;

        if (openSpotPrefab != null)
            currentOpenSpotInstance = Instantiate(openSpotPrefab, chosen.transform.position, chosen.transform.rotation);

        Debug.Log($"SpawnManager: Opened spot at {chosen.name}");

        if (showMarker && flashMarkerPrefab != null)
            SpawnMarkerAt(chosen.transform.position);
    }

    public void ResetLevelOpenFlag()
    {
        levelOpenSpotSpawned = false;
    }

    private void SpawnMarkerAt(Vector3 position)
    {
        if (flashMarkerPrefab == null) return;

        Vector3 spawnPos = position + Vector3.up * 0.5f;
        activeMarker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);
        activeMarker.tag = "Marker";

        Renderer rend = activeMarker.GetComponent<Renderer>();
        if (rend != null)
        {
            Material matInstance = new Material(rend.material);
            matInstance.EnableKeyword("_EMISSION");
            rend.material = matInstance;
            markerFlashRoutine = StartCoroutine(MarkerFlashLoop(matInstance));
        }
    }

    private IEnumerator MarkerFlashLoop(Material mat)
    {
        float speed = 2f;
        float baseIntensity = 1.2f;

        while (true)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            Color c = Color.Lerp(Color.yellow, Color.white, t);
            mat.SetColor("_EmissionColor", c * baseIntensity);
            yield return null;
        }
    }

    public void HideGlowMarker()
    {
        if (activeMarker == null) return;
        StopMarkerFlash();
        StartCoroutine(FadeOutAndDestroyMarker(activeMarker, 1.2f));
        activeMarker = null;
    }

    private IEnumerator FadeOutAndDestroyMarker(GameObject marker, float duration)
    {
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend == null)
        {
            Destroy(marker);
            yield break;
        }

        Material mat = rend.material;
        Color start = mat.HasProperty("_EmissionColor")
            ? mat.GetColor("_EmissionColor")
            : Color.white;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / duration);
            mat.SetColor("_EmissionColor", start * a);
            yield return null;
        }

        Destroy(marker);
    }

    private void StopMarkerFlash()
    {
        if (markerFlashRoutine != null)
        {
            StopCoroutine(markerFlashRoutine);
            markerFlashRoutine = null;
        }
    }

    // -------------------------------
    // MOVING CAR SPAWN SYSTEM
    // -------------------------------
    private IEnumerator SpawnCarsByTag(string tag, float baseInterval)
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            SpawnCarWithTag(tag);

            // Add randomness so intervals feel natural
            float randomOffset = Random.Range(-spawnIntervalVariance, spawnIntervalVariance);
            float actualInterval = Mathf.Max(0.1f, baseInterval + randomOffset);
            yield return new WaitForSeconds(actualInterval);
        }
    }

    private void SpawnCarWithTag(string tag)
    {
        if (movingCarPrefab == null || carSpawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager: Missing car prefab or spawn points.");
            return;
        }

        // Find all spawn points with this tag
        List<GameObject> taggedSpawns = new List<GameObject>();
        foreach (var sp in carSpawnPoints)
        {
            if (sp != null && sp.CompareTag(tag))
                taggedSpawns.Add(sp);
        }

        if (taggedSpawns.Count == 0)
            return;

        // Randomly pick one spawner with this tag
        GameObject chosenSpawner = taggedSpawns[Random.Range(0, taggedSpawns.Count)];
        GameObject newCar = Instantiate(movingCarPrefab, chosenSpawner.transform.position, chosenSpawner.transform.rotation);

        // Set car speed based on tag
        MovingCar carScript = newCar.GetComponent<MovingCar>();
        if (carScript != null)
        {
            carScript.speed = GetSpeedForTag(tag);
            Debug.Log($"Spawned {tag} car at {chosenSpawner.name} → Speed: {carScript.speed}");
        }
    }

    private float GetSpeedForTag(string tag)
    {
        switch (tag)
        {
            case "Slow": return slowCarSpeed;
            case "Medium": return mediumCarSpeed;
            case "Fast": return fastCarSpeed;
            default:
                Debug.LogWarning($"Spawner tag not recognized: {tag}, using medium speed.");
                return mediumCarSpeed;
        }
    }
}
