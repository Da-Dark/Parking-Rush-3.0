using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 1.5f;
    public float spawnRateIncrease = 0.1f;
    public bool enableCarSpawning = true;

    [Header("Parked Cars & Open Spot")]
    public GameObject glowMarkerPrefab;
    private List<GameObject> allParkedCars = new List<GameObject>();
    private GameObject currentOpenSpotMarker;
    private GameObject currentOpenCar;

    private float currentSpawnInterval;
    private bool openSpotActive = false;

    private void Awake()
    {
        currentSpawnInterval = initialSpawnInterval;
    }

    private IEnumerator Start()
    {
        // Wait one frame to ensure other managers (like LevelCounterManager) are initialized
        yield return null;

        // Collect all parked cars in this scene
        CollectAllParkedCars();

        // Activate them
        foreach (var car in allParkedCars)
        {
            if (car != null)
                car.SetActive(true);
        }

        // Create one open spot
        OpenSingleSpot();

        // Start car spawning if enabled
        if (enableCarSpawning)
            StartCoroutine(SpawnCarsRoutine());
    }

    private void CollectAllParkedCars()
    {
        allParkedCars.Clear();
        GameObject[] parkedCars = GameObject.FindGameObjectsWithTag("ParkedCars");
        allParkedCars.AddRange(parkedCars);

        Debug.Log($"SpawnManager: Collected {allParkedCars.Count} parked cars in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
    }

    public void OpenSingleSpot()
    {
        if (allParkedCars.Count == 0)
        {
            Debug.LogWarning("SpawnManager: No active parked cars found!");
            return;
        }

        // Pick one random parked car to remove
        int randomIndex = Random.Range(0, allParkedCars.Count);
        GameObject selectedCar = allParkedCars[randomIndex];

        if (selectedCar == null)
        {
            Debug.LogWarning("SpawnManager: Selected parked car was null!");
            return;
        }

        currentOpenCar = selectedCar;
        allParkedCars.Remove(selectedCar);

        // Hide the parked car to create an open spot
        selectedCar.SetActive(false);
        openSpotActive = true;

        Debug.Log($"SpawnManager: Created open spot at {selectedCar.transform.position}");

        // Only show glow marker if on Level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            ShowGlowMarker(selectedCar.transform.position);
        }
    }

    private void ShowGlowMarker(Vector3 position)
    {
        if (glowMarkerPrefab == null)
        {
            Debug.LogWarning("SpawnManager: Glow marker prefab not assigned!");
            return;
        }

        currentOpenSpotMarker = Instantiate(glowMarkerPrefab, position + Vector3.up * 1.5f, Quaternion.identity);
    }

    public void HideGlowMarker()
    {
        if (currentOpenSpotMarker != null)
        {
            Destroy(currentOpenSpotMarker);
            currentOpenSpotMarker = null;
        }
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (enableCarSpawning)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            SpawnMovingCar();

            currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnRateIncrease, minSpawnInterval);
        }
    }

    private void SpawnMovingCar()
    {
        if (carSpawners.Count == 0 || movingCarPrefab == null)
            return;

        Transform spawnPoint = carSpawners[Random.Range(0, carSpawners.Count)];
        Instantiate(movingCarPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void ClearExistingMarkers()
    {
        foreach (var marker in GameObject.FindGameObjectsWithTag("GlowMarker"))
        {
            Destroy(marker);
        }
    }
}
