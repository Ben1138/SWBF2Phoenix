using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Region : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public Action<GameObject> OnEntered;
    public Action<GameObject> OnExit;

    // Start is called before the first frame update
    void Start()
    {
        Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        OnEntered?.Invoke(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        OnExit?.Invoke(other.gameObject);
    }
}
