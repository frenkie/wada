using UnityEngine;

namespace Wada
{
    public class WadaMath
    {
        public static float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            float fromAbs = from - fromMin;
            float fromMaxAbs = fromMax - fromMin;

            float normal = fromAbs / fromMaxAbs;

            float toMaxAbs = toMax - toMin;
            float toAbs = toMaxAbs * normal;

            float to = toAbs + toMin;

            return to;
        }

        public static Vector2 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius)
        {
            Vector2 randomDirection = (Random.insideUnitCircle * origin).normalized;

            float randomDistance = Random.Range( minRadius, maxRadius );

            Vector2 point = origin + randomDirection * randomDistance;

            return point;
        }
    }
}