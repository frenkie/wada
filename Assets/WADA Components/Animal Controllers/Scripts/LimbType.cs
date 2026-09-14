using UnityEngine;
using System;

namespace Wada
{
    [Flags]
    public enum LimbTypeMask : uint
    {
        Tail = 1,
        Wing = 2,
        BackLeg = 4,
        FrontLeg = 8,
        Head = 16
    }

    public enum LimbType
    {
        None = 0,
        Tail = 1,
        Wing = 2,
        BackLeg = 4,
        FrontLeg = 8,
        Head = 16
    }
}