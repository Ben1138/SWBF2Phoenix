using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Region : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public Action<ISWBFInstance> OnEnter;
    public Action<ISWBFInstance> OnLeave;

    // Start is called before the first frame update
    void Start()
    {
        Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        ISWBFInstance inst = other.gameObject.GetComponent<ISWBFInstance>();
        if (inst != null)
        {
            OnEnter?.Invoke(inst);
        }
    }

    void OnTriggerExit(Collider other)
    {
        ISWBFInstance inst = other.gameObject.GetComponent<ISWBFInstance>();
        if (inst != null)
        {
            OnLeave?.Invoke(inst);
        }
    }
}
