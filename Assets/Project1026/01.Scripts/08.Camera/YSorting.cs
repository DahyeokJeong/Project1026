using UnityEngine;

public class YSorting : MonoBehaviour
{
    #region Component
    [Header("Component")]
    [SerializeField] private SpriteRenderer[] sr;
    #endregion

    #region Sorting
    [Header("Sorting")]
    [SerializeField] private Transform sortingPoint;
    [SerializeField] private bool isDynamic = false;
    #endregion

    private const float SORTING_SCALE = 10f;

    private void Awake()
    {
        if (sr == null || sr.Length == 0)
        {
            sr = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        UpdateSortingOrder();
    }

    private void LateUpdate()
    {
        if (!isDynamic)
            return;

        UpdateSortingOrder();
    }

    private void UpdateSortingOrder()
    {
        int sortingOrder = Mathf.RoundToInt(-sortingPoint.position.y * SORTING_SCALE);

        foreach (SpriteRenderer sr in sr)
        {
            sr.sortingOrder = sortingOrder;
        }
    }
}