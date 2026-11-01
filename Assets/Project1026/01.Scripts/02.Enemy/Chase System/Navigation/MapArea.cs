using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapArea : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private SpriteRenderer mapRander;
    [SerializeField] private LayerMask obsLayer;

    public bool IsReady => mapRander != null;
    public Bounds worldBounds => mapRander.bounds;
    public LayerMask Obs => obsLayer;

    public bool TryGetInserBounds(float clearance, out Vector2 min, out Vector2 max)
    {
        min = Vector2.zero;
        max = Vector2.zero;

        if (!IsReady)
            return false;

        clearance = Mathf.Max(0f, clearance);

        Bounds bounds = worldBounds;

        min = (Vector2)bounds.min + Vector2.one * clearance;
        max = (Vector2)bounds.max - Vector2.one * clearance;

        return min.x <= max.x && min.y <= max.y;
    }

    public bool CanPlace(Vector2 pos, float clearance)
    {
        if (!TryGetInserBounds(clearance, out Vector2 min, out Vector2 max))
            return false;

        if (pos.x < min.x || pos.x > max.x || pos.y < min.y || pos.y > max.y)
            return false;

        return Physics2D.OverlapCircle(pos, Mathf.Max(0f, clearance), obsLayer) == null;
    }
}
