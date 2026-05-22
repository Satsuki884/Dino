using UnityEngine;

public class CameraAutoSize : MonoBehaviour
{
    public Camera targetCamera;
    public BoxCollider2D fieldBounds;
    public float padding = 1f;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        FitCamera();
    }

    private void FitCamera()
    {
        if (targetCamera == null || fieldBounds == null)
            return;

        Bounds bounds = fieldBounds.bounds;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = bounds.size.x / bounds.size.y;

        if (screenRatio >= targetRatio)
        {
            targetCamera.orthographicSize = bounds.size.y / 2f + padding;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            targetCamera.orthographicSize = bounds.size.y / 2f * differenceInSize + padding;
        }

        targetCamera.transform.position = new Vector3(
            bounds.center.x,
            bounds.center.y,
            -10f
        );
    }
}