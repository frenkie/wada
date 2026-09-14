using UnityEngine;

namespace Wada
{
    public interface ITouchReceiver
    {
        public void OnTouch(WadaHand hand);
        public void OnLeaveTouch(WadaHand hand);
    }
}