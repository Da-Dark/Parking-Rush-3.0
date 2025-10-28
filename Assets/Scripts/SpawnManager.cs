using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning")]
    public List<Transform> carSpawners;       // assign spawn points in inspector
    public GameObject movingCarPrefab;        // moving car prefab
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 1.5f;
    public float spawnRateIncrease = 0.1f;
    public bool enableCarSpawning = true;

    private float currentSpawnInterval;
    private Coroutine spawnRoutine;

    [Header("Open Spot Marker")]
    public GameObject flashMarkerPrefab;      // marker prefab (must have Renderer)
    private GameObject activeMarker;
    private Coroutine markerFlashRoutine;

    [Header("Parked Cars Discovery")]
    // collects parked cars from parents named "Enemy Cars for..."
    private List<GameObject> allParkedCars = new List<GameObject>();
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        CollectAllParkedCars();
        SpawnInitialOpenSpot();
        StartCarSpawner();
    }

    // Find all parked cars under parents whose name starts with "Enemy Cars for"
    private void CollectAllParkedCars()
    {
        allParkedCars.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Enemy Cars for"))
            {
                foreach (Transform child in obj.transform)
                {
                    if (child.CompareTag("ParkedCars"))
                        allParkedCars.Add(child.gameObject);
                }
            }
        }

        Debug.Log($"SpawnManager: found {allParkedCars.Count} parked cars across areas.");
    }

    private void SpawnInitialOpenSpot()
    {
        if (allParkedCars.Count == 0)
            CollectAllParkedCars();

        OpenSpot(); // immediately ensure an open spot exists
    }

    /// <summary>
    /// Called when player completes a level.
    /// Reactivates previous open spot, selects new spot (disables that parked car), spawns marker (level 1 only).
    /// </summary>
    public void OpenSpot()
    {
        // Re-enable previous spot so there's always only one open spot at a time
        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        // Build candidates from currently active parked cars
        List<GameObject> candidates = new List<GameObject>();
        foreach (var car in allParkedCars)
        {
            if (car != null && car.activeInHierarchy)
                candidates.Add(car);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SpawnManager: No active parked cars available to open a spot.");
            return;
        }

        // pick and deactivate
        currentOpenSpot = candidates[Random.Range(0, candidates.Count)];
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;
        Debug.Log($"SpawnManager: opened spot at {currentOpenSpot.name}");

        // only spawn a visible marker on Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            SpawnMarkerAt(currentOpenSpot.transform.position);
        }

        // speed up moving car spawn for next level
        IncreaseSpawnRate();
    }

    // spawn marker and start its marker flash coroutine
    private void SpawnMarkerAt(Vector3 worldPosition)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("SpawnManager: flashMarkerPrefab not assigned!");
            return;
        }

        if (activeMarker != null)
        {
            StopMarkerFlash();
            Destroy(activeMarker);
            activeMarker = null;
        }

        Vector3 spawnPos = worldPosition + Vector3.up * 0.5f;
        activeMarker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        Renderer rend = activeMarker.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("SpawnManager: flashMarkerPrefab has no Renderer!");
            return;
        }

        Material matInstance = new Material(rend.material); // unique instance
        matInstance.EnableKeyword("_EMISSION");
        rend.material = matInstance;

        markerFlashRoutine = StartCoroutine(MarkerFlashLoop(matInstance));
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

    private void StopMarkerFlash()
    {
        if (markerFlashRoutine != null)
        {
            StopCoroutine(markerFlashRoutine);
            markerFlashRoutine = null;
        }
    }

    /// <summary>
    /// Called by PlayerController on player's first movement input.
    /// Fades marker out smoothly and destroys it.
    /// </summary>
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
        Color start = mat.GetColor("_EmissionColor");
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

    // --- Moving car spawning system ---
    private void StartCarSpawner()
    {
        if (enableCarSpawning && spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (enableCarSpawning)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            SpawnRandomCar();
        }
    }

    private void SpawnRandomCar()
    {
        if (movingCarPrefab == null || carSpawners == null || carSpawners.Count == 0)
        {
            Debug.LogWarning("SpawnManager: missing movingCarPrefab or carSpawners.");
            return;
        }

        Transform sp = carSpawners[Random.Range(0, carSpawners.Count)];
        Instantiate(movingCarPrefab, sp.position, sp.rotation);
    }

    public void IncreaseSpawnRate()
    {
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnRateIncrease, minSpawnInterval);
        Debug.Log($"SpawnManager: spawn interval now {currentSpawnInterval:F2}s");
    }
}
