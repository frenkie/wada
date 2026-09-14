using UnityEngine;

namespace ImaginaryFriend
{
    public class SelectImageByLanguage : MonoBehaviour
    {
        void OnEnable()
        {
            UpdateImageTexts();
        }

        public void UpdateImageTexts()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        }
    }
}