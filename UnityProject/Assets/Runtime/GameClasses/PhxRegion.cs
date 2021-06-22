using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PhxRegion : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public Action<IPhxControlableInstance> OnEnter;
    public Action<IPhxControlableInstance> OnLeave;

    // Start is called before the first frame update
    void Start()
    {
        Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        IPhxControlableInstance inst = other.gameObject.GetComponent<IPhxControlableInstance>();
        if (inst != null)
        {
            OnEnter?.Invoke(inst);
        }
    }

    void OnTriggerExit(Collider other)
    {
        IPhxControlableInstance inst = other.gameObject.GetComponent<IPhxControlableInstance>();
        if (inst != null)
        {
            OnLeave?.Invoke(inst);
        }
    }
}
