using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parking Spots & Cars")]
    public List<GameObject> parkedCars = new List<GameObject>(); // Cars tagged "ParkedCars"
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    [Header("Flasher Settings")]
    public GameObject flashMarkerPrefab; // Prefab with OpenSpotFlasherWithMarker attached
    public float respawnDelay = 0.5f;

    [Header("Flash Control")]
    public List<OpenSpotFlasherWithMarker> activeFlashMarkers = new List<OpenSpotFlasherWithMarker>();
    public bool firstLevelFlashDone = false; // Only flash in level 1

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

        // Re-enable previous open spot
        if (previousOpenSpot != null)
            previousOpenSpot.SetActive(true);

        if (parkedCars.Count == 0)
        {
            Debug.LogWarning("No parked cars found — cannot open a new spot.");
            return;
        }

        int index = Random.Range(0, parkedCars.Count);
        currentOpenSpot = parkedCars[index];
        if (currentOpenSpot == null)
        {
            Debug.LogWarning("Selected parked car is null!");
            return;
        }

        currentOpenSpot.SetActive(false); // Open spot
        previousOpenSpot = currentOpenSpot;

        // Only flash on level 1
        if (!firstLevelFlashDone)
        {
            SpawnFlashingMarker(currentOpenSpot.transform.position);
        }
    }

    private void SpawnFlashingMarker(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("No flashMarkerPrefab assigned!");
            return;
        }

        GameObject marker = Instantiate(flashMarkerPrefab, position + Vector3.up * 0.5f, Quaternion.identity);
        OpenSpotFlasherWithMarker flasher = marker.GetComponent<OpenSpotFlasherWithMarker>();
        if (flasher != null)
        {
            activeFlashMarkers.Add(flasher);
        }
        else
        {
            Debug.LogWarning("flashMarkerPrefab does not have OpenSpotFlasherWithMarker attached!");
        }
    }

    // Called by PlayerController on first input
    public void OnPlayerFirstInput()
    {
        foreach (var flasher in activeFlashMarkers)
        {
            if (flasher != null)
                flasher.StopFlashing();
        }

        activeFlashMarkers.Clear();
        firstLevelFlashDone = true;
    }
}
