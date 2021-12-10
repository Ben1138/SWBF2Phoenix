using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

// EXAMPLE CLASS
public class PhxTEMPLATE : PhxInstance<PhxTEMPLATE.ClassProperties>
{
    // ODF properties
    public class ClassProperties : PhxClass
    {
        // Names must match ODF property names!
        public PhxProp<float> MaxHealth = new PhxProp<float>(100.0f);
    }

    // SWBF instance properties
    // Names must match instance property names!
    public PhxProp<float> CurrHealth = new PhxProp<float>(100.0f);


    public override void Init()
    {
        // constructor
        // Use this to create required Unity components (like AudioSource, SpotLight, Rigidbody, etc...)
        // and bind property change events
        Team.OnValueChanged += OnTeamChange;
    }

    public override void Destroy()
    {
        // destructor
        // Use this to free resources
    }

    void OnTeamChange(int oldTeam)
    {
        // this object's Team has been changed, e.g. through Lua
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void TickPhysics(float deltaTime)
    {

    }
}
