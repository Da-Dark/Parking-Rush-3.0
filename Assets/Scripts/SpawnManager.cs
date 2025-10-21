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
    public GameObject flashMarkerPrefab; // assign prefab that now has OpenSpotFlasherWithMarker attached
    public float markerHeight = 0.5f;
    public float respawnDelay = 0.5f;

    private bool firstLevelFlasherSpawned = false;

    private void Start()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshParkedCars();
        OpenSpot(); // pick initial open spot
    }

    private void RefreshParkedCars()
    {
        parkedCars.Clear();
        foreach (GameObject car in GameObject.FindGameObjectsWithTag("ParkedCars"))
        {
            if (car.activeInHierarchy)
                parkedCars.Add(car);
        }
    }

    public void OpenSpot()
    {
        RefreshParkedCars();

        // Re-enable previous spot
        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        if (parkedCars.Count == 0)
        {
            Debug.LogWarning("⚠️ No parked cars found — cannot open a new spot.");
            return;
        }

        int index = Random.Range(0, parkedCars.Count);
        currentOpenSpot = parkedCars[index];

        if (currentOpenSpot == null)
        {
            Debug.LogWarning("⚠️ Selected parked car is null!");
            return;
        }

        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;

        // Only spawn flashing marker on Level 1
        if (!firstLevelFlasherSpawned &&
            LevelCounterManager.Instance != null &&
            LevelCounterManager.Instance.GetCurrentLevel() == 1)
        {
            SpawnFlashingMarker(currentOpenSpot.transform.position);
            firstLevelFlasherSpawned = true;
        }
    }

    private void SpawnFlashingMarker(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("⚠️ No flashMarkerPrefab assigned in SpawnManager.");
            return;
        }

        Vector3 spawnPos = position + Vector3.up * markerHeight;
        GameObject marker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        OpenSpotFlasherWithMarker flasher = marker.GetComponent<OpenSpotFlasherWithMarker>();
        if (flasher != null)
        {
            flasher.shouldStopOnFirstInput = true;
        }
        else
        {
            Debug.LogWarning("⚠️ flashMarkerPrefab does not have OpenSpotFlasherWithMarker attached!");
        }
    }
}
