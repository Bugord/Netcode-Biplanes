using System.Collections;
using System.Collections.Generic;
using Network;
using Pilot;
using ScoreUI;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField]
        private NetworkedPlaneController planePrefab;

        [SerializeField]
        private Transform serverSpawnPoint;

        [SerializeField]
        private Transform clientSpawnPoint;

        [SerializeField]
        private ScoreDisplay scoreDisplay;

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
                var team = i == 0 ? Team.Blue : Team.Red;
                var spawnPosition = team == Team.Blue ? serverSpawnPoint.position : clientSpawnPoint.position;
                
                var plane = Instantiate(planePrefab, spawnPosition, Quaternion.identity);
                var networkedPlaneController = plane.GetComponent<NetworkedPlaneController>();
                networkedPlaneController.Init(team, spawnPosition, edgeDistance);
                
                plane.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerData.ClientID);

                networkedPlaneController.Crashed += OnPlaneCrash;
                networkedPlaneController.EnteredRespawnArea += OnPilotEnteredRespawnArea;
            }
        }

        private void OnPilotEnteredRespawnArea(NetworkedPlaneController plane)
        {
            Debug.Log($"[{nameof(GameSession)}] Plane respawned: {plane.Team}");
            plane.Respawn();
        }

        private void OnPlaneCrash(NetworkedPlaneController plane, PlaneDiedReason reason)
        {
            Debug.Log($"[{nameof(GameSession)}] Plane crashed: {plane.Team} {reason}");
            switch (reason) {
                case PlaneDiedReason.PlaneDestroyed:
                    ChangeScore(plane.Team == Team.Red ? Team.Blue : Team.Red, 1);
                    break;
                case PlaneDiedReason.Suicide:
                    ChangeScore(plane.Team, -1);
                    break;
                case PlaneDiedReason.PilotShot:
                    ChangeScore(plane.Team == Team.Red ? Team.Blue : Team.Red, 2);
                    break;
            }
            
            scoreDisplay.SetScore(teamsScore[Team.Blue], teamsScore[Team.Red]);

            StartCoroutine(RespawnWithDelay(plane, respawnTime));
        }

        private void ChangeScore(Team team, int score)
        {
            teamsScore[team] += score;

            teamsScore[Team.Red] = Mathf.Max(teamsScore[Team.Red], 0);
            teamsScore[Team.Blue] = Mathf.Max(teamsScore[Team.Blue], 0);
        }
        
        public IEnumerator RespawnWithDelay(NetworkedPlaneController plane, float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.Log($"[{nameof(GameSession)}] Plane respawn: {plane.Team}");
            plane.Respawn();
        }
    }
}