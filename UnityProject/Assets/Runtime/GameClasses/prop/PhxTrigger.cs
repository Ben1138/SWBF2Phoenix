
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Enums;
using System.Runtime.ExceptionServices;


public class PhxTrigger : MonoBehaviour
{
    Action EnterCallBack;
    Action ExitCallBack;


    public void Init(float TriggerRadius, Action EnterCallBack_, Action ExitCallBack_ = null)
    {
        SphereCollider TriggerCollider = gameObject.AddComponent<SphereCollider>();
        TriggerCollider.radius = TriggerRadius;
        TriggerCollider.isTrigger = true;

        EnterCallBack = EnterCallBack_;
        ExitCallBack = ExitCallBack_;
    }

    public void OnTriggerEnter(Collider other)
    {
        EnterCallBack?.Invoke();
    }

    public void OnTriggerExit(Collider other)
    {
        ExitCallBack?.Invoke();
    }
}
