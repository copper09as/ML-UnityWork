using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class CharacteAgent : Agent
{
    public Vector2 arenaCenter = Vector2.zero; // 场地中心位置

    private CharacterStateMachine stateMachine;
    public BattleEnvController envController;
    private int lastMoveDir = 0;
   
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
    [Header("倒地检测参数")]
    public int hitsToDown = 3;          // 连续受击次数阈值
    public float hitComboTime = 2f;     // 时间窗口（秒）

    [NonSerialized] public int hitCount = 0;      // 当前连续受击次数
    [NonSerialized] public float lastHitTime = -10f; // 上一次受击时间

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


    [Header("奖励参数 - 行为")]
    [SerializeField] private float moveAward = 0.002f;         // 移动奖励

    [SerializeField] private float stayHealthyAward = 0.005f;  // 保持高血量奖励
    [SerializeField] private float fallAward = -1f;            // 掉落惩罚
    [SerializeField] private float centerBonusRadius = 2f; // 半径范围
    [SerializeField] private float centerReward = 0.01f;   // 奖励值
    [SerializeField]private float switchDirPenalty = -0.005f;
    [SerializeField] private float attackRangeWardBest = -0.005f;
    [SerializeField] private float attackRangeWardFar = -0.005f;
    [SerializeField] private float attackRangeWardClose= -0.005f;
    [SerializeField] private float enemyDistanceAward = -0.005f;
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
    private float flipCooldown = 0.2f;  // 最小间隔
    private float flipTimer = 0f;

    private void UpdateFlipDirection()
    {
        flipTimer -= Time.deltaTime;
        if (flipTimer > 0f) return; // 冷却时间内不允许翻转

        if (stateMachine.Input.moveDir > 0 && flip)
        {
            flip = false;
            sr.flipX = false;
            flipTimer = flipCooldown;
        }
        else if (stateMachine.Input.moveDir < 0 && !flip)
        {
            flip = true;
            sr.flipX = true;
            flipTimer = flipCooldown;
        }
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

        if (stateMachine.Input.moveDir != 0 && lastMoveDir != 0 && stateMachine.Input.moveDir != lastMoveDir)
            AddReward(switchDirPenalty);
        stateMachine.Input.jump = actionBuffers.DiscreteActions[1] == 1;
        stateMachine.Input.attack = actionBuffers.DiscreteActions[2] == 1;
        stateMachine.Input.dash = actionBuffers.DiscreteActions[3] == 1;

        if (stateMachine.Input.moveDir > 0)
            flip = false;
        else if (stateMachine.Input.moveDir < 0)
            flip = true;

        if (stateMachine.Input.moveDir != 0)
            AddReward(moveAward);

        float currentDist = Vector2.Distance(transform.position, enemy.transform.position);
        float distanceChange = lastDistanceToEnemy - currentDist;
        float optimalMin = attackRange * 0.8f;
        float optimalMax = attackRange * 1.2f;
        float tooClose = attackRange * 0.6f;
        float tooFar = attackRange * 1.4f;

        if ((currentDist < optimalMin && distanceChange > 0) ||
            (currentDist > optimalMax && distanceChange < 0))
        {
            AddReward(enemyDistanceAward * Mathf.Abs(distanceChange));
        }
        if (currentDist >= optimalMin && currentDist <= optimalMax)
        {
            AddReward(attackRangeWardBest);
        }
        else if (currentDist < tooClose)
        {
            AddReward(attackRangeWardClose);
        }
        else if (currentDist > tooFar)
        {
            AddReward(attackRangeWardFar);
        }

        lastDistanceToEnemy = currentDist;

        AddReward((hp / (float)maxHp) * stayHealthyAward);

        // --------- 新增：靠近中心位置奖励 ---------
        float distToCenter = Math.Abs(transform.position.x - arenaCenter.x);//Vector2.Distance(transform.position, arenaCenter);
        if (distToCenter <= centerBonusRadius)
        {
            AddReward(centerReward);
        }
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!inGround)
        {
            actionMask.SetActionEnabled(branch: 1, actionIndex: 1, isEnabled: false);
        }
        if (dashColdTimer > 0f && state == 4)
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
        UpdateFlipDirection();
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

            hp -= fallDamage;
            hpText.text = hp.ToString();
            if (hp <= 0)
            {
                envController.OnAgentDeath(this);
            }
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
        if (isInvincible) return; // 无敌时不处理

        hp -= damage;
        AddReward(damage * beAttackAward);
        hpText.text = hp.ToString();


        float now = Time.time;
        if (now - lastHitTime <= hitComboTime)
        {
            hitCount++;
        }
        else
        {
            hitCount = 1; // 超过时间窗口重新计数
        }
        lastHitTime = now;


        if (hitCount >= hitsToDown)
        {
            hitCount = 0;
            var downState = new CharacterDown();
            downState.InjectStateMachine(stateMachine, this);
            stateMachine.Enter(downState); // 切换到倒地状态
            return;
        }

        // ----- 播放受击反馈 -----
        if (sr != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashHitColor());
        }

        // ----- 判断死亡 -----
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
