using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float Speed = 5.0f;
    public float turnSpeed = 5.0f;

    private float horizontalInput;
    private float forwardInput;
    private bool hasMadeFirstMove = false;

    private Vector3 initialPos;
    private Quaternion initialRot;

    [Header("References")]
    public SpawnManager SpawnManager;
    public GameObject Deathscreen;

    [Header("Bounds")]
    public float xRange = 9.5f;
    public float zRange = 5.7f;

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;

        if (SpawnManager == null)
            SpawnManager = FindObjectOfType<SpawnManager>();

<<<<<<< HEAD
<<<<<<< HEAD
=======
        // Make sure SpawnManager knows we’re starting fresh on a map
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
=======
        // Make sure SpawnManager knows we’re starting fresh on a map
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
        if (SpawnManager != null)
            SpawnManager.ResetLevelOpenFlag();
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
        if (!hasMadeFirstMove && (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(forwardInput) > 0.01f))
        {
            hasMadeFirstMove = true;

            // Hide glow marker on first movement
            if (SpawnManager != null)
                SpawnManager.HideGlowMarker();
        }

        // Movement logic
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
        transform.Translate(Vector3.left * Time.deltaTime * Speed * forwardInput);
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -xRange, xRange),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -zRange, zRange)
        );
    }

<<<<<<< HEAD
=======
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("SuccessCollision"))
        {
            if (IsFullyInsideCollider(other))
            {
                HandleLevelSuccess();
            }
        }
    }

    private bool IsFullyInsideCollider(Collider goal)
    {
        Collider playerCol = GetComponent<Collider>();
        if (playerCol == null) return false;

        Bounds playerBounds = playerCol.bounds;
        Vector3[] corners = new Vector3[8];
        corners[0] = playerBounds.min;
        corners[7] = playerBounds.max;

        for (int i = 1; i < 7; i++)
        {
            corners[i] = new Vector3(
                i % 2 == 0 ? playerBounds.max.x : playerBounds.min.x,
                i < 4 ? playerBounds.min.y : playerBounds.max.y,
                (i == 1 || i == 2 || i == 5 || i == 6) ? playerBounds.max.z : playerBounds.min.z
            );
        }

        foreach (Vector3 corner in corners)
        {
            if (!goal.bounds.Contains(corner))
                return false;
        }

        return true;
    }

    private void HandleLevelSuccess()
    {
        Debug.Log("✅ PlayerController: Level Completed!");
        transform.position = initialPos;
        transform.rotation = initialRot;
        hasMadeFirstMove = false;

        if (LevelCounterManager.Instance != null)
            LevelCounterManager.Instance.AddLevel();

        // Tell SpawnManager to open a new single spot (without duplicate)
        if (SpawnManager != null)
        {
            SpawnManager.ResetLevelOpenFlag();  // Reset spawn control
            SpawnManager.OpenSpot(false);       // Spawn one new open spot, no marker
        }
        else
        {
            Debug.LogWarning("PlayerController: SpawnManager is null when trying to open next spot.");
        }
    }

>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ParkedCars") || other.CompareTag("MovingCar"))
        {
            Debug.Log("💥 Player hit a car!");
            if (DeathscreenManager.Instance != null)
                DeathscreenManager.Instance.ShowDeathscreen();
            else
                Debug.LogError("❌ No DeathscreenManager instance found!");
        }
    }
<<<<<<< HEAD
<<<<<<< HEAD

    // 👇 Add these methods for UI Button OnClick events

    public void RestartGame()
    {
        Debug.Log("🔁 Restart button pressed (PlayerController).");
        Time.timeScale = 1f;

        if (DeathscreenManager.Instance != null && DeathscreenManager.Instance.deathscreenUI != null)
        {
            DeathscreenManager.Instance.deathscreenUI.SetActive(false);
        }
        else if (Deathscreen != null)
        {
            Deathscreen.SetActive(false);
        }

        if (LevelCounterManager.Instance != null)
            LevelCounterManager.Instance.ResetCounter();

        SceneManager.LoadScene(1);
    }

    // 👇 Quit button logic
    public void QuitToMenu()
    {
        Debug.Log("🚪 Quit button pressed (PlayerController).");
        Time.timeScale = 1f;

        if (DeathscreenManager.Instance != null && DeathscreenManager.Instance.deathscreenUI != null)
            DeathscreenManager.Instance.deathscreenUI.SetActive(false);
        else if (Deathscreen != null)
            Deathscreen.SetActive(false);

        SceneManager.LoadScene(0);
    }
=======
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
=======
>>>>>>> parent of 15f59a9 (moving car spawner overhaul + bug fixes)
}
