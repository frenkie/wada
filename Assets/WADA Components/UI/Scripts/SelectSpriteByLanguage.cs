using System;
using UnityEngine;

namespace Wada
{
    [Serializable] public class SpriteByLanguage : SerializableDictionary<Languages, Sprite> { }
    
    public class SelectSpriteByLanguage : MonoBehaviour
    {
        public SpriteByLanguage Sprites;

        void OnEnable()
        {
            UpdateImageTexts();
        }

        public void UpdateImageTexts()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            Languages currentLanguage = GameEngine.GetInstance().ActiveLanguage;
            if (Sprites.ContainsKey(currentLanguage))
            {
                renderer.sprite = Sprites[currentLanguage];
            }
            // else the default it already has
        }        
    }
}