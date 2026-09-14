using UnityEngine;

namespace Wada
{
    public class DockTrigger : MonoBehaviour
    {
        Inventory inventory;

        void Awake()
        {
            inventory = GetComponentInParent<Inventory>();
        }

        void OnTriggerStay(Collider other)
        {
            inventory.OnTriggerStay( other );
        }

        void OnTriggerExit(Collider other)
        {
            inventory.OnTriggerExit( other );
        }
    }
}