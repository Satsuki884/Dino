using System;
using UnityEngine;

public class WorldImageLayers : MonoBehaviour
{
    private static readonly int WorldShadowCenterId = Shader.PropertyToID("_WorldShadowCenter");
    private static readonly int WorldShadowSizeId = Shader.PropertyToID("_WorldShadowSize");

    [Serializable]
    public class ImageLayer
    {
        public SpriteRenderer renderer;
        public Sprite sprite;
        public int sortingOrder;
        [Range(0f, 1f)] public float alpha = 1f;
        public float zOffset;
    }

    [Header("Camera")]
    public Camera targetCamera;
    public bool matchLayersToBackground = true;

    [Header("Layers")]
    public ImageLayer background = new ImageLayer
    {
        sortingOrder = 0,
        alpha = 1f,
        zOffset = 4f
    };

    public ImageLayer foreground = new ImageLayer
    {
        sortingOrder = 20,
        alpha = 1f,
        zOffset = 2f
    };

    public ImageLayer smoke = new ImageLayer
    {
        sortingOrder = 5,
        alpha = 1f,
        zOffset = 3f
    };

    public ImageLayer raid = new ImageLayer
    {
        sortingOrder = 10,
        alpha = 1f,
        zOffset = 2.5f
    };

    public ImageLayer shadow = new ImageLayer
    {
        sortingOrder = 15,
        alpha = 0.45f,
        zOffset = 1f
    };

    private int lastScreenWidth;
    private int lastScreenHeight;
    private float lastOrthographicSize;
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        EnsureLayer(ref background, "Background");
        EnsureLayer(ref foreground, "Foreground");
        EnsureLayer(ref smoke, "Smoke");
        EnsureLayer(ref raid, "Raid");
        EnsureLayer(ref shadow, "Shadow");
        ApplyLayers();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        if (!HasCameraChanged())
            return;

        ApplyLayers();
    }

    private bool HasCameraChanged()
    {
        return lastScreenWidth != Screen.width ||
               lastScreenHeight != Screen.height ||
               !Mathf.Approximately(lastOrthographicSize, targetCamera.orthographicSize) ||
               lastCameraPosition != targetCamera.transform.position;
    }

    private void ApplyLayers()
    {
        float matchedScale = GetMatchedScale();

        ApplyLayer(background, matchedScale);
        ApplyLayer(smoke, matchedScale);
        ApplyLayer(raid, matchedScale);
        ApplyLayer(foreground, matchedScale);
        ApplyLayer(shadow, matchedScale);
        UpdateWorldShadowProjection();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastOrthographicSize = targetCamera.orthographicSize;
        lastCameraPosition = targetCamera.transform.position;
    }

    private void EnsureLayer(ref ImageLayer layer, string objectName)
    {
        if (layer == null)
            layer = new ImageLayer();

        if (layer.renderer != null)
            return;

        GameObject layerObject = new GameObject(objectName);
        layerObject.transform.SetParent(transform, false);
        layer.renderer = layerObject.AddComponent<SpriteRenderer>();
    }

    private float GetMatchedScale()
    {
        if (!matchLayersToBackground || background == null || background.sprite == null)
            return 0f;

        Vector2 backgroundSize = background.sprite.bounds.size;

        if (backgroundSize.x <= 0f || backgroundSize.y <= 0f)
            return 0f;

        return GetCameraCoverScale(backgroundSize);
    }

    private void ApplyLayer(ImageLayer layer, float matchedScale)
    {
        if (targetCamera == null || layer == null || layer.renderer == null)
            return;

        if (layer.sprite != null)
            layer.renderer.sprite = layer.sprite;

        layer.renderer.sortingOrder = layer.sortingOrder;
        Color color = layer.renderer.color;
        color.a = layer.alpha;
        layer.renderer.color = color;

        Transform layerTransform = layer.renderer.transform;
        Vector3 cameraPosition = targetCamera.transform.position;
        layerTransform.position = new Vector3(
            cameraPosition.x,
            cameraPosition.y,
            cameraPosition.z + layer.zOffset
        );

        if (matchedScale > 0f)
            layer.renderer.transform.localScale = new Vector3(matchedScale, matchedScale, 1f);
        else
            FitRendererToCamera(layer.renderer);
    }

    private void FitRendererToCamera(SpriteRenderer renderer)
    {
        if (renderer.sprite == null)
            return;

        Vector2 spriteSize = renderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        float scale = GetCameraCoverScale(spriteSize);
        renderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private float GetCameraCoverScale(Vector2 spriteSize)
    {
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        return Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
    }

    private void UpdateWorldShadowProjection()
    {
        SpriteRenderer referenceRenderer = background != null
            ? background.renderer
            : null;

        if (referenceRenderer == null || referenceRenderer.sprite == null)
            return;

        Bounds bounds = referenceRenderer.bounds;
        Shader.SetGlobalVector(WorldShadowCenterId, new Vector4(
            bounds.center.x,
            bounds.center.y,
            0f,
            0f
        ));
        Shader.SetGlobalVector(WorldShadowSizeId, new Vector4(
            bounds.size.x,
            bounds.size.y,
            0f,
            0f
        ));
    }
}
