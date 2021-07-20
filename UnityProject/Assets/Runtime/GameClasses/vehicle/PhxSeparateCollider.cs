using System;
using UnityEngine;

 
public class PhxSeparateCollider : MonoBehaviour
{

    public PhxVehicle Owner;


    public void OnCollisionEnter(Collision C)
    {
        // do damage, etc
    }
}
