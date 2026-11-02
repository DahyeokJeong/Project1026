using System.Reflection.Metadata;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnPosition
{
    private readonly MapArea mapArea;

    public EnemySpawnPosition(MapArea mapArea)
    {
        this.mapArea = mapArea;
    }

    public bool TryGetPosition(float clearance, int attempts, out Vector2 pos)
    {
        pos = Vector2.zero;

        if (mapArea == null || !mapArea.IsReady)
            return false;

        if (!mapArea.TryGetInserBounds(clearance, out Vector2 min, out Vector2 max))
            return false;

        float width = max.x - min.x;
        float height = max.y - min.y;

        if (width <= 0f || height <= 0f)
            return false;

        float perimeter = (width + height) * 2f;

        for (int i = 0; i < attempts; ++i)
        {
            float dis = Random.Range(0f, perimeter);
            Vector2 candidate;

            if (dis < width)
                candidate = new Vector2(min.x + dis, min.y);
            else if (dis < width + height)
                candidate = new Vector2(max.x, min.y + dis - width);
            else if (dis < width * 2f + height)
                candidate = new Vector2(max.x - (dis - width - height), max.y);
            else
                candidate = new Vector2(min.x, max.y - (dis - width * 2f - height));

            if(!mapArea.CanPlace(candidate, clearance))
                continue;

            pos = candidate;
            return true;
        }
        return false;
    }
}
