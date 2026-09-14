using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public abstract class Interactable : MonoBehaviour
    {
        [HideInInspector] public bool Shared;
        public bool Repeatable;
        public float ProbingTime;
        protected bool fired;
        protected bool started;
        protected bool probed;

        public bool Started()
        {
            return started;
        }

        public bool Fired()
        {
            return fired;
        }

        public bool Stopped()
        {
            return started;
        }

        public abstract void StartAction();

        public abstract void StopAction();

        public abstract void Probing();

        public abstract void ProbingStopped();
    }
}