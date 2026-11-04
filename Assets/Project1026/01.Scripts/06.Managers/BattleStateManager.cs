using UnityEngine;

public enum BattleState
{
    Playing,
    AugmentSelecting,
    Paused,
    Cleared
}

public class BattleStateManager
{
    public BattleState Current {  get; private set; } = BattleState.Playing;
    public bool CanUpdate => Current == BattleState.Playing;

    public void SetState(BattleState state)
    {
        Current = state;
    }

    public void Reset()
    {
        Current = BattleState.Playing;
    }
}
