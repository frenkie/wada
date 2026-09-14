using UnityEngine;

namespace Wada
{
    public class GuidingDuckPassOn : MonoBehaviour
    {
        public GuidingDuck Guide;

        public void OnAnimationEnd()
        {
            Guide.OnAnimationEnd();
        }
    }
}