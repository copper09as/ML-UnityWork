using UnityEngine;

public class CharacterAttack : CharacterState
{


    private bool hasAttacked = false;

    public override void OnEnter()
    {
        character.attackTimer = character.attackCooldown;
        character.state = 1;
        character.animator.Play("Attack");
        hasAttacked = false; // 每次进入攻击状态重置
    }

    public override void OnExit()
    {
        
    }

    public override void Update()
    {
        // 控制水平移动
        character.rb.velocity = new Vector2(character.speedSky * stateMachine.Input.moveDir, character.rb.velocity.y);
        if (!hasAttacked)
        {
            Vector2 dir = character.IsFacingRight() ? Vector2.right : Vector2.left;

            // Collider 中心
            Vector2 colCenter = character.col.bounds.center;
            float colHeight = character.col.bounds.size.y;
            float colWidth = character.col.bounds.size.x;

            // 射线起点：从胸口位置出发（中心向上 0.2~0.3 高度）
            Vector2 origin = colCenter + dir * (colWidth / 2) + Vector2.up * (colHeight * 0.2f);



            float distance = character.attackRange;

            // 发射射线检测前方所有碰撞体
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, distance, character.playerLayer);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != character.gameObject)
                {
                    // 只攻击前方的角色
                    Vector2 toTarget = hit.collider.transform.position - (Vector3)colCenter;
                    if (Vector2.Dot(toTarget, dir) > 0)
                    {
                        CharacteAgent target = hit.collider.GetComponent<CharacteAgent>();
                        if (target != null && !target.isInvincible)
                        {
                            character.Attack();
                            hasAttacked = true;
                            break; // 只攻击第一个符合条件的目标
                        }
                        else
                        {
                            character.AddReward(character.attackMissAward);
                        }
                    }
                }
            }

            // 可视化射线
            Debug.DrawLine(origin, origin + dir * distance, Color.red);
        }





        // ==== 等待动画播完再切状态 ====
        AnimatorStateInfo info = character.animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Attack") && info.normalizedTime < 1f)
        {
            return; // 动画未播完，保持攻击状态
        }

        // 动画播完，根据输入切换状态
        if (stateMachine.Input.moveDir == 0)
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


