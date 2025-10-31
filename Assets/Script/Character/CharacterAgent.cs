using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class CharacteAgent : Agent
{
    private CharacterStateMachine stateMachine;
    public BattleEnvController envController;

    [Header("角色视觉反馈")]
    public Color hitColor = Color.red;  // 被攻击时的颜色
    [SerializeField] private Color originalColor;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Text hpText;

    [Header("角色引用")]
    [SerializeField] private Transform initTransform;
    public Rigidbody2D rb;
    public Animator animator;
    public Collider2D col;
    public LayerMask playerLayer;
    public CharacteAgent enemy;

    [Header("角色属性")]
    public float jumpHeight = 5f;
    public float speedSky = 5f;
    public float speed = 5f;
    public int damage = 10;
    public int maxHp = 100;
    public int hp = 100;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public float dashCooldown = 0.5f;
    public float dashDuration = 0.3f;
    public float dashSpeed = 10f;
    public int fallDamage = 30;
    public float hitFlashDuration = 0.2f;

    [Header("智能体状态（运行时）")]
    [NonSerialized] public int state;
    [NonSerialized] public bool isInvincible = false;
    [NonSerialized] public float dashColdTimer = 0f;
    [NonSerialized] public float attackTimer = 0f;
    [NonSerialized] public bool inGround;
    [NonSerialized] public bool flip = false;
    [NonSerialized] public bool defaultFacingRight = true;
    [NonSerialized] public float dashTimer = 0f;

    // --------------------------- 奖励参数 ---------------------------
    [Header("奖励参数 - 战斗")]
    [SerializeField] private float beAttackAward = 0.01f;      // 被攻击惩罚
    [SerializeField] private float attackAward = 0.5f;         // 攻击命中奖励
    [SerializeField] public float attackMissAward = -0.1f;    // 攻击未命中奖励
    [SerializeField] private float victoryAward = 5f;          // 胜利奖励
    [SerializeField] private float defeatAward = -5f;          // 失败惩罚

    [Header("奖励参数 - 行为")]
    [SerializeField] private float moveAward = 0.002f;         // 移动奖励
    [SerializeField] private float approachEnemyAward = 0.01f; // 靠近敌人奖励
    [SerializeField] private float stayHealthyAward = 0.005f;  // 保持高血量奖励
    [SerializeField] private float fallAward = -1f;            // 掉落惩罚

    private float lastDistanceToEnemy;

    // --------------------------------------------------------------

    private void Start()
    {
        stateMachine = new CharacterStateMachine(this);
    }

    public override void OnEpisodeBegin()
    {
        var moveState = new CharacterMove();
        moveState.InjectStateMachine(stateMachine, this);
        stateMachine.Enter(moveState);

        rb.velocity = Vector2.zero;
        transform.position = initTransform.position;
        hp = maxHp;
        hpText.text = hp.ToString();
        sr.color = originalColor;
        lastDistanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.y);
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);
        sensor.AddObservation(hp / (float)maxHp);
        sensor.AddObservation(state);
        sensor.AddObservation(inGround ? 1f : 0f);
        sensor.AddObservation(flip ? 1f : 0f);
        sensor.AddObservation(attackTimer / attackCooldown);
        sensor.AddObservation(isInvincible ? 1f : 0f);

        sensor.AddObservation(enemy.rb.velocity.x);
        sensor.AddObservation(enemy.rb.velocity.y);
        sensor.AddObservation(enemy.transform.position.x);
        sensor.AddObservation(enemy.transform.position.y);
        sensor.AddObservation(enemy.hp / (float)enemy.maxHp);
        sensor.AddObservation(enemy.state);
        sensor.AddObservation(enemy.inGround ? 1f : 0f);
        sensor.AddObservation(enemy.flip ? 1f : 0f);
    }

    public bool IsFacingRight()
    {
        return defaultFacingRight ? !flip : flip;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        stateMachine.Input.moveDir = actionBuffers.DiscreteActions[0] - 1;
        stateMachine.Input.jump = actionBuffers.DiscreteActions[1] == 1;
        stateMachine.Input.attack = actionBuffers.DiscreteActions[2] == 1;
        stateMachine.Input.dash = actionBuffers.DiscreteActions[3] == 1;

        if (stateMachine.Input.moveDir > 0)
        {
            flip = false;
            sr.flipX = false;
        }
        else if (stateMachine.Input.moveDir < 0)
        {
            flip = true;
            sr.flipX = true;
        }


        if (stateMachine.Input.moveDir != 0)
            AddReward(moveAward);


        float currentDist = Vector2.Distance(transform.position, enemy.transform.position);
        float distanceChange = lastDistanceToEnemy - currentDist;
        AddReward(distanceChange * approachEnemyAward);
        lastDistanceToEnemy = currentDist;


        AddReward((hp / (float)maxHp) * stayHealthyAward);
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!inGround)
        {
            actionMask.SetActionEnabled(branch: 1, actionIndex: 1, isEnabled: false);
        }
        if (dashColdTimer > 0f)
        {
            actionMask.SetActionEnabled(branch: 3, actionIndex: 1, isEnabled: false);
            
        }
        if(attackTimer>0)
        {
            actionMask.SetActionEnabled(branch: 2, actionIndex: 1, isEnabled: false);
        }

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;


        int move = 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            move = 0;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            move = 2;

        int jump = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

        discreteActions[0] = move;
        discreteActions[1] = jump;
    }
    private void Update()
    {
        stateMachine.Update();

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer < 0f) attackTimer = 0f;
        }
        if(dashColdTimer>0f)
        {
            dashColdTimer -= Time.deltaTime;
            if(dashColdTimer<0f)dashColdTimer = 0f;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            inGround = true;
        }

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            inGround = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Dead"))
        {
            transform.position = initTransform.position;
            AddReward(fallAward);
            BeAttacked(fallDamage);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            inGround = false;
        }
    }
    public void Attack()
    {
        if (enemy == null) return;

        enemy.BeAttacked(damage);

        AddReward(attackAward);
    }


    public void BeAttacked(int damage)
    {
        hp -= damage;
        AddReward(-damage * beAttackAward);
        hpText.text = hp.ToString();

        // 播放受击效果
        if (sr != null)
        {
            StopAllCoroutines(); // 停掉上一次的闪烁
            StartCoroutine(FlashHitColor());
        }

        if (hp <= 0)
        {
            envController.OnAgentDeath(this);
        }
    }

    private IEnumerator FlashHitColor()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.color = originalColor;
    }

}
