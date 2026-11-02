using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    //임시 코드

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime);
    }
}
