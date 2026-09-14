using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingcircle : MonoBehaviour {

    private RectTransform rectComponent;
    private Image imageComp;

    public float progress = 0f; // 0 - 1

    void Start () {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }

	void Update () {
        imageComp.fillAmount = Mathf.Min( progress, 1 );
    }
}
