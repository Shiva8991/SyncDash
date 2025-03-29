using UnityEngine;

[RequireComponent(typeof(Camera))] 
public class SmartCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float followSmoothness = 5f;

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.025f;
    public bool shakeEnabled = true;

    private Vector3 originalPosition;
    private float currentShakeTime;
    private Vector3 basePosition;

    void Start()
    {
        if (target == null)
            Debug.LogError("Camera target not assigned!");
        
        originalPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        basePosition = target.position + offset;

        Vector3 smoothPosition = Vector3.Lerp(
            transform.position,
            basePosition,
            followSmoothness * Time.deltaTime
        );

        if (currentShakeTime > 0 && shakeEnabled)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            transform.position = smoothPosition + shakeOffset;
            currentShakeTime -= Time.deltaTime;
        }
        else
        {
            transform.position = smoothPosition;
        }
    }

    public void TriggerShake(float duration = 0.5f, float magnitude = 0.1f)
    {
        if (!shakeEnabled) return;
        
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeTime = shakeDuration;
    }

    public void ToggleShake(bool enable)
    {
        shakeEnabled = enable;
    }
}



