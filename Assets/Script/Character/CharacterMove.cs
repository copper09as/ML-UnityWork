using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character2DAgent : Agent
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 180f;
    private Rigidbody2D rb;

    [Header("Target Settings")]
    public Transform targetTransform;
    public Transform initPositionTransform;
    public Transform deadPositionTransform;
    public float reachRadius = 0.5f;
    
    // ¶¯×÷»º´æ
    private float moveInput = 0f;
    private float turnInput = 0f;

    [Header("Debug Settings")]
    public bool debugLogs = true;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public override void OnEpisodeBegin()
    {
        if (this.transform.localPosition.y < deadPositionTransform.position.y)
        {
            this.rb.angularVelocity = 0;
            this.rb.velocity = Vector2.zero;
        }
        targetTransform.localPosition = new Vector2(Random.Range(-8,8), targetTransform.position.y);
        transform.position = initPositionTransform.position;

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rb.velocity.x);
    }
    public float forceMultiplier = 2;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int dir = actionBuffers.DiscreteActions[0]-1;
        rb.velocity = new Vector2(dir * forceMultiplier, rb.velocity.y);
        // Rewards
        float distanceToTarget = Vector2.Distance(this.transform.localPosition,targetTransform.localPosition);
        if (distanceToTarget < 2f)
        {
            SetReward(100.0f);
            Debug.Log("Reward"+distanceToTarget.ToString());
            EndEpisode();
        }
        else if (this.transform.localPosition.y < deadPositionTransform.position.y)
        {
            Debug.Log(transform.localPosition.y.ToString());
            AddReward(-100);
            EndEpisode();
        }
        AddReward(-0.001f);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
    }
}
