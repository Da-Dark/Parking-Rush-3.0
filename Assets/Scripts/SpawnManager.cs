using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Open Spot Marker")]
    public GameObject flashMarkerPrefab;
    private GameObject activeMarker;
    private Coroutine markerFlashRoutine;

    [Header("Parked Cars Discovery")]
    private List<GameObject> allParkedCars = new List<GameObject>();
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;

        // Ensure no leftover markers exist from editor play mode
        ClearExistingMarkers();

        // Ensure all parked cars are active before we pick one
        CollectAllParkedCars();
        foreach (var car in allParkedCars)
        {
            if (car != null)
                car.SetActive(true);
        }

        // Open a single spot to begin
        OpenSpot();

        // Optionally start spawning moving cars
        if (enableCarSpawning)
            StartCarSpawner();
    }

    private void ClearExistingMarkers()
    {
        // Destroy all pre-placed markers in the scene
        var oldMarkers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (var marker in oldMarkers)
        {
            Destroy(marker);
        }
    }

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

        Debug.Log($"SpawnManager: Found {allParkedCars.Count} parked cars.");
    }

    public void OpenSpot()
    {
        // Reactivate the previous spot
        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        // Destroy any old markers before creating a new one
        if (activeMarker != null)
        {
            StopMarkerFlash();
            Destroy(activeMarker);
            activeMarker = null;
        }

        // Build a list of active parked cars to pick from
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

        // Choose one randomly and deactivate it (making it the open spot)
        currentOpenSpot = candidates[Random.Range(0, candidates.Count)];
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;

        Debug.Log($"SpawnManager: Opened spot at {currentOpenSpot.name}");

        // Only show flashing marker on Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            SpawnMarkerAt(currentOpenSpot.transform.position);
        }

        IncreaseSpawnRate();
    }

    private void SpawnMarkerAt(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("SpawnManager: flashMarkerPrefab not assigned!");
            return;
        }

        // Spawn above the open spot
        Vector3 spawnPos = position + Vector3.up * 0.5f;
        activeMarker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);
        activeMarker.tag = "Marker"; // Helps cleanup system

        Renderer rend = activeMarker.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("SpawnManager: flashMarkerPrefab missing Renderer!");
            return;
        }

        Material matInstance = new Material(rend.material);
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

    // === Moving Car Spawn System ===
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
