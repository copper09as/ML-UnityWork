using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine
{
    private CharacterState currentState;
    public CharacterState lastState;
    public CharacterInput Input = new();
    public CharacteAgent Agent;
    public CharacterStateMachine(CharacteAgent agent)
    {
        this.Agent = agent;
    }

    public void Enter(CharacterState state)
    {
        if (currentState!=null)
        {
            currentState.OnExit();
            lastState = currentState;
        }
        currentState = state;
        currentState.OnEnter();
    }
    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
        if(Input.dash && Agent.dashColdTimer<=0f && Agent.state != 4)
        {
            Agent.dashColdTimer = Agent.dashCooldown;
            var dashState = new CharacterDash();
            dashState.InjectStateMachine(this, Agent);
            Enter(dashState);
        }
    }
}
