using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{    
    float fillPercent = 100;
    [SerializeField] Image bar;

    public void SetFill(float value) {
        fillPercent = value;
        bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillPercent);
    }
}
