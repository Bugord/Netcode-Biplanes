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

        public void SetDefaultSprite()
        {
            SetDefaultSpriteRpc();
        }

        public void SetFlySprite()
        {
            SetFlySpriteRpc();
        }

        public void SetDestroyedSprite()
        {
            SetDestroyedSpriteRpc();
        }

        [Rpc(SendTo.NotServer)]
        private void SetDefaultSpriteRpc()
        {
            spriteRenderer.sprite = team == Team.Blue ? idleSpriteBlue : idleSpriteRed;
        }

        [Rpc(SendTo.NotServer)]
        private void SetFlySpriteRpc()
        {
            spriteRenderer.sprite = team == Team.Blue ? flySpireBlue : flySpireRed;
        }

        [Rpc(SendTo.NotServer)]
        private void SetDestroyedSpriteRpc()
        {
            spriteRenderer.sprite = null;
        }
    }
}