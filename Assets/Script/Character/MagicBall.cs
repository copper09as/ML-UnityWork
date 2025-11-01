using System.Collections;
using UnityEngine;

public class MagicBall : MonoBehaviour
{
    [Header("ħ��������")]
    public float speed = 5f;
    public int damage = 10;
    public CharacteAgent owner;

    [Header("����")]
    public Rigidbody2D rb;
    public Animator animator;
    public string hitAnimTrigger = "Hit";

    [Header("��ײ����")]
    public LayerMask targetLayer; // ��ɫ���ڲ�
    public LayerMask deadLayer;   // �߽紥�������ڲ�
    public bool isInit;
    private bool hasHit = false;
    private Vector2 direction;

    /// <summary>
    /// ��ʼ��ħ����
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

        // �ƶ�ħ����
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

        // ֻ�Խ�ɫ����˺�
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
        rb.isKinematic = true; // ֹͣ�����ƶ�

        if (animator != null)
        {
            animator.Play("Boom"); // ������ײ����
        }

        Destroy(gameObject, 0.5f); // ����������ɺ�����
    }
}
