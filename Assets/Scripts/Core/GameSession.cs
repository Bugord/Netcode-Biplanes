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

        private Plane hostPlane;
        private Plane clientPlane;

        private int hostScore;
        private int clientScore;

        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;

        public void StartSession()
        {
            SpawnPrefabForPlayers();
            
            hostScore = 0;
            clientScore = 0;
        }

        private void SpawnPrefabForPlayers()
        {
            foreach (var playerData in SessionManager.PlayersDataList) {
                if (playerData.IsHost) {
                    hostPlane = Instantiate(planePrefab, serverSpawnPoint.position, Quaternion.identity);
                    hostPlane.GetComponent<NetworkObject>().SpawnWithOwnership(playerData.ClientID);
                    hostPlane.Died += OnHostPlaneCrash;
                }
                else {
                    clientPlane = Instantiate(planePrefab, clientSpawnPoint.position, Quaternion.identity);
                    clientPlane.GetComponent<NetworkObject>().SpawnWithOwnership(playerData.ClientID);
                    clientPlane.Died += OnclientPlanePlaneCrash;
                }
            }
        }

        private void OnclientPlanePlaneCrash(PlaneCrashReason reason)
        {
            OnPlaneCrash(reason, true);
        }

        private void OnHostPlaneCrash(PlaneCrashReason reason)
        {
            OnPlaneCrash(reason, false);
        }

        private void OnPlaneCrash(PlaneCrashReason reason, bool isHostPlane)
        {
            switch (reason) {
                case PlaneCrashReason.Destroyed when isHostPlane:
                    clientScore++;
                    break;
                case PlaneCrashReason.Destroyed:
                    hostScore++;
                    break;
                case PlaneCrashReason.Suicide when isHostPlane:
                    clientScore--;
                    break;
                case PlaneCrashReason.Suicide:
                    hostScore--;
                    break;
            }

            hostScore = Mathf.Max(hostScore, 0);
            clientScore = Mathf.Max(clientScore, 0);
            
            Debug.Log($"Host:Client {hostScore}:{clientScore}");
        }
    }
}