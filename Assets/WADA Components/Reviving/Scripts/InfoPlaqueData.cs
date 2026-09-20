using UnityEngine;

namespace Wada
{
    [CreateAssetMenu]
    public class InfoPlaqueData : ScriptableObject
    {
        public string ID;
        public string Name;
        public string Location;
        public string LocationID;
        public string Description;
        public Sprite Image;
        public Sprite Image_NL;
    }
}