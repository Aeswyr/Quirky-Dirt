using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float lifetime;
    
    void Start() {
        lifetime += Time.time;
    }

    public void Init(float lifetime) {
        this.lifetime = lifetime;
    }

    void FixedUpdate()
    {
        if (Time.time > lifetime)
            Destroy(gameObject);
    }
}
