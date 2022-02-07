using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PhxInputControl : ushort
{
    None = 0,

    Thrust = 1 << 0,
    Fire = 1 << 1,
    FireSecondary = 1 << 2,
    Jump = 1 << 3,
    Sprint = 1 << 4,
    Crouch = 1 << 5,
    Reload = 1 << 6,

    All = 0xffff
}

[Flags]
// Subset of PhxInputControl
public enum PhxInputButtons : ushort
{
    None = 0,

    Fire = 1 << 1,
    FireSecondary = 1 << 2,
    Jump = 1 << 3,
    Sprint = 1 << 4,
    Crouch = 1 << 5,
    Reload = 1 << 6,

    FireBoth = Fire | FireSecondary,
}

public enum PhxInputButtonAction
{
    None, Tab, DoubleTab, Hold, Down
}

public struct PhxInput
{
    public PhxInputButtonAction 
}

