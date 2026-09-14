using UnityEngine;

namespace Wada
{
    public interface IDockable
    {
        public GameObject GetGameObject();
        public Rigidbody GetRigidbody();

        public bool Docked { get; set; }
        public bool CloseToInventory { get; set; }
    }
}