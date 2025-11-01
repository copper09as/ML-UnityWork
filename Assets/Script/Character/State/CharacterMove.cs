using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMove : CharacterState
{
    
    public override void OnEnter()
    {
        character.state = 3;
        character.animator.Play("Walk");
    }

    public override void OnExit()
    {
       
    }

    public override void Update()
    {
        character.rb.velocity = new Vector2(character.speed * stateMachine.Input.moveDir, character.rb.velocity.y);
        if (stateMachine.Input.attack && character.attackTimer<=0)
        {
            var attackState = new CharacterAttack();
            attackState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(attackState);
            return;
        }
        if (stateMachine.Input.jump && character.inGround)
        {
            var jumpState = new CharacterJump();
            jumpState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(jumpState);
            return;
        }
        if(stateMachine.Input.moveDir==0)
        {
            var idleState = new CharacterIdle();
            idleState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(idleState);
            return;
        }
    }
}

