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


    public Color hitColor = Color.red;  // 被攻击时的颜色
    
    [SerializeField] private Transform initTransform;
    [SerializeField] private SpriteRenderer sr;
    
    [NonSerialized]public int state;
    [NonSerialized] public bool isInvincible = false;
    [NonSerialized] public float dashColdTimer = 0f; // 冷却计时
    public Rigidbody2D rb;
    [NonSerialized] public float attackTimer = 0f;
    [NonSerialized] public bool inGround;
    [NonSerialized] public bool flip = false;
    [NonSerialized] public bool defaultFacingRight = true;
    [SerializeField] private Text hpText;
    public Animator animator;
    [NonSerialized] public float dashTimer = 0f;
    public CharacteAgent enemy;
    public LayerMask playerLayer;
    public Collider2D col;
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
    public float hitFlashDuration = 0.2f; // 变色持续时间
    [Header("奖励参数")]
    public float beAttackAward;
    public float moveAward;
    public float attackAward;
    public float attackMissAward;
    public float fallAward;
  
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
    }
    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(rb.velocity.x);         // 1
        sensor.AddObservation(rb.velocity.y);         // 2
        sensor.AddObservation(transform.position.x);  // 3
        sensor.AddObservation(transform.position.y);  // 4
        sensor.AddObservation(hp / (float)maxHp);     // 5 
        sensor.AddObservation(state);                 // 6
        sensor.AddObservation(inGround ? 1f : 0f);    // 7
        sensor.AddObservation(flip ? 1f : 0f);        // 8
        sensor.AddObservation(attackTimer / attackCooldown); // 9
        sensor.AddObservation(isInvincible); // 9
        sensor.AddObservation(enemy.rb.velocity.x);         // 10
        sensor.AddObservation(enemy.rb.velocity.y);         // 11
        sensor.AddObservation(enemy.transform.position.x);  // 12
        sensor.AddObservation(enemy.transform.position.y);  // 13
        sensor.AddObservation(enemy.hp / (float)enemy.maxHp); // 14
        sensor.AddObservation(enemy.state);                 // 15
        sensor.AddObservation(enemy.inGround ? 1f : 0f);    // 16
        sensor.AddObservation(enemy.flip ? 1f : 0f);        // 17
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
        {
            AddReward(moveAward);
        }

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
        Color originalColor = sr.color;
        sr.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.color = originalColor;
    }

}
