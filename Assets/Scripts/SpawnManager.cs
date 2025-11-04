using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 1.5f;
    public float spawnRateIncrease = 0.1f;
    public bool enableCarSpawning = true;

    private float currentSpawnInterval;
    private Coroutine spawnRoutine;

    [Header("Open Spot System")]
    public GameObject openSpotPrefab;
    public GameObject flashMarkerPrefab;

    private GameObject activeMarker;
    private Coroutine markerFlashRoutine;
    private GameObject currentOpenSpotInstance;

    private List<GameObject> allParkedCars = new List<GameObject>();
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;
    private bool levelOpenSpotSpawned = false;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;

        ClearExistingMarkers();
        CollectAllParkedCars();
        ActivateAllParkedCars();

        // Spawn initial open spot on map start
        OpenSpot(LevelCounterManager.Instance == null || LevelCounterManager.Instance.GetCurrentLevel() == 1);

        // Start car spawner
        if (enableCarSpawning)
            StartCarSpawner();
    }

    // --- Parked Cars Setup ---
    private void CollectAllParkedCars()
    {
        allParkedCars.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("ParkedCars"))
                allParkedCars.Add(obj);
        }

        Debug.Log($"SpawnManager: Found {allParkedCars.Count} parked cars.");
    }

    private void ActivateAllParkedCars()
    {
        foreach (var car in allParkedCars)
        {
            if (car != null)
                car.SetActive(true);
        }
    }

    private void ClearExistingMarkers()
    {
        var oldMarkers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (var marker in oldMarkers)
            Destroy(marker);
    }

    // --- Open Spot Logic ---
    public void OpenSpot(bool showMarker = false)
    {
        if (levelOpenSpotSpawned) return; // Prevent multiple spawns in one level
        levelOpenSpotSpawned = true;

        // Destroy the previous open spot and marker
        if (currentOpenSpotInstance != null)
        {
            Destroy(currentOpenSpotInstance);
            currentOpenSpotInstance = null;
        }

        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        // Pick one parked car to disable (making the open spot)
        List<GameObject> candidates = new List<GameObject>();
        foreach (var car in allParkedCars)
        {
            if (car != null && car.activeInHierarchy)
                candidates.Add(car);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SpawnManager: No active parked cars left!");
            return;
        }

        currentOpenSpot = candidates[Random.Range(0, candidates.Count)];
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;

        // Spawn the open spot prefab (success trigger)
        if (openSpotPrefab != null)
        {
            currentOpenSpotInstance = Instantiate(
                openSpotPrefab,
                currentOpenSpot.transform.position,
                currentOpenSpot.transform.rotation
            );
        }

        Debug.Log($"SpawnManager: Opened spot at {currentOpenSpot.name}");

        // Show flashing marker only if requested
        if (showMarker && flashMarkerPrefab != null)
        {
            SpawnMarkerAt(currentOpenSpot.transform.position);
        }

        IncreaseSpawnRate();
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

    // --- Moving Car System ---
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
            Debug.LogWarning("SpawnManager: Missing movingCarPrefab or spawners.");
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
