using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parking Spots & Cars")]
    public List<GameObject> parkedCars = new List<GameObject>(); // cars tagged "ParkedCars"
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    [Header("Flasher Settings")]
    public GameObject flashMarkerPrefab; // prefab with OpenSpotFlasherWithMarker attached
    public float markerOffsetY = 0.5f;

    [Header("Moving Cars")]
    public GameObject movingRedCarPrefab;
    public Transform[] movingCarSpawners;
    public float movingCarSpawnTime = 5f;

    private void Start()
    {
        StartCoroutine(InitializeAfterDelay());
        InvokeRepeating(nameof(SpawnMovingCar), movingCarSpawnTime, movingCarSpawnTime);
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshParkedCars();  // Build the full list of parked cars
        OpenSpot();           // Open the first spot
    }

    /// <summary>
    /// Refresh the list of active parked cars in the scene
    /// </summary>
    private void RefreshParkedCars()
    {
        parkedCars.Clear();
        GameObject[] cars = GameObject.FindGameObjectsWithTag("ParkedCars");
        foreach (GameObject car in cars)
        {
            if (car.activeInHierarchy)
                parkedCars.Add(car);
        }

        Debug.Log($"🚗 Refreshed parkedCars list: {parkedCars.Count} active cars.");
    }

    /// <summary>
    /// Opens a new parking spot and re-enables previous one.
    /// </summary>
    public void OpenSpot()
    {
        RefreshParkedCars(); // always make sure we have the current parked cars

        // Re-enable the previous open spot
        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
        }

        if (parkedCars.Count == 0)
        {
            Debug.LogWarning("⚠️ No parked cars found — cannot open a new spot.");
            return;
        }

        // Pick a random car to open
        GameObject[] candidates = parkedCars.ToArray();
        int index = Random.Range(0, candidates.Length);
        currentOpenSpot = candidates[index];

        if (currentOpenSpot == null)
        {
            Debug.LogWarning("⚠️ Selected parked car is null!");
            return;
        }

        // Deactivate it to create an open spot
        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;

        Debug.Log($"🅿️ New open parking spot: {currentOpenSpot.name}");

        // Spawn flashing marker only on level 1
        if (LevelCounterManager.Instance != null && LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            SpawnFlashingMarker(currentOpenSpot.transform.position);
        }
    }

    /// <summary>
    /// Spawn a marker above the open spot
    /// </summary>
    private void SpawnFlashingMarker(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("⚠️ flashMarkerPrefab not assigned in SpawnManager.");
            return;
        }

        Vector3 spawnPos = position + Vector3.up * markerOffsetY;
        GameObject marker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        // Attach flasher script if it exists
        OpenSpotFlasherWithMarker flasher = marker.GetComponent<OpenSpotFlasherWithMarker>();
        if (flasher == null)
        {
            Debug.LogWarning("⚠️ flashMarkerPrefab does not have OpenSpotFlasherWithMarker attached!");
        }
    }

    /// <summary>
    /// Stop flashing when player makes first input
    /// </summary>
    public void HideGlowMarker()
    {
        OpenSpotFlasherWithMarker flasher = FindObjectOfType<OpenSpotFlasherWithMarker>();
        if (flasher != null)
        {
            flasher.StopFlashing();
        }
    }

    /// <summary>
    /// Spawn moving cars
    /// </summary>
    private void SpawnMovingCar()
    {
        if (movingRedCarPrefab == null || movingCarSpawners.Length == 0)
            return;

        int index = Random.Range(0, movingCarSpawners.Length);
        Transform spawnPoint = movingCarSpawners[index];
        Instantiate(movingRedCarPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
