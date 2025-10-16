using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] parkedCars;
    public GameObject[] CarInitialPos;
    public GameObject MovingRedCar;
    public float RespawnTime = 10;
    public float MovingTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        parkedCars[Random.Range(0, parkedCars.Length)].SetActive(false);
        InvokeRepeating("SpawnMovingCar", MovingTime, RespawnTime);
    }

    // Update is called once per frame
    void Update()
    {
         
    }
    public void OpenSpot ()
    {
        for (int i = 0; i < parkedCars.Length;i++)
        {
            parkedCars[i].SetActive(true);

        }
        parkedCars[Random.Range(0, parkedCars.Length)].SetActive(false);

    }

    public void SpawnMovingCar ()
    {
        int SpawnPosition = Random.Range(0, CarInitialPos.Length);
        Instantiate(MovingRedCar, CarInitialPos[SpawnPosition].transform.position, CarInitialPos[SpawnPosition].transform.rotation);


    }
}
