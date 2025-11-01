using UnityEngine;

public class CharacterJump : CharacterState
{

    public override void OnEnter()
    {
        character.rb.velocity = new Vector2(character.rb.velocity.x, character.jumpHeight);
        character.state = 2;
        character.animator.Play("Jump");
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
        if (character.inGround)
        {
            if (stateMachine.Input.moveDir == 0)
            {
                var idleState = new CharacterIdle();
                idleState.InjectStateMachine(stateMachine, character);
                stateMachine.Enter(idleState);
                return;
            }
            if (stateMachine.Input.moveDir != 0)
            {
                var moveState = new CharacterMove();
                moveState.InjectStateMachine(stateMachine, character);
                stateMachine.Enter(moveState);
                return;
            }
        }
    }
}

