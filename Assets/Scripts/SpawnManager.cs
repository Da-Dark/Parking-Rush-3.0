using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Parking Spots")]
    public List<GameObject> parkedCars = new List<GameObject>();
    public GameObject openSpotPrefab;
    public Transform[] parkingSpots;
    private GameObject currentOpenSpot;
    private bool hasShownMarkerThisMap = false;
    private bool hasOpenedSpotThisLevel = false;

    [Header("Glow Marker")]
    public GameObject glowMarkerPrefab;
    private GameObject activeGlowMarker;

    [Header("Car Spawning")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 1.5f;
    public float spawnRateIncrease = 0.1f;
    public bool enableCarSpawning = true;

    private float currentSpawnInterval;
    private bool isSpawningCars = true;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;

        // Always open one spot when the first map starts
        hasShownMarkerThisMap = false;
        hasOpenedSpotThisLevel = false;
        OpenSingleSpot(true);

        // Start car spawning if enabled
        if (enableCarSpawning)
            StartCoroutine(SpawnCarsRoutine());
    }

    // Called when new map loads
    public void ResetLevelOpenFlag()
    {
        hasShownMarkerThisMap = false;
        hasOpenedSpotThisLevel = false;
    }

    // Core function that opens exactly one open spot
    public void OpenSingleSpot(bool isInitial)
    {
        if (hasOpenedSpotThisLevel)
            return;

        List<GameObject> activeParkedCars = new List<GameObject>();
        foreach (GameObject car in parkedCars)
        {
            if (car != null && car.activeInHierarchy)
                activeParkedCars.Add(car);
        }

        if (activeParkedCars.Count == 0)
        {
            Debug.LogWarning("SpawnManager: No active parked cars left!");
            return;
        }

        int randomIndex = Random.Range(0, activeParkedCars.Count);
        GameObject selectedCar = activeParkedCars[randomIndex];

        // Remove that parked car to make an open spot
        selectedCar.SetActive(false);

        Vector3 spotPos = selectedCar.transform.position;
        Quaternion spotRot = selectedCar.transform.rotation;

        currentOpenSpot = Instantiate(openSpotPrefab, spotPos, spotRot);
        hasOpenedSpotThisLevel = true;

        // Only show glow marker for first open spot on each map
        if (!hasShownMarkerThisMap)
        {
            ShowGlowMarker(spotPos);
            hasShownMarkerThisMap = true;
        }
    }

    public void HideGlowMarker()
    {
        if (activeGlowMarker != null)
        {
            Destroy(activeGlowMarker);
            activeGlowMarker = null;
        }
    }

    private void ShowGlowMarker(Vector3 pos)
    {
        if (glowMarkerPrefab == null) return;
        activeGlowMarker = Instantiate(glowMarkerPrefab, pos + Vector3.up * 2f, Quaternion.identity);
    }

    // Reset open-spot flag each level
    public void OpenSpot(bool isInitial)
    {
        hasOpenedSpotThisLevel = false;
        OpenSingleSpot(isInitial);
    }

    // -------------------------------------
    // Car Spawning System
    // -------------------------------------
    private IEnumerator SpawnCarsRoutine()
    {
        while (isSpawningCars)
        {
            SpawnRandomCar();
            yield return new WaitForSeconds(currentSpawnInterval);
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - spawnRateIncrease);
        }
    }

    private void SpawnRandomCar()
    {
        if (movingCarPrefab == null || carSpawners.Count == 0)
        {
            Debug.LogWarning("SpawnManager: Missing moving car prefab or spawner points.");
            return;
        }

        int randomIndex = Random.Range(0, carSpawners.Count);
        Transform spawnPoint = carSpawners[randomIndex];
        GameObject car = Instantiate(movingCarPrefab, spawnPoint.position, spawnPoint.rotation);

        if (car == null)
        {
            Debug.LogWarning("SpawnManager: Car instantiation failed (prefab or component missing).");
            return;
        }
    }
}
