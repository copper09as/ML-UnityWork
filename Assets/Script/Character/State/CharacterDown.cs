using UnityEngine;

public class CharacterDown : CharacterState
{
    private float downTimer;
    private float downDuration = 2f;

    private Vector2 originalSize;
    private Vector2 originalOffset;

    public override void OnEnter()
    {
        character.animator.Play("Down");
        downTimer = downDuration;
        character.rb.velocity = Vector2.zero;
        character.isInvincible = true;
        character.rb.bodyType = RigidbodyType2D.Static;
        character.state = 4;
        if (character.col is CapsuleCollider2D capsule)
        {
            // 记录原始值
            originalSize = capsule.size;
            originalOffset = capsule.offset;

            // 缩小高度
            capsule.size = new Vector2(originalSize.x, originalSize.y * 0.5f);
            // ✅ 向上移动，使动画更贴地
            capsule.offset = new Vector2(originalOffset.x, originalOffset.y + originalSize.y * 0.35f);
        }
    }

    public override void Update()
    {
        downTimer -= Time.deltaTime;
        if (downTimer <= 0f)
        {
            character.isInvincible = false;

            if (character.col is CapsuleCollider2D capsule)
            {
                capsule.size = originalSize;
                capsule.offset = originalOffset;
            }


            var moveState = new CharacterMove();
            moveState.InjectStateMachine(stateMachine, character);
            stateMachine.Enter(moveState);
        }
    }

    public override void OnExit()
    {
        if (character.col is CapsuleCollider2D capsule)
        {
            capsule.size = originalSize;
            capsule.offset = originalOffset;
        }
        character.rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
