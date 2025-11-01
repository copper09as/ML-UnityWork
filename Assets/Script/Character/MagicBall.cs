using System.Collections;
using UnityEngine;

public class MagicBall : MonoBehaviour
{
    [Header("魔法球属性")]
    public float speed = 5f;
    public int damage = 10;
    public CharacteAgent owner;

    [Header("引用")]
    public Rigidbody2D rb;
    public Animator animator;
    public string hitAnimTrigger = "Hit";

    [Header("碰撞设置")]
    public LayerMask targetLayer; // 角色所在层
    public LayerMask deadLayer;   // 边界触发器所在层
    public bool isInit;
    private bool hasHit = false;
    private Vector2 direction;

    /// <summary>
    /// 初始化魔法球
    /// </summary>
    public void Init(Vector2 dir, CharacteAgent ownerAgent, int dmg, float spd)
    {
        direction = dir.normalized;
        owner = ownerAgent;
        damage = dmg;
        speed = spd;
        isInit = true;
    }

    private void Update()
    {
        if (hasHit) return;

        // 移动魔法球
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
        if(transform.position.x<-16.58 ||transform.position.x>5.8)
        {
            owner.AddReward(owner.magicMissAward);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isInit) return;
        if (hasHit) return;

        // 只对角色造成伤害
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            CharacteAgent target = collision.gameObject.GetComponent<CharacteAgent>();
            if (target != null && target != owner && !target.isInvincible)
            {
                owner.AddReward(owner.magicAward);
                target.BeAttacked(damage);
                
                Hit();
            }
        }
    }


    

    private void Hit()
    {
        hasHit = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // 停止物理移动

        if (animator != null)
        {
            animator.Play("Boom"); // 播放碰撞动画
        }

        Destroy(gameObject, 0.5f); // 动画播放完成后销毁
    }
}
