using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Car Spawning")]
    public List<Transform> carSpawners;
    public GameObject movingCarPrefab;
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 1.5f;
    public float spawnRateIncrease = 0.1f;
    public bool enableCarSpawning = true;

    private float currentSpawnInterval;
    private Coroutine spawnRoutine;

    [Header("Open Spot Marker")]
    public GameObject flashMarkerPrefab;
    private GameObject activeMarker;
    private GameObject currentOpenSpot;
    private GameObject previousOpenSpot;

    private List<GameObject> allParkedCars = new List<GameObject>();

    [Header("Level Tracking")]
    public int currentLevel = 1; // You can hook this up from your GameManager later

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        CollectAllParkedCars();
        OpenSpot();
        StartCarSpawner();
    }

    private void CollectAllParkedCars()
    {
        allParkedCars.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Enemy Cars for"))
            {
                foreach (Transform child in obj.transform)
                {
                    if (child.CompareTag("ParkedCars"))
                        allParkedCars.Add(child.gameObject);
                }
            }
        }

        Debug.Log($"🅿️ Found {allParkedCars.Count} parked cars across all areas.");
    }

    public void OpenSpot()
    {
        // Reactivate previous open spot
        if (previousOpenSpot != null)
        {
            previousOpenSpot.SetActive(true);
            previousOpenSpot = null;
        }

        if (allParkedCars.Count == 0)
        {
            Debug.LogWarning("⚠️ No parked cars found!");
            return;
        }

        // Pick random parked car to deactivate
        int index = Random.Range(0, allParkedCars.Count);
        currentOpenSpot = allParkedCars[index];

        if (currentOpenSpot == null)
        {
            Debug.LogWarning("⚠️ Selected car is null!");
            return;
        }

        currentOpenSpot.SetActive(false);
        previousOpenSpot = currentOpenSpot;
        Debug.Log($"🅿️ Open parking spot created at: {currentOpenSpot.name}");

        // 🔆 Only spawn marker if this is level 1
        if (currentLevel == 1)
            SpawnFlashingMarker(currentOpenSpot.transform.position);

        IncreaseSpawnRate();
    }

    private void SpawnFlashingMarker(Vector3 position)
    {
        if (flashMarkerPrefab == null)
        {
            Debug.LogWarning("⚠️ No flash marker prefab assigned!");
            return;
        }

        if (activeMarker != null)
            Destroy(activeMarker);

        Vector3 spawnPos = position + Vector3.up * 0.5f;
        activeMarker = Instantiate(flashMarkerPrefab, spawnPos, Quaternion.identity);

        //sets trigger to open flsher marker to time based rahter than player positioned ba
        //StartCoroutine(FlashAndFadeMarker(activeMarker, 3f));
    }

    private IEnumerator FlashAndFadeMarker(GameObject marker, float duration)
    {
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend == null) yield break;

        Material mat = rend.material;
        float elapsed = 0f;
        float colorCycleSpeed = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 🔄 Cycle color through red, green, blue, back to red
            float t = Mathf.PingPong(elapsed * colorCycleSpeed, 1f);
            Color glowColor = Color.Lerp(Color.red, Color.blue, t);

            // 🔅 Fade out alpha over time
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            mat.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);

            yield return null;
        }

        Destroy(marker);
    }

    public void HideGlowMarker()
    {
        if (activeMarker != null)
        {
            StopAllCoroutines(); // Stop flashing coroutine if running
            StartCoroutine(FadeOutAndDestroy(activeMarker, 1.5f));
            activeMarker = null;
        }
    }

    private IEnumerator FadeOutAndDestroy(GameObject marker, float duration)
    {
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = rend.material;
            Color initialColor = mat.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                mat.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }
        }

        Destroy(marker);
    }

    private void StartCarSpawner()
    {
        if (enableCarSpawning && spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (enableCarSpawning)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            SpawnRandomCar();
        }
    }

    private void SpawnRandomCar()
    {
        if (movingCarPrefab == null || carSpawners.Count == 0)
        {
            Debug.LogWarning("⚠️ Missing car spawners or prefab!");
            return;
        }

        Transform spawnPoint = carSpawners[Random.Range(0, carSpawners.Count)];
        Instantiate(movingCarPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void IncreaseSpawnRate()
    {
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnRateIncrease, minSpawnInterval);
        Debug.Log($"🚗 Car spawn rate increased! New interval: {currentSpawnInterval:F2}s");
    }
}
