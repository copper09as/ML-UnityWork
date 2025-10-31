using UnityEngine;

public class CharacterIdle : CharacterState
{

    public override void OnEnter()
    {
        character.state = 0;
    }

    public override void OnExit()
    {

    }

    public override void Update()
    {
        character.rb.velocity = new Vector2(character.speedSky * stateMachine.Input.moveDir, character.rb.velocity.y);
        if (stateMachine.Input.attack && character.attackTimer <= 0)
        {
            var attackState = new CharacterAttack();
            attackState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(attackState);
            return;
        }
        if (stateMachine.Input.moveDir != 0)
        {
            var moveState = new CharacterMove();
            moveState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(moveState);
            return;
        }
        if (stateMachine.Input.jump && character.inGround)
        {
            var jumpState = new CharacterJump();
            jumpState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(jumpState);
            return;
        }
        
    }
}

