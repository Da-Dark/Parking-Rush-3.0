using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parking Spots & Cars")]
    public List<GameObject> parkedCars = new List<GameObject>();
    private GameObject currentOpenSpot;

    [Header("Visuals")]
    public GameObject glowMarkerPrefab;
    private GameObject currentGlowMarker;

    [Header("Delays")]
    public float respawnDelay = 0.5f;

    private void Start()
    {
        // Initialize after short delay to allow scene to load
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshParkedCars();
        OpenSpot();
    }

    /// <summary>
    /// Finds all parked cars currently active in the scene.
    /// </summary>
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

        Debug.Log($"🚗 Refreshed parkedCars list: {parkedCars.Count} cars found.");
    }

    /// <summary>
    /// Selects a random car to despawn and marks that as the new open parking spot.
    /// </summary>
    public void OpenSpot()
    {
        RefreshParkedCars();

        if (parkedCars.Count == 0)
        {
            Debug.LogWarning("⚠️ No parked cars found — cannot create open spot.");
            return;
        }

        // Pick a random parked car
        int index = Random.Range(0, parkedCars.Count);
        currentOpenSpot = parkedCars[index];

        if (currentOpenSpot == null)
        {
            Debug.LogWarning("⚠️ Selected parked car is null!");
            return;
        }

        // Remove that car to open the space
        currentOpenSpot.SetActive(false);

        Debug.Log($"🅿️ New open parking spot: {currentOpenSpot.name}");

        // Spawn glow marker above it
        SpawnGlowMarker(currentOpenSpot.transform.position);
    }

    /// <summary>
    /// Spawns the glow marker directly above the open parking spot.
    /// </summary>
    private void SpawnGlowMarker(Vector3 position)
    {
        if (glowMarkerPrefab == null)
        {
            Debug.LogWarning("⚠️ Glow Marker Prefab not assigned in SpawnManager!");
            return;
        }

        // Remove any previous marker
        if (currentGlowMarker != null)
            Destroy(currentGlowMarker);

        // Spawn new one slightly above the open spot
        Vector3 markerPos = position + Vector3.up * 1.5f;
        currentGlowMarker = Instantiate(glowMarkerPrefab, markerPos, Quaternion.identity);
    }

    /// <summary>
    /// Called when player makes their first move.
    /// </summary>
    public void HideGlowMarker()
    {
        if (currentGlowMarker != null)
        {
            Destroy(currentGlowMarker);
            currentGlowMarker = null;
        }
    }
}
