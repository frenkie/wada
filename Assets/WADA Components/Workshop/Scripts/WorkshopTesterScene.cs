using UnityEngine;

namespace Wada
{
    public class WorkshopTesterScene : MonoBehaviour
    {
        public void EnableActiveAnimals()
        {
            foreach ( AnimalController animalController in FindObjectsOfType<AnimalController>() )
            {
                if ( animalController.gameObject.activeInHierarchy )
                {
                    animalController.DebugWorkshop();
                }
            }
        }
    }
}