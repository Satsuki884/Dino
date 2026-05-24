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

        if (zoneCollider == null)
            return false;

        return zoneCollider.OverlapPoint(dino.transform.position);
    }
}