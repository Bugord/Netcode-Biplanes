using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class NetworkedPlaneController : NetcodeHooks
    {
        private readonly NetworkVariable<Team> team = new NetworkVariable<Team>();

        private readonly Vector3 mirroredPlayerRotation = new Vector3(0, 180, 0);

        public Team Team => team.Value;

        public void Init(Team team)
        {
            this.team.Value = team;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (team.Value == Team.Red) {
                transform.rotation = Quaternion.Euler(mirroredPlayerRotation);
            }
        }
    }
}