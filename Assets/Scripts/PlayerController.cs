using UnityEngine;
using UnityEngine.SceneManagement;

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

    private PlayerCarFlasher carFlasher;

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
        carFlasher = GetComponent<PlayerCarFlasher>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (!hasMadeFirstMove && (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(forwardInput) > 0.01f))
        {
            hasMadeFirstMove = true;

            if (SpawnManager != null)
                SpawnManager.HideGlowMarker();

            if (carFlasher != null)
                carFlasher.TriggerFadeOut();
        }

        // Movement
        transform.Translate(Vector3.left * Time.deltaTime * Speed * forwardInput);
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        // Keep player within bounds
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
        Debug.Log("PlayerController: Level Completed!");
        transform.position = initialPos;
        transform.rotation = initialRot;
        hasMadeFirstMove = false;

        if (LevelCounterManager.Instance != null)
            LevelCounterManager.Instance.AddLevel();

        if (SpawnManager != null)
            SpawnManager.OpenSpot();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Option A: die when hitting any car
        if (other.CompareTag("ParkedCars") || other.CompareTag("MovingCar"))
        {
            Debug.Log("PlayerController: Player hit a car!");
            Deathscreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
