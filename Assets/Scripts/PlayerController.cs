using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 5f;
    public float jumpForce = 6f;
    public float speedIncreaseRate = 0.5f;
    public float jumpCooldown = 0.5f;

    [Header("Network Sync")]
    public float syncInterval = 0.05f;



    [Header("Finish Line")]
    public float stoppingDuration = 1f; // Time it takes to fully stop
    private bool isStopping = false;
    private float stopStartTime;
    private float initialStopSpeed;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float nextSyncTime;
    private float lastJumpTime;

    // Data structure for ghost syncing
    [System.Serializable]
    public struct PlayerData
    {
        public Vector3 position;
        public bool isJumping;
        public float currentSpeed;
    }
    public Queue<PlayerData> dataQueue = new Queue<PlayerData>();
    public static PlayerController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        nextSyncTime = Time.time;
    }

    void FixedUpdate()
    {
        if (isStopping)
        {
            HandleStopMovement();
        }
        else
        {
            forwardSpeed += speedIncreaseRate * Time.deltaTime;
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }

        // Network sync
        if (Time.time >= nextSyncTime)
        {
            dataQueue.Enqueue(new PlayerData
            {
                position = transform.position,
                isJumping = !isGrounded,
                currentSpeed = forwardSpeed
            });
            nextSyncTime = Time.time + syncInterval;
        }
    }

    void Update()
    {
        if (CanJump())
        {
            Debug.LogWarning("JUMP");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;
        }
    }

    bool CanJump()
    {
        return Input.GetMouseButtonDown(0)
               && isGrounded
               && Time.time > lastJumpTime + jumpCooldown
               && !GameManager.Instance.IsGameOver()
               && GameManager.Instance.CanJump();
    }

    public void StartSmoothStop()
    {
        if (!isStopping)
        {
            isStopping = true;
            stopStartTime = Time.time;
            initialStopSpeed = forwardSpeed;
        }
    }

    void HandleStopMovement()
    {
        GameManager.Instance.SetCanJump(false);
        float stopProgress = (Time.time - stopStartTime) / stoppingDuration;

        if (stopProgress >= 1f)
        {
            // Fully stopped
            forwardSpeed = 0f;
            isStopping = false;
            transform.Translate(Vector3.forward * 0);
            GameManager.Instance.CompleteGame();
        }
        else
        {
            forwardSpeed = Mathf.Lerp(initialStopSpeed, 0f, stopProgress);
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ground check
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if (!collision.gameObject.CompareTag("Obstacle") || GameManager.Instance.IsGameOver())
            return;

        GameManager.Instance.GameOver();

        // Return obstacle to pool
        if (collision.gameObject.TryGetComponent<AutoReturnToPool>(out var poolComponent) && !GameManager.Instance.IsGameOver())
        {
            PoolManager.Instance.ReturnToPool(poolComponent.poolTag, collision.gameObject);
        }
    }

    // Orb collection (trigger-based)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            // Replace PooledObject with direct pool access
            PoolManager.Instance.ReturnToPool("Orb", other.gameObject);
            ScoreManager.Instance.AddScore(10);
        }
    }
}