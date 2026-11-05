public interface IAttack
{
}

public interface ITickAttack : IAttack
{
    void Tick(float deltaTime);
}

public interface IActivatableAttack : IAttack
{
    void Activate();
}