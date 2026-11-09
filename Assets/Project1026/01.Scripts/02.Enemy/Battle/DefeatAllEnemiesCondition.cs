using UnityEngine;

public class DefeatAllEnemiesCondition : IStageClearCondition
{
    private readonly EnemyGenerate generate;

    public DefeatAllEnemiesCondition(EnemyGenerate generate)
    {
        this.generate = generate;
    }

    public bool IsSatisfied()
    {
        return generate.ActiveEnemyCount == 0;
    }
}
