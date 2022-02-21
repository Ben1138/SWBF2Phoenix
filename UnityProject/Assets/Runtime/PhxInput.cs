using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PhxInputControl : ushort
{
    None = 0,

    // Axes
    Thrust = 1 << 0,
    View = 1 << 1,

    // Buttons
    Fire = 1 << 2,
    FireSecondary = 1 << 3,
    Jump = 1 << 4,
    Sprint = 1 << 5,
    Crouch = 1 << 6,
    Reload = 1 << 7,

    FireBoth = Fire | FireSecondary,

    All = 0xffff
}

public enum PhxInputButtonAction
{
    None, 
    Changed, 
    Tab, 
    DoubleTab, 
    Hold, 
    Down
}

public struct PhxInputEvent
{
    public PhxInputControl Input;
    public PhxInputButtonAction Action;
}

public class PhxInput
{
    public static bool IsAxis(PhxInputControl input)
    {
        return
            input == PhxInputControl.Thrust ||
            input == PhxInputControl.View;
    }

    public static bool IsButton(PhxInputControl input)
    {
        return !IsAxis(input) || input == PhxInputControl.FireBoth;
    }


}