using UnityEngine;

public class BattleEnvController : MonoBehaviour
{
    public CharacteAgent agentA;
    public CharacteAgent agentB;

    public void OnAgentDeath(CharacteAgent loser)
    {
        CharacteAgent winner = (loser == agentA) ? agentB : agentA;

        winner.AddReward(10000f);
        loser.AddReward(-10000f);

        winner.EndEpisode();
        loser.EndEpisode();
    }
}
