using UnityEngine;

[ExecuteAlways]
public class ForceAspectRatio : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        Camera cam = GetComponent<Camera>();
        Rect rect = cam.rect;

        if (currentAspect > targetAspect)
        {
            rect.width = 0.5f * (targetAspect / currentAspect);
            rect.x = 0.25f - (rect.width / 2f);
        }
        else
        {
            rect.width = 0.5f;
        }
        cam.rect = rect;
    }
}