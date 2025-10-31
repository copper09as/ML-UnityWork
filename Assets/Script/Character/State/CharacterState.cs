

public abstract class CharacterState
{
    protected CharacterStateMachine stateMachine;
    protected CharacteAgent character;
    public virtual void InjectStateMachine(CharacterStateMachine machine, CharacteAgent character)
    {
        stateMachine = machine;
        this.character = character;
    }
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Update();
}

