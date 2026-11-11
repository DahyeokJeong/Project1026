using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BattleCameraBounds : MonoBehaviour
{
    [SerializeField] private MapArea mapArea;

    private Camera battleCamera;
    private float defaultOrthographicSize;

    private void Awake()
    {
        battleCamera = GetComponent<Camera>();
        defaultOrthographicSize = battleCamera.orthographicSize;
    }

    public Vector3 Constrain(Vector3 position)
    {
        if (!isActiveAndEnabled || !battleCamera.orthographic || mapArea == null || !mapArea.IsReady)
            return position;

        Bounds bounds = mapArea.worldBounds;
        float aspect = battleCamera.aspect;
        Vector3 extents = bounds.extents;
        if (aspect <= 0f || extents.x <= 0f || extents.y <= 0f)
            return position;

        float maxHalfHeight = Mathf.Min(extents.y, extents.x / aspect);
        float halfHeight = Mathf.Min(defaultOrthographicSize, maxHalfHeight);
        battleCamera.orthographicSize = halfHeight;

        Vector3 center = bounds.center;
        float horizontalTravel = Mathf.Max(0f, extents.x - halfHeight * aspect);
        float verticalTravel = Mathf.Max(0f, extents.y - halfHeight);
        position.x = Mathf.Clamp(position.x, center.x - horizontalTravel, center.x + horizontalTravel);
        position.y = Mathf.Clamp(position.y, center.y - verticalTravel, center.y + verticalTravel);
        return position;
    }
}
