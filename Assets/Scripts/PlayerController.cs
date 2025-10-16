using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed = 5.0f;
    public float turnSpeed = 5.0f;
    private float horizontalInput;
    private float forwardInput;
    private Vector3 initialPos;
    private Quaternion initialRot;
    public SpawnManager SpawnManager;
    public GameObject Deathscreen;
    public float xRange = 9.5f;
    public float zRange = 5.7f;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        transform.Translate(Vector3.left * Time.deltaTime * Speed * forwardInput);
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);


        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }

        if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }

        if (transform.position.y < -zRange)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -zRange);
        }

        if (transform.position.y > zRange)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zRange);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ParkedCars")
        {
            Debug.Log("GameOver");
            Destroy(gameObject);

            Deathscreen.SetActive(true);
        }

        if (other.tag == "SuccessCollision")
        {
            Debug.Log("Success!");
            transform.position = initialPos;
            transform.rotation = initialRot;
            SpawnManager.OpenSpot();

            // Increase Level Counter
            if (LevelCounterManager.Instance != null)
            {
                LevelCounterManager.Instance.AddLevel();
            }
            else
            {
                Debug.LogWarning("No LevelCounterManager found in the scene!");
            }
        }


    }



}
