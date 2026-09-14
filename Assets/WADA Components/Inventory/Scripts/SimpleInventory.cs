using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class SimpleInventory : MonoBehaviour
    {
        public
            List<Ressurectable> saved = new();

        public void Save(Ressurectable resurrectable)
        {
            saved.Add( resurrectable );
            resurrectable.gameObject.SetActive( false );
        }
    }
}