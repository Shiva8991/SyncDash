using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    public string poolTag;
    public float returnOffset = 20f; // Units behind player to despawn

    void Update()
    {
        if (transform.position.z < PlayerController.Instance.transform.position.z - returnOffset)
        {
            if (gameObject.activeSelf) 
            {
                PoolManager.Instance.ReturnToPool(poolTag, gameObject);
            }
        }
    }
}