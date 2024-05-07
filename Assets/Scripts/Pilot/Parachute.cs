using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pilot
{
    public class Parachute : MonoBehaviour
    {
        [SerializeField]
        private PilotParachuteController pilotParachuteController;
        
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private List<Sprite> parachuteSprites;

        public void SetDirection(int direction)
        {
            var spriteIndex = direction + 1;
            spriteRenderer.sprite = parachuteSprites[spriteIndex];
        }

        public void DestroyParachute()
        {
            pilotParachuteController.HideParachute();
        }
    }
}