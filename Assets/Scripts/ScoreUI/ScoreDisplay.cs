using Unity.Netcode;
using UnityEngine;

namespace ScoreUI
{
    public class ScoreDisplay : NetworkBehaviour
    {
        [SerializeField]
        private ScoreNumberDisplay blueScoreNumberDisplay;

        [SerializeField]
        private ScoreNumberDisplay redScoreNumberDisplay;
        
        public void SetScore(int blueScore, int redScore)
        {
            SetScoreRpc(blueScore, redScore);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SetScoreRpc(int blueScore, int redScore)
        {
            blueScoreNumberDisplay.SetNumber(blueScore);
            redScoreNumberDisplay.SetNumber(redScore);
        }
    }
}