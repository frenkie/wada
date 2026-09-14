using UnityEngine;

namespace Wada
{
    public class WorkshopTester : MonoBehaviour
    {
        public AnimalController[] Animals;

        bool montage;


        public void ToggleTest()
        {
            // go from animation back to montage
            // TODO we have to program the remontage anyways
            if ( montage )
            {
                montage = false;

                foreach ( AnimalController animal in Animals )
                {
                    animal.DisableFreeMovement();
                    animal.SetGodModeReadyToBeReleased( true );
                    animal.Animate();
                }
            }
            else
            {
                montage = true;

                foreach ( AnimalController animal in Animals )
                {
                    animal.DebugWorkshop();
                }
            }
        }
    }
}