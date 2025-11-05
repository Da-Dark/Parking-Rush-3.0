using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning Settings")]
    [Tooltip("Assign all car spawner transforms here.")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;

    [Header("Spawn Intervals (seconds)")]
    public float slowSpawnInterval = 4f;
    public float mediumSpawnInterval = 2.5f;
    public float fastSpawnInterval = 1.2f;

    [Header("Car Speeds")]
    public float slowCarSpeed = 3f;
    public float mediumCarSpeed = 5f;
    public float fastCarSpeed = 8f;

    [Tooltip("Toggle car spawning on/off.")]
    public bool enableCarSpawning = true;

    private float baseSpawnInterval;
    private float baseCarSpeed;

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

    private Coroutine randomSpawnerRoutine;

    // -------------------------------
    // MAIN START
    // -------------------------------
    private void Start()
    {
        SetBaseSpawnAndSpeedByLevel();

        ClearExistingMarkers();
        CollectAllParkedCars();
        ActivateAllParkedCars();

        // Spawn an open spot for level start
        OpenSpot(LevelCounterManager.Instance == null || LevelCounterManager.Instance.GetCurrentLevel() == 1);

        if (enableCarSpawning)
            StartRandomSpawnerLoop();
    }

    // -------------------------------
    // LEVEL DIFFICULTY ADJUSTMENT
    // -------------------------------
    private void SetBaseSpawnAndSpeedByLevel()
    {
        int level = LevelCounterManager.Instance != null ? LevelCounterManager.Instance.GetCurrentLevel() : 1;

        if (level <= 3)
        {
            baseSpawnInterval = slowSpawnInterval;
            baseCarSpeed = slowCarSpeed;
        }
        else if (level <= 6)
        {
            baseSpawnInterval = mediumSpawnInterval;
            baseCarSpeed = mediumCarSpeed;
        }
        else
        {
            baseSpawnInterval = fastSpawnInterval;
            baseCarSpeed = fastCarSpeed;
        }

        Debug.Log($"SpawnManager: Level {level} uses base spawn interval {baseSpawnInterval}s and base speed {baseCarSpeed}");
    }

    // -------------------------------
    // PARKED CARS SYSTEM
    // -------------------------------
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
            if (car != null) car.SetActive(true);
    }

    private void ClearExistingMarkers()
    {
        var oldMarkers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (var marker in oldMarkers)
            Destroy(marker);
    }

    // -------------------------------
    // OPEN SPOT LOGIC
    // -------------------------------
    public void OpenSpot(bool showMarker = false)
    {
        if (levelOpenSpotSpawned) return;
        levelOpenSpotSpawned = true;

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

        List<GameObject> candidates = new List<GameObject>();
        foreach (var car in allParkedCars)
            if (car != null && car.activeInHierarchy)
                candidates.Add(car);

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SpawnManager: No active parked cars left!");
            return;
        }

        currentOpenSpot = candidates[Random.Range(0, candidates.Count)];
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;

        if (openSpotPrefab != null)
        {
            currentOpenSpotInstance = Instantiate(
                openSpotPrefab,
                currentOpenSpot.transform.position,
                currentOpenSpot.transform.rotation
            );
        }

        Debug.Log($"SpawnManager: Opened spot at {currentOpenSpot.name}");

        if (showMarker && flashMarkerPrefab != null)
            SpawnMarkerAt(currentOpenSpot.transform.position);
    }

    public void ResetLevelOpenFlag() => levelOpenSpotSpawned = false;

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
    // SIMPLIFIED RANDOM CAR SPAWNING
    // -------------------------------
    private void StartRandomSpawnerLoop()
    {
        if (randomSpawnerRoutine != null)
            StopCoroutine(randomSpawnerRoutine);

        randomSpawnerRoutine = StartCoroutine(RandomSpawnerRoutine());
    }

    private IEnumerator RandomSpawnerRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); // small delay before start

        while (enableCarSpawning)
        {
            if (carSpawners.Count == 0 || movingCarPrefab == null)
            {
                Debug.LogWarning("SpawnManager: Missing car prefab or spawners!");
                yield break;
            }

            // Pick a random spawner from the list
            Transform spawner = carSpawners[Random.Range(0, carSpawners.Count)];

            // Spawn a car there
            SpawnCarAt(spawner);

            // Wait a random amount between 0.8x–1.5x of its tag interval
            float waitTime = GetIntervalForTag(spawner.tag) * Random.Range(0.8f, 1.5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnCarAt(Transform spawner)
    {
        if (movingCarPrefab == null || spawner == null)
            return;

        GameObject car = Instantiate(movingCarPrefab, spawner.position, spawner.rotation);
        MovingCar carScript = car.GetComponent<MovingCar>();

        if (carScript != null)
            carScript.speed = GetSpeedForTag(spawner.tag);
    }

    private float GetIntervalForTag(string tag)
    {
        return tag switch
        {
            "Slow" => slowSpawnInterval,
            "Medium" => mediumSpawnInterval,
            "Fast" => fastSpawnInterval,
            _ => baseSpawnInterval
        };
    }

    private float GetSpeedForTag(string tag)
    {
        return tag switch
        {
            "Slow" => slowCarSpeed,
            "Medium" => mediumCarSpeed,
            "Fast" => fastCarSpeed,
            _ => baseCarSpeed
        };
    }
}
