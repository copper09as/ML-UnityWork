using UnityEngine;
using System;

public class CharacterDash : CharacterState
{

    private float originalGravity;
    private Rigidbody2D rb;
    private int dir;

    public override void OnEnter()
    {
        dir = stateMachine.Input.moveDir;
        character.state = 4;
        character.isInvincible = true;
        character.dashTimer = 0f;

        rb = character.rb;

        originalGravity = rb.gravityScale;

        rb.gravityScale = 0f;


        character.animator.Play("Dash");

    }

    public override void OnExit()
    {
        character.isInvincible = false;
        rb.gravityScale = originalGravity;
    }

    public override void Update()
    {
        character.dashTimer += Time.deltaTime;

        // 保持水平速度
        rb.velocity = new Vector2(dir*character.dashSpeed, 0f);

        if (character.dashTimer >= character.dashDuration)
        {
            if (stateMachine.Input.moveDir == 0)
            {
                var idleState = new CharacterIdle();
                idleState.InjectStateMachine(stateMachine, character);
                stateMachine.Enter(idleState);
            }
            else
            {
                var moveState = new CharacterMove();
                moveState.InjectStateMachine(stateMachine, character);
                stateMachine.Enter(moveState);
            }
        }
    }
}
