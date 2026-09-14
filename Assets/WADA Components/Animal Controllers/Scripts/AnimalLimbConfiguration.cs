using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    [Serializable]
    public class AnimalLimbConfiguration
    {
        public string Name;
        public LimbType Type;
        public List<GameObject> BodyParts;
        public bool FootSupport;
        public bool FlySupport;
        public GameObject SocketParent;
        public LimbTypeMask AllowedLimbsForSocket;
    }
}