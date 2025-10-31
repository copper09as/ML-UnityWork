using UnityEngine;

public class CharacterAttack : CharacterState
{


    public override void OnEnter()
    {
        character.attackTimer = character.attackCooldown;
        character.state = 1;
        character.animator.Play("Attack");
    }

    public override void OnExit()
    {
    }

    public override void Update()
    {
        character.rb.velocity = new Vector2(character.speedSky * stateMachine.Input.moveDir, character.rb.velocity.y);

        // ==== 计算BoxCast参数 ====
        Vector2 dir = character.IsFacingRight() ? Vector2.right : Vector2.left;
        float halfWidth = character.col.bounds.extents.x;
        float halfHeight = character.col.bounds.extents.y;
        Vector2 boxSize = new Vector2(character.attackRange, halfHeight * 1.0f); // 攻击范围 * 高度
        Vector2 origin = (Vector2)character.transform.position + dir * halfWidth; // 从角色边缘出发

        // ==== BoxCast检测 ====
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, dir, 0f, character.playerLayer);
        Debug.DrawRay(origin - Vector2.up * halfHeight, dir * character.attackRange, Color.red); // 可视化

        if (hit.collider != null && hit.collider.gameObject != character.gameObject)
        {
            CharacteAgent target = hit.collider.GetComponent<CharacteAgent>();
            if (target != null && !target.isInvincible)
            {
                character.Attack();
            }
            else
            {
                character.AddReward(character.attackMissAward);
            }

            // 攻击结束回Idle
            var idleState = new CharacterIdle();
            idleState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(idleState);
            return;
        }

        // ==== 状态切换 ====
        if (stateMachine.Input.moveDir == 0)
        {
            var idleState = new CharacterIdle();
            idleState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(idleState);
            return;
        }
        else
        {
            var moveState = new CharacterMove();
            moveState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(moveState);
            return;
        }
    }

}
