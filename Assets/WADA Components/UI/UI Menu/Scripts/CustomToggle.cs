using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ImaginaryFriend
{
    public class CustomToggle : MonoBehaviour
    {
        [HideInInspector] public bool _isSelected = false;
        private bool _curState = false;


        public bool IsChecked = true;

        [Header("To Assign")]
        public GameObject[] Content;
        public TextMeshProUGUI ContentDescription;
        public GameObject Checkmark;
        public Image SelectionImage;


        void Start()
        {
            UpdateGameObjects(IsChecked);
        }

        void Update()
        {


            if (_isSelected)
            {
                SelectionImage.color = new Color(1, 0, 1, 1);
            }

            else
            {
                SelectionImage.color = new Color(1, 0, 1, 0);
            }
        }

        public void UpdateGameObjects(bool TargetState)
        {

            foreach (GameObject i in Content)
            {
                i.SetActive(TargetState);
            }
            Checkmark.SetActive(TargetState);

            IsChecked = TargetState;
            Debug.Log("Setted "+ ContentDescription.text + " to " + TargetState);
        }
    }
}
