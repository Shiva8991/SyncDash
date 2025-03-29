using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("Settings")]
    public float followSmoothness = 8f;
    public float jumpForce = 8f;
    public float xOffset = -2f; 

    private Queue<PlayerController.PlayerData> receivedData = new Queue<PlayerController.PlayerData>();
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.position = new Vector3(xOffset, 0.5f, 0f);
    }

    void Update()
    {
        // Receive player data
        while (player.dataQueue.Count > 0)
        {
            receivedData.Enqueue(player.dataQueue.Dequeue());
        }

        // Apply with delay (0.15s)
        if (receivedData.Count > 3)
        {
            PlayerController.PlayerData data = receivedData.Dequeue();
            targetPosition = new Vector3(xOffset, data.position.y, data.position.z);
            if (data.isJumping && isGrounded)
            {
                rb.velocity = new Vector3(0, jumpForce, 0);
                isGrounded = false;
            }
        }
        transform.position = Vector3.Lerp(transform.position,
                                        targetPosition,
                                        followSmoothness * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

     void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            PoolManager.Instance.ReturnToPool("Orb", other.gameObject);
        }
    }
}