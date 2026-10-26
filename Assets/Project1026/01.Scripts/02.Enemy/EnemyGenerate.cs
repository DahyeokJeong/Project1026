using UnityEngine;

public class EnemyGenerate : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Nav Ai")]
    [SerializeField] private NavigationArea area;
    private GameObject enemyObject;

    void Start()
    {
        enemyObject = Managers.Pool.Pop(enemyPrefab, transform.position, Quaternion.identity);

        if (enemyObject == null)
            return;


    }

    void Update()
    {
        
    }
}
