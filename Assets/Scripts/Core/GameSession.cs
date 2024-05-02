using System.Collections;
using System.Collections.Generic;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField]
        private Plane planePrefab;

        [SerializeField]
        private Transform serverSpawnPoint;

        [SerializeField]
        private Transform clientSpawnPoint;

        [SerializeField]
        private float edgeDistance = 14.5f;

        [SerializeField]
        private float respawnTime = 1f;

        private readonly Dictionary<Team, int> teamsScore = new Dictionary<Team, int>();

        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;
        
        public void StartSession()
        {
            Debug.Log($"[{nameof(GameSession)}] Starting session");
            SpawnPrefabForPlayers();

            teamsScore[Team.Red] = 0;
            teamsScore[Team.Blue] = 0;
        }

        private void SpawnPrefabForPlayers()
        {
            for (var i = 0; i < SessionManager.PlayersDataList.Count; i++) {
                var playerData = SessionManager.PlayersDataList[i];
                Debug.Log($"[{nameof(GameSession)}] Spawning object for player {playerData.PlayerName}");

                var plane = Instantiate(planePrefab);

                plane.GetComponent<NetworkObject>().SpawnWithOwnership(playerData.ClientID);
                plane.InitRpc((Team)i, i == 0 ? serverSpawnPoint.position : clientSpawnPoint.position, edgeDistance);

                plane.Crashed += OnPlaneCrash;
            }
        }

        private void OnPlaneCrash(Plane plane, PlaneCrashReason reason)
        {
            switch (reason) {
                case PlaneCrashReason.Destroyed:
                    ChangeScore(plane.Team == Team.Red ? Team.Blue : Team.Red, 1);
                    break;
                case PlaneCrashReason.Suicide:
                    ChangeScore(plane.Team, -1);
                    break;
            }

            StartCoroutine(RespawnWithDelay(plane, respawnTime));
        }

        private void ChangeScore(Team team, int score)
        {
            teamsScore[team] += score;

            teamsScore[Team.Red] = Mathf.Max(teamsScore[Team.Red], 0);
            teamsScore[Team.Blue] = Mathf.Max(teamsScore[Team.Blue], 0);

            Debug.Log($"Red:Blue {teamsScore[Team.Red]}:{teamsScore[Team.Blue]}");
        }
        
        public IEnumerator RespawnWithDelay(Plane plane, float delay)
        {
            yield return new WaitForSeconds(delay);
            plane.Respawn();
        }
    }
}