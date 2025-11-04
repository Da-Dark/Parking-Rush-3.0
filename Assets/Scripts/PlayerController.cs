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
    public SpawnManager SpawnManager; // auto-found if not set
    public GameObject Deathscreen;

    [Header("Bounds")]
    public float xRange = 9.5f;
    public float zRange = 5.7f;

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;

        // Auto-find SpawnManager if not assigned
        if (SpawnManager == null)
        {
            SpawnManager = FindObjectOfType<SpawnManager>();
            if (SpawnManager == null)
                Debug.LogWarning("PlayerController: SpawnManager not found in scene.");
        }

        // Make sure SpawnManager knows we’re starting fresh on a map
        if (SpawnManager != null)
            SpawnManager.ResetLevelOpenFlag();
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (!hasMadeFirstMove && (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(forwardInput) > 0.01f))
        {
            hasMadeFirstMove = true;

            // Hide glow marker on first movement
            if (SpawnManager != null)
                SpawnManager.HideGlowMarker();
        }

        // Movement logic
        transform.Translate(Vector3.left * Time.deltaTime * Speed * forwardInput);
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        // Keep player within map bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -xRange, xRange),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -zRange, zRange)
        );
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ParkedCars") || other.CompareTag("MovingCar"))
        {
            Debug.Log("💥 PlayerController: Player hit a car!");
            if (Deathscreen != null)
            {
                Deathscreen.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
}
