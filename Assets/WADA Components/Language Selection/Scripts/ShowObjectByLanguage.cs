using System;
using UnityEngine;

namespace Wada
{
    [Serializable] public class GameObjectByLanguage : SerializableDictionary<Languages, GameObject> { }    
    
    public class ShowObjectByLanguage : MonoBehaviour
    {
        public GameObjectByLanguage ToShow;

        void Start()
        {
            Languages currentLanguage = GameEngine.GetInstance().ActiveLanguage;
            if (ToShow.ContainsKey(currentLanguage))
            {
                ToShow[currentLanguage].SetActive(true);
            }            
        }
    }
}