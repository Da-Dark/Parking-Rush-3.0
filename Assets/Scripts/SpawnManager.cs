using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] parkedCars;       // All parked cars
    public GameObject[] CarInitialPos;    // Moving car spawn positions
    public GameObject MovingRedCar;
    public float RespawnTime = 10f;

    [Header("Glow Marker")]
    public GameObject glowMarkerPrefab;
    public Vector3 markerOffset = new Vector3(0, 2f, 0);

    private GameObject activeGlowMarker;
    private int lastOpenIndex = -1;

    void Start()
    {
        // If parkedCars already assigned, open spot immediately
        if (parkedCars != null && parkedCars.Length > 0)
        {
            OpenSpot();
        }
        else
        {
            // Otherwise wait until parkedCars are populated
            StartCoroutine(InitializeAfterDelay());
        }

        InvokeRepeating("SpawnMovingCar", 0f, RespawnTime);
    }

    private IEnumerator InitializeAfterDelay()
    {
        while (parkedCars == null || parkedCars.Length == 0)
        {
            parkedCars = GameObject.FindGameObjectsWithTag("ParkedCars");
            yield return new WaitForSeconds(0.1f);
        }

        OpenSpot();
    }

    public void OpenSpot()
    {
        if (parkedCars == null || parkedCars.Length == 0) return;

        // Reactivate all cars
        foreach (var car in parkedCars)
            car.SetActive(true);

        // Pick a new spot different from last
        int newIndex;
        do
        {
            newIndex = Random.Range(0, parkedCars.Length);
        } while (newIndex == lastOpenIndex && parkedCars.Length > 1);

        parkedCars[newIndex].SetActive(false);
        lastOpenIndex = newIndex;

        // Move glow marker above new spot
        MoveGlowMarkerToSpot(parkedCars[newIndex]);
    }

    private void MoveGlowMarkerToSpot(GameObject spot)
    {
        if (glowMarkerPrefab == null) return;

        Vector3 pos = spot.transform.position + markerOffset;

        if (activeGlowMarker == null)
            activeGlowMarker = Instantiate(glowMarkerPrefab, pos, Quaternion.identity);
        else
        {
            activeGlowMarker.transform.position = pos;
            activeGlowMarker.SetActive(true);
        }
    }

    public void HideGlowMarker()
    {
        if (activeGlowMarker != null)
            activeGlowMarker.SetActive(false);
    }

    public void SpawnMovingCar()
    {
        if (CarInitialPos == null || CarInitialPos.Length == 0 || MovingRedCar == null) return;

        int spawnIndex = Random.Range(0, CarInitialPos.Length);
        Instantiate(MovingRedCar,
            CarInitialPos[spawnIndex].transform.position,
            CarInitialPos[spawnIndex].transform.rotation);
    }
}
