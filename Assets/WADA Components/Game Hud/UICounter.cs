using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wada
{
    public class UICounter : MonoBehaviour
    {
        public RectTransform NormalLeft;
        public RectTransform BonusLeft;
        public TextMeshProUGUI NormalTime;
        public TextMeshProUGUI BonusTime;

        void Update()
        {
            Clock instance = Clock.GetInstance();
            if ( instance != null )
            {
                NormalLeft.localScale = new Vector3(
                    instance.GetNormalProgress(),
                    1,
                    1
                );

                BonusLeft.localScale = new Vector3(
                    instance.GetBonusProgress(),
                    1,
                    1
                );

                NormalTime.text = instance.GetNormalTime() + " / " + instance.TotalTimeInSeconds;
                BonusTime.text = instance.GetBonusTime() + " / " + instance.GetBonusMaxTime();
            }
        }
    }
}