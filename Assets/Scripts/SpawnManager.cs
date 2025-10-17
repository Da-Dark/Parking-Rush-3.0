using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parking Spots & Cars")]
    public List<GameObject> parkedCars = new List<GameObject>();
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    [Header("Flasher Settings")]
    public GameObject flashMarkerPrefab;
    public float respawnDelay = 0.5f;

    private void Start()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshParkedCars();
        OpenSpot();
    }

    private void RefreshParkedCars()
    {
        parkedCars.Clear();
        foreach (GameObject car in GameObject.FindGameObjectsWithTag("ParkedCars"))
        {
            if (car.activeInHierarchy)
            {
                parkedCars.Add(car);
            }
        }

        Debug.Log($"🚗 Refreshed parkedCars list: {parkedCars.Count} active cars found.");
    }

    public void OpenSpot()
    {
        RefreshParkedCars();

        // Re-enable previous open spot
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
        Debug.Log($"🅿️ New open parking spot: {currentOpenSpot.name}");

        SpawnFlashingMarker(currentOpenSpot.transform.position);

        previousOpenSpot = currentOpenSpot;
    }

    private void SpawnFlashingMarker(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("⚠️ No flashMarkerPrefab assigned in SpawnManager.");
            return;
        }

        Vector3 spawnPos = position + Vector3.up * 0.5f;
        GameObject marker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        OpenSpotFlasherWithMarker flasher = marker.GetComponent<OpenSpotFlasherWithMarker>();
        if (flasher != null)
        {
            Debug.Log("✨ Flashing marker spawned successfully.");
        }
        else
        {
            Debug.LogWarning("⚠️ The flashMarkerPrefab does not have OpenSpotFlasherWithMarker attached!");
        }
    }

    public void HideGlowMarker()
    {
        OpenSpotFlasherWithMarker flasher = FindObjectOfType<OpenSpotFlasherWithMarker>();
        if (flasher != null)
        {
            flasher.StopFlashing();
        }
    }
}
