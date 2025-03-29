
using System.Collections;
using UnityEngine;

public class MasterSpawner : MonoBehaviour
{
    [Header("Ground Settings")]
    public float groundSegmentLength = 10f;
    public int visibleGroundSegments = 5;
    private float nextGroundZ = 0f;

    [Header("Obstacle Settings")]
    public float minObstacleDistance = 15f;
    public float maxObstacleDistance = 25f;
    public float[] lanes = { -2f, 2f };
    public float orbSpawnChance = 0.9f;
    public float initialSpawnOffset = 20;

    [Header("Game Completion")]
    public float completionDistance = 170f; // Distance to complete the game
    public bool gameCompleted = false;
    public float stopSpawningDistance = 20f; // Stop spawning obstacles X units before end
    private bool stopSpawning = false;

    [Header("Finish Line Settings")]
    public float finishLineAppearDistance = 40f;
    private bool finishLineSpawned = false;

    private Transform player;
    private float nextObstacleZ = 0f; 
    public static MasterSpawner Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("MasterSpawner started");
        Debug.Log("nextObstacleZ: " + nextObstacleZ);
        player = PlayerController.Instance.transform;

        // Initial ground
        for (int i = 0; i < visibleGroundSegments; i++)
        {
            SpawnGround();
        }

        nextObstacleZ = player.position.z + initialSpawnOffset;
        SpawnObstacleGroup(nextObstacleZ);
    }

    void Update()
    {
        // Check if we should show finish line
        if (!finishLineSpawned && player.position.z >= completionDistance - finishLineAppearDistance)
        {
            Debug.LogWarning("Stopped spawning obstacles - approaching finish" + gameCompleted);
            SpawnFinishLine();
        }

        if (!gameCompleted && (player.position.z >= completionDistance + 5f))
        {
            CompleteGame();
            return;
        }

        // Spawn new obstacle when player is halfway to nextObstacleZ
        if (player.position.z > nextObstacleZ - (minObstacleDistance / 2))
        {
            float newZ = nextObstacleZ + Random.Range(minObstacleDistance, maxObstacleDistance);
            SpawnObstacleGroup(newZ);
            nextObstacleZ = newZ;
        }

        // Ground spawning (independent of obstacles)
        if (player.position.z > nextGroundZ - (visibleGroundSegments * groundSegmentLength))
        {
            SpawnGround();
        }
    }

    void SpawnFinishLine()
    {
        finishLineSpawned = true;

        // Stop all obstacle spawning
        stopSpawning = true;

        // Spawn finish line from pool
        Vector3 finishPos = new Vector3(0, 0.01f, completionDistance);
        GameObject finishLine = PoolManager.Instance.SpawnFromPool("FinishLine", finishPos);
    }

    void SpawnGround()
    {
        if (stopSpawning) return;
        PoolManager.Instance.SpawnFromPool("Ground", new Vector3(0, 0, nextGroundZ));
        nextGroundZ += groundSegmentLength;
    }

    void SpawnObstacleGroup(float zPos)
    {
        if (stopSpawning) return;
        // Spawn obstacles in both lanes
        PoolManager.Instance.SpawnFromPool("Obstacle", new Vector3(lanes[0], 0.5f, zPos));
        PoolManager.Instance.SpawnFromPool("Obstacle", new Vector3(lanes[1], 0.5f, zPos));

        // Spawn multiple orbs between previous and current obstacle
        if (Random.value <= orbSpawnChance)
        {
            int orbCount = Random.Range(2, 5); // Spawn 2-4 orbs per group
            float minOrbZ = zPos - minObstacleDistance;
            float maxOrbZ = zPos - 5f; // 5f buffer before next obstacle

            for (int i = 0; i < orbCount; i++)
            {
                // Place orb Z positions evenly
                float lerpFactor = (i + 1f) / (orbCount + 1f);
                float orbZ = Mathf.Lerp(minOrbZ, maxOrbZ, lerpFactor);

                PoolManager.Instance.SpawnFromPool("Orb", new Vector3(lanes[0], 1f, orbZ));
                PoolManager.Instance.SpawnFromPool("Orb", new Vector3(lanes[1], 1f, orbZ));
            }
        }
    }

    void CompleteGame()
    {
        Debug.Log("Game completed successfully!");
        PlayerController.Instance.StartSmoothStop();
        gameCompleted = true;
    }
}