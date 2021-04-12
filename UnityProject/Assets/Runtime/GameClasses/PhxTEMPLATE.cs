using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

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
    public PhxProp<int> Team = new PhxProp<int>(0);


    public override void Init()
    {
        // constructor
    }

    public override void BindEvents()
    {
        Team.OnValueChanged += OnTeamChange;
    }

    void OnTeamChange(int oldTeam)
    {
        // this object's Team has been changed, e.g. through Lua
    }

    void Update()
    {

    }
}
