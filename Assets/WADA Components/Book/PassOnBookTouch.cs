using UnityEngine;

namespace Wada
{
    public enum WadaBookPageNav
    {
        Previous,
        Next
    }

    public class PassOnBookTouch : MonoBehaviour
    {
        public WadaBook Book;
        public WadaBookPageNav NavType;

        bool isOpening = false;

        void EnablePress()
        {
            isOpening = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if ( other.gameObject.tag == "Hand" && !isOpening )
            {
                isOpening = true;

                switch ( NavType )
                {
                    case WadaBookPageNav.Previous:
                        Book.PreviousPage();
                        break;

                    case WadaBookPageNav.Next:
                        Book.NextPage();
                        break;
                }

                Invoke( "EnablePress", .5f );
            }
        }
    }
}