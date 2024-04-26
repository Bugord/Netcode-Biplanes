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

        private readonly Dictionary<Team, int> teamsScore = new Dictionary<Team, int>();

        private Plane hostPlane;
        private Plane clientPlane;

        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;

        public void StartSession()
        {
            SpawnPrefabForPlayers();

            teamsScore[Team.Red] = 0;
            teamsScore[Team.Blue] = 0;
        }

        private void SpawnPrefabForPlayers()
        {
            foreach (var playerData in SessionManager.PlayersDataList) {
                hostPlane = Instantiate(planePrefab);   

                hostPlane.GetComponent<NetworkObject>().SpawnWithOwnership(playerData.ClientID);
                hostPlane.InitRpc(playerData.IsHost ? Team.Red : Team.Blue, playerData.IsHost ? serverSpawnPoint.position : clientSpawnPoint.position);

                hostPlane.Crashed += OnPlaneCrash;
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
        }

        private void ChangeScore(Team team, int score)
        {
            teamsScore[team] += score;

            teamsScore[Team.Red] = Mathf.Max(teamsScore[Team.Red], 0);
            teamsScore[Team.Blue] = Mathf.Max(teamsScore[Team.Blue], 0);

            Debug.Log($"Host:Client {teamsScore[Team.Red]}:{teamsScore[Team.Blue]}");
        }
    }
}