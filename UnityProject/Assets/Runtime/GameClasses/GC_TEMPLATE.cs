using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class GC_TEMPLATE : ISWBFInstance<GC_TEMPLATE.ClassProperties>
{
    // ODF properties
    public class ClassProperties : ISWBFClass
    {
        // Names must match ODF property names!
        public Prop<float> MaxHealth = new Prop<float>(100.0f);
    }

    // SWBF instance properties
    // Names must match instance property names!
    Prop<int> Team = new Prop<int>(0);


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
