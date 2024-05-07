using System.Collections.Generic;
using UnityEngine;

namespace ScoreUI
{
    public class ScoreNumberDisplay : MonoBehaviour
    {
        [SerializeField]
        private List<Sprite> numberSprites;

        [SerializeField]
        private List<SpriteRenderer> numberRenderers;

        public void SetNumber(int number)
        {
            if (number is < 0 or > 99) {
                return;
            }

            var stringNumber = number.ToString("D" + 2);
            
            for (var i = 0; i < numberRenderers.Count; i++) {
                var digit = stringNumber[i] - '0';
                numberRenderers[i].sprite = numberSprites[digit];
            }
        }
    }
}