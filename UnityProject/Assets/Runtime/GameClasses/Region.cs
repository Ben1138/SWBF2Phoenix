using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Region : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public Action<GameObject> OnEnter;
    public Action<GameObject> OnLeave;

    // Start is called before the first frame update
    void Start()
    {
        Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        OnLeave?.Invoke(other.gameObject);
    }
}
