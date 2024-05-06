using System;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class PlaneSpriteController : NetworkBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Sprite idleSpriteBlue;

        [SerializeField]
        private Sprite flySpireBlue;
        
        [SerializeField]
        private Sprite idleSpriteRed;

        [SerializeField]
        private Sprite flySpireRed;

        private Team team;

        public void Init(Team team)
        {
            this.team = team;
        }

        [Rpc(SendTo.NotServer)]
        public void SetDefaultSpriteRpc()
        {
            spriteRenderer.sprite = team == Team.Blue ? idleSpriteBlue : idleSpriteRed;
        }

        [Rpc(SendTo.NotServer)]
        public void SetFlySpriteRpc()
        {
            spriteRenderer.sprite = team == Team.Blue ? flySpireBlue : flySpireRed;
        }

        [Rpc(SendTo.NotServer)]
        public void SetDestroyedSpriteRpc()
        {
            spriteRenderer.sprite = null;
        }
    }
}