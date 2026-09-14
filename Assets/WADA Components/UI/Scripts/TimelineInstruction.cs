using UnityEngine;

namespace Wada
{
    public abstract class TimelineInstruction : AbstractInstruction
    {
        [HideInInspector] public InstructionsController Instructions;
    }
}