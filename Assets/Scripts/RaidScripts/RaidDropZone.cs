using UnityEngine;

public class RaidDropZone : MonoBehaviour
{
    private Collider2D zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    public bool IsDinoInsideRaidZone(Dino dino)
    {
        if (dino == null)
            return false;

        return ContainsPoint(dino.transform.position);
    }

    public bool ContainsPoint(Vector3 point)
    {
        if (zoneCollider == null)
            return false;

        return zoneCollider.OverlapPoint(point);
    }
}
