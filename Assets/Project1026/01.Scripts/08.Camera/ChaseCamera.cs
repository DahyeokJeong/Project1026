using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    //임시 코드

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;

    [SerializeField] private BattleCameraBounds cameraBounds;

    private void LateUpdate()
    {
        if (target == null && cameraBounds == null)
            return;

        Vector3 position = transform.position;

        if (target != null)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, position.z);
            position = Vector3.Lerp(position, targetPosition, followSpeed * Time.deltaTime);
        }

        if (cameraBounds != null)
            position = cameraBounds.Constrain(position);

        transform.position = position;
    }
}
