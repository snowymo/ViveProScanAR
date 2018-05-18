using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    public enum TouchpadSprite
    {
        none = -1,
        left = 0,
        right = 1,
        up = 2,
        down = 3,
        mid = 4,
    }

    public class ViveSR_Experience_Tutorial_TextureSwap : MonoBehaviour
    {
        public bool isDisabled;
        public bool isAnimating;

        int currentTextureNum = 0;

        [SerializeField]
        List<Sprite> sprites;

        Image targetImage;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
        }
        
        public IEnumerator Animate()
        {
            while (true)
            {
                currentTextureNum = (currentTextureNum + 1 == sprites.Count) ? 0 : currentTextureNum + 1;
                targetImage.sprite = sprites[currentTextureNum];

                yield return new WaitForSeconds(0.6f);
            }
        }
    }
}
