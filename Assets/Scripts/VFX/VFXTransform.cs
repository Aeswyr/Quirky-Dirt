using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTransform : MonoBehaviour
{
    [SerializeField] private AnimationCurve xCurve;
    [SerializeField] private AnimationCurve yCurve;
    [SerializeField] private AnimationCurve scaleCurve;
    
    private float startTime;
    private Vector2 startPos;
    void Start() {
        startTime = Time.time;
        startPos = new(transform.localPosition.x, transform.localPosition.y);
    }

    void FixedUpdate()
    {
        float x = xCurve == null ? 0 : xCurve.Evaluate(Time.time - startTime);
        float y = yCurve == null ? 0 : yCurve.Evaluate(Time.time - startTime);

        transform.localPosition = startPos + new Vector2(x, y);

        
    }
}
