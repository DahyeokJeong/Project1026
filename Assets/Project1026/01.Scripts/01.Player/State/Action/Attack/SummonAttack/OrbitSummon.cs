using UnityEngine;

public class OrbitSummon : SummonBase
{
    #region Settins
    [Header("Orbit Settings")]
    [SerializeField] private float angularSpeed = 180f;
    [SerializeField, Range(0.1f, 1f)] private float verticalRatio = 0.5f;
    #endregion

    #region Fields
    private float angle;
    private float orbitRadius;
    private float orbitVerticalRatio;
    private float zOffset;
    #endregion

    protected override void OnSummonInitialized()
    {
        Vector3 offset = transform.position - Owner.transform.position;

        orbitVerticalRatio = Mathf.Clamp(verticalRatio, 0.1f, 1f);
        Vector2 orbitOffset = new Vector2(offset.x, offset.y / orbitVerticalRatio);

        orbitRadius = orbitOffset.magnitude;
        angle = Mathf.Atan2(orbitOffset.y, orbitOffset.x);
        zOffset = offset.z;
    }

    protected override void Move(float deltaTime)
    {
        angle = Mathf.Repeat(angle + angularSpeed * Mathf.Deg2Rad * deltaTime, Mathf.PI * 2f);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            Mathf.Sin(angle) * orbitRadius * orbitVerticalRatio,
            zOffset);

        transform.position = Owner.transform.position + offset;
    }
}