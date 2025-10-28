using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parked cars discovery")]
    public string parentNamePrefix = "Enemy Cars for"; // parents that hold parked cars
    public string parkedTag = "ParkedCars";

    [Header("Marker (Level 1 only)")]
    public GameObject flashMarkerPrefab; // prefab must have a Renderer (quad/plane/cylinder)
    private GameObject activeMarker = null;

    [Header("Moving car spawning")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;
    public float initialSpawnInterval = 4f;
    public float spawnRateIncrease = 0.1f;
    public float minSpawnInterval = 1.0f;

    // internal
    private List<GameObject> allParkedCars = new List<GameObject>();
    private GameObject currentOpenSpot = null;   // disabled GameObject used as the "open spot"
    private GameObject previousOpenSpot = null;
    private float currentSpawnInterval;
    private Coroutine spawnRoutine;

    void Awake()
    {
        currentSpawnInterval = initialSpawnInterval;
    }

    void Start()
    {
        CollectAllParkedCars();
        EnsureAllParkedCarsActive();
        CreateSingleOpenSpotAtStart();
        StartCarSpawner();
    }

    // collect parked cars under parents that start with parentNamePrefix
    private void CollectAllParkedCars()
    {
        allParkedCars.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith(parentNamePrefix))
            {
                foreach (Transform child in obj.transform)
                {
                    if (child != null && child.CompareTag(parkedTag))
                        allParkedCars.Add(child.gameObject);
                }
            }
        }

        Debug.Log($"SpawnManager: Collected parkedCars count = {allParkedCars.Count}");
    }

    // defensive: ensure none of the parked cars are accidentally disabled in the scene
    private void EnsureAllParkedCarsActive()
    {
        int revived = 0;
        foreach (var car in allParkedCars)
        {
            if (car == null) continue;
            if (!car.activeInHierarchy)
            {
                car.SetActive(true);
                revived++;
            }
        }
        if (revived > 0)
            Debug.Log($"SpawnManager: Reactivated {revived} parked cars at startup.");
    }

    // Called once at scene start to ensure exactly one open spot exists
    private void CreateSingleOpenSpotAtStart()
    {
        // Clear any active marker left in the scene (defensive)
        DestroyActiveMarkerImmediate();

        OpenSpotImmediate();
    }

    // PUBLIC: call this to create the next open spot (called after level success)
    public void OpenSpot()
    {
        // When moving to next level we should clean up marker immediately (player moved earlier)
        DestroyActiveMarkerImmediate();

        // reselect and open another spot
        OpenSpotImmediate();
    }

    // Core routine: re-activate all parked cars, re-enable previous if needed, choose one to deactivate
    private void OpenSpotImmediate()
    {
        if (allParkedCars.Count == 0)
            CollectAllParkedCars();

        // Make sure all are active before selecting
        EnsureAllParkedCarsActive();

        // Re-enable the previousOpenSpot (if any) so there's only one open at a time
        if (previousOpenSpot != null)
        {
            if (!previousOpenSpot.activeInHierarchy)
                previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        // Build candidate list (active parked cars only)
        List<GameObject> candidates = new List<GameObject>();
        foreach (var car in allParkedCars)
        {
            if (car != null && car.activeInHierarchy)
                candidates.Add(car);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("SpawnManager.OpenSpotImmediate: no active parked cars found.");
            return;
        }

        // Pick one at random and deactivate it -> becomes the open spot
        int idx = Random.Range(0, candidates.Count);
        currentOpenSpot = candidates[idx];
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot; // store so next OpenSpot() can re-enable it
        Debug.Log($"SpawnManager: opened spot -> {currentOpenSpot.name} (idx {idx})");

        // Spawn marker only on Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            SpawnMarkerAt(currentOpenSpot.transform.position);
        }
        else
        {
            // ensure no marker exists if not level 1
            DestroyActiveMarkerImmediate();
        }

        // increase spawn rate slightly
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnRateIncrease, minSpawnInterval);
    }

    // Marker: spawn, pulse, fade/destroy on HideGlowMarker
    private void SpawnMarkerAt(Vector3 worldPosition)
    {
        DestroyActiveMarkerImmediate();

        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("SpawnManager.SpawnMarkerAt: flashMarkerPrefab not assigned");
            return;
        }

        Vector3 spawnPos = worldPosition + Vector3.up * 0.5f;
        activeMarker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        Renderer rend = activeMarker.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.material);
            mat.EnableKeyword("_EMISSION");
            rend.material = mat;
            StartCoroutine(MarkerPulse(mat));
        }
    }

    private IEnumerator MarkerPulse(Material mat)
    {
        float speed = 2f;
        float intensity = 1.2f;
        while (activeMarker != null)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            Color c = Color.Lerp(Color.yellow, Color.white, t);
            if (mat != null) mat.SetColor("_EmissionColor", c * intensity);
            yield return null;
        }
    }

    // Called by PlayerController on first input to fade out the current marker (if any)
    public void HideGlowMarker()
    {
        if (activeMarker != null)
        {
            StartCoroutine(FadeOutAndDestroyMarker(activeMarker, 1.2f));
            activeMarker = null; // mark as gone so pulse coroutine ends
        }
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
        Color start = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.white;
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

    // immediate destroy (no fade)
    private void DestroyActiveMarkerImmediate()
    {
        if (activeMarker != null)
        {
            var toDestroy = activeMarker;
            activeMarker = null;
            Destroy(toDestroy);
        }
    }

    // --- moving car spawning system ---
    private void StartCarSpawner()
    {
        if (movingCarPrefab == null || carSpawners == null || carSpawners.Count == 0)
        {
            Debug.Log("SpawnManager: moving cars disabled (no prefab/spawners).");
            return;
        }

        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            if (movingCarPrefab != null && carSpawners != null && carSpawners.Count > 0)
            {
                Transform sp = carSpawners[Random.Range(0, carSpawners.Count)];
                Instantiate(movingCarPrefab, sp.position, sp.rotation);
            }
        }
    }
}
