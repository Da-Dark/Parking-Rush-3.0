using UnityEngine;

public class CarSpawnerSettings : MonoBehaviour
{
    public enum SpawnType { Slow, Normal, Fast }

    [Header("Spawner Type Settings")]
    public SpawnType spawnType = SpawnType.Normal;

    [Tooltip("Optional override for car spawn speed (leave 0 to use defaults).")]
    public float customCarSpeed = 0f;
}
