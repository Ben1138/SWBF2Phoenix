
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHoverMainSection : PhxVehicleSection
{
    public PhxHoverMainSection(uint[] properties, string[] values,
                            ref int i, PhxHover hv, bool print = false) : 
                            base(properties, values, ref i, hv, 0){} 

    // Returns a Vector3 where x = strafe input, y = drive input, and z = turn input.
    /*
    public Vector4 GetDriverInput()
    {
        if (Occupant == null)
        {
            return Vector3.zero;
        }
        else 
        {
            PhxPawnController Controller = Occupant.GetController();
            return new Vector4(Controller.MoveDirection.x, Controller.MoveDirection.y, Controller.mouseX, Controller.mouseY);
        }
    }
    */

    public PhxPawnController GetController()
    {
        return Occupant == null ? null : Occupant.GetController();
    }
}
