using System;
using UnityEngine;

public class Flake : MonoBehaviour
{
    public float lifetime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.mass = 0.01f;
        }
    }
}
