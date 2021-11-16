
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Wrappers;
using System.Runtime.ExceptionServices;

public class PhxHoverMainSection : PhxVehicleSection
{
    public PhxHoverMainSection(PhxHover Hover) : base(Hover, 0){} 

    public override void InitManual(EntityClass EC, int StartIndex, string Header, string HeaderValue)
    {
        base.InitManual(EC, StartIndex, "FLYERSECTION", "BODY");
    }

    public PhxPawnController GetController()
    {
        return Occupant == null ? null : Occupant.GetController();
    }
}
