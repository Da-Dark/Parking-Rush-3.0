using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MovingCar : MonoBehaviour
{
    public float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.left);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Turn Collision")
        {
            Debug.Log("Turning");
            transform.Rotate(Vector3.up, other.GetComponent<RandomTurn>().turnAngle[Random.Range(0, other.GetComponent<RandomTurn>().turnAngle.Length)]);
        }
    }
}
