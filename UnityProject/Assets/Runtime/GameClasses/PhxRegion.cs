using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PhxRegion : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public Action<PhxInstance> OnEnter;
    public Action<PhxInstance> OnLeave;

    // Start is called before the first frame update
    void Start()
    {
        Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        PhxInstance inst = other.gameObject.GetComponent<PhxInstance>();
        if (inst != null)
        {
            OnEnter?.Invoke(inst);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PhxInstance inst = other.gameObject.GetComponent<PhxInstance>();
        if (inst != null)
        {
            OnLeave?.Invoke(inst);
        }
    }
}
