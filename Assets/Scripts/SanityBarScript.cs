using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityBarScript : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void setMaxSanity(int sanity){
        slider.maxValue = sanity;
        slider.value = sanity;

        fill.color = gradient.Evaluate(1f);
    }

    public void setSanity(int sanity){
        slider.value = sanity;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}



