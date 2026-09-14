using System;
using UnityEngine;

namespace Wada
{
    public abstract class AbstractInstruction : MonoBehaviour
    {
        public static event Action<AbstractInstruction> OnFinished = delegate { };

        public abstract void FadeIn();

        public virtual void FadeOut()
        {
            Debug.Log( "[AbstractInstruction] FadeOut not implemented" );
            FinishInstruction();
        }

        protected void FinishInstruction()
        {
            OnFinished.Invoke( this );
        }

        public abstract void Go();
    }
}