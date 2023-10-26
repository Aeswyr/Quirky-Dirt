using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VFXTextColor : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    [SerializeField] private AnimationCurve alphaCurve;

    private float startTime;
    
    void Start() {
        startTime = Time.time;
    }

    void FixedUpdate() {
        var color = text.color;
        color.a = alphaCurve.Evaluate(Time.time - startTime);

        text.color = color;
    }


}
