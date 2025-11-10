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

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;

        if (SpawnManager == null)
            SpawnManager = FindObjectOfType<SpawnManager>();

        if (SpawnManager != null)
            SpawnManager.ResetLevelOpenFlag();
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        transform.Translate(Vector3.left * Time.deltaTime * Speed * forwardInput);
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -xRange, xRange),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -zRange, zRange)
        );
    }

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
}
