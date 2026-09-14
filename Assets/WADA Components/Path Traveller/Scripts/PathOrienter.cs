using UnityEngine;
using PathCreation;

namespace Wada
{
    public class PathOrienter : MonoBehaviour
    {
        public PathCreator Path;
        public float DistanceTravelled = 0f;

        void Awake()
        {
            DistanceTravelled = 0f;
        }

        void Update()
        {
            Vector3 newPosition = GetPositionAtCurrentDistanceTravelled();
            transform.position = newPosition;
        }

        public Vector3 GetPositionAtCurrentDistanceTravelled()
        {
            return Path.path.GetPointAtDistance( DistanceTravelled, EndOfPathInstruction.Stop );
        }
    }
}