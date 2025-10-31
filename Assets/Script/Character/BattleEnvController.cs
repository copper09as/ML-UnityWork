using UnityEngine;

public class BattleEnvController : MonoBehaviour
{
    public CharacteAgent agentA;
    public CharacteAgent agentB;

    public void OnAgentDeath(CharacteAgent loser)
    {
        CharacteAgent winner = (loser == agentA) ? agentB : agentA;

        winner.AddReward(1000f);
        loser.AddReward(-1000f);

        winner.EndEpisode();
        loser.EndEpisode();
    }
}
