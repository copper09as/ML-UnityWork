using UnityEngine;

public class CharacterMagic : CharacterState
{
    private bool hasCast = false; // 是否已经发射
    private Vector2 dir;
    private Vector2 origin;

    public override void OnEnter()
    {
        character.state = 5;
        character.animator.Play("Magic");
        hasCast = false;

        // 计算魔法球发射方向
        dir = character.IsFacingRight() ? Vector2.right : Vector2.left;

        // 发射位置参考攻击射线
        float halfWidth = character.col.bounds.extents.x;
        float halfHeight = character.col.bounds.extents.y;
        origin = (Vector2)character.transform.position + dir * halfWidth;
    }

    public override void OnExit()
    {
   }

    public override void Update()
    {
        // 水平移动
        character.rb.velocity = new Vector2(character.speedSky * stateMachine.Input.moveDir, character.rb.velocity.y);

        // 发射魔法球（只发一次）
        if (!hasCast)
        {
            MagicBall ball = GameObject.Instantiate(character.ballPrefab, origin, Quaternion.identity);
            ball.Init(dir, character, character.damage, 20); // Init自己写，包含发射方向、伤害、速度等
            hasCast = true;
        }

        // 等动画播完再切状态
        AnimatorStateInfo info = character.animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Magic") && info.normalizedTime < 1f)
            return;

        // 动画结束后切回之前状态
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
