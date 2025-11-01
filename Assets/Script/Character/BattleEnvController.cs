using UnityEngine;
using UnityEngine.UI;

public class BattleEnvController : MonoBehaviour
{
    public CharacteAgent agentA;
    public CharacteAgent agentB;
    public Text agentATxt;
    public Text agentBTxt;
    public float winAward = 10000f;
    public float lostAward = -10000f;
    public float finalWinnerBonus = 30000f; // 最终胜利奖励
    public float finalLoserPenalty = -10000f; // 最终失败惩罚

    private int scoreA = 0;
    private int scoreB = 0;

    public void OnAgentDeath(CharacteAgent loser)
    {
        CharacteAgent winner = (loser == agentA) ? agentB : agentA;

        // 计算得分
        if (loser == agentA)
            scoreB++;
        else
            scoreA++;

        // 更新 UI
        agentATxt.text = $"A: {scoreA}";
        agentBTxt.text = $"B: {scoreB}";

        // 当前回合奖励
        winner.AddReward(winAward);
        loser.AddReward(lostAward);

        // 检查是否到了10局
        if (scoreA + scoreB >= 3)
        {
            // 判断总胜利者
            if (scoreA > scoreB)
            {
                agentA.AddReward(finalWinnerBonus);
                agentB.AddReward(finalLoserPenalty);
                Debug.Log("最终胜者：A");
            }
            else if (scoreB > scoreA)
            {
                agentB.AddReward(finalWinnerBonus);
                agentA.AddReward(finalLoserPenalty);
                Debug.Log("最终胜者：B");
            }
            else
            {
                agentA.AddReward(finalWinnerBonus / 2);
                agentB.AddReward(finalWinnerBonus / 2);
                Debug.Log("比赛平局");
            }

            agentA.EndEpisode();
            agentB.EndEpisode();

            scoreA = 0;
            scoreB = 0;
        }
        else
        {
            agentA.OnEpisodeBegin();
            agentB.OnEpisodeBegin();
        }
        agentATxt.text = $"A: {scoreA}";
        agentBTxt.text = $"B: {scoreB}";
    }
}
