using System;
using System.Collections.Generic;
using UnityEngine;


[Flags]
public enum PhxInput : ulong
{
    None = 0,

    //
    // Buttons
    //

    // Buttons - UI
    UI_Pause  = 1 << 0, // a.k.a. Esc
    UI_Enter  = 1 << 1,
    UI_Back   = 1 << 2,

    // Buttons - Soldier
    Soldier_FirePrimary          = 1 << 3,
    Soldier_FireSecondary        = 1 << 4,
    Soldier_FireBoth             = Soldier_FirePrimary | Soldier_FireSecondary,
    Soldier_Jump                 = 1 << 5,
    Soldier_Sprint               = 1 << 6,
    Soldier_Crouch               = 1 << 7,
    Soldier_Roll                 = 1 << 8,
    Soldier_Reload               = 1 << 9,
    Soldier_Enter                = 1 << 10,
    Soldier_NextPrimaryWeapon    = 1 << 11,
    Soldier_NextSecondaryWeapon  = 1 << 12,

    // Buttons - Vehicle
    Vehicle_FirePrimary    = 1 << 13,
    Vehicle_FireSecondary  = 1 << 14,
    Vehicle_SwitchSeat     = 1 << 15,
    Vehicle_Exit           = 1 << 16,

    // Buttons - Flyer
    Flyer_FirePrimary    = 1 << 17,
    Flyer_FireSecondary  = 1 << 18,
    Flyer_SwitchSeat     = 1 << 19,
    Flyer_StartLand      = 1 << 20,
    Flyer_Exit           = 1 << 21,


    //
    // Axes
    //

    // Axes - Soldier
    Soldier_Thrust  = 1 << 22,
    Soldier_View    = 1 << 23,

    // Axes - Vehicle
    Vehicle_Thrust  = 1 << 24,
    Vehicle_View    = 1 << 25,

    // Axes - Flyer
    Flyer_Thrust  = 1 << 26,
    Flyer_View    = 1 << 27,


    All = 0xffffffffffffffff
}

public struct PhxButtonEvents       // Description:                                                        |  Available:
{                                   // ---------------------------------------------------------------------------------------                               
    public PhxInput Down;           // As long as the button is down                                       |  multiple frames
    public PhxInput Changed;        // When value changed                                                  |  one frame
    public PhxInput Pressed;        // When button was initially pressed down                              |  one frame
    public PhxInput Released;       // When button is back up                                              |  one frame
    public PhxInput Tab;            // When the button was held for a short time and then released         |  one frame
    //public PhxInput DoubleTab;    // When the button was tapped twice within a short time period         |  one frame
    public PhxInput Hold;           // When the button was hold for a slightly longer time, then released  |  one frame


    // I hope these get inlined...
    public bool IsDown(PhxInput input)
    {
        return (Down & input) == input;
    }

    public bool IsChanged(PhxInput input)
    {
        return (Changed & input) == input;
    }

    public bool IsPressed(PhxInput input)
    {
        return (Pressed & input) == input;
    }

    public bool IsReleased(PhxInput input)
    {
        return (Released & input) == input;
    }

    public bool IsTab(PhxInput input)
    {
        return (Tab & input) == input;
    }

    public bool IsHold(PhxInput input)
    {
        return (Hold & input) == input;
    }
}

public struct PhxInputAxis
{
    public float  Value;
    public float  Scale; // Invert Axis -> negative scale
    public float  Deadzone;
}

public struct PhxInputAxis2D
{
    public PhxInputAxis X;
    public PhxInputAxis Y;

    public Vector2 GetValues()
    {
        return new Vector2(X.Value, Y.Value);
    }
}

public struct PhxInputAxesGroup
{
    public PhxInputAxis2D Thrust;
    public PhxInputAxis2D View;

    public static PhxInputAxesGroup New()
    {
        PhxInputAxesGroup g = new PhxInputAxesGroup();
        PhxInputAxis ax = new PhxInputAxis { Value = 0f, Scale = 1f, Deadzone = 0.05f };
        g.Thrust = new PhxInputAxis2D { X = ax, Y = ax };
        g.View = new PhxInputAxis2D { X = ax, Y = ax };
        return g;
    }
}

// NOTE: Axes contain the delta changes for the current frame, NOT the absolute values!
public class PhxPlayerInput
{
    PhxButtonTime[]    PrevFrameButtons;
    PhxButtonEvents    ButtonFrameEvents;
    PhxInputAxesGroup  Soldier_AxesDelta;
    PhxInputAxesGroup  Vehicle_AxesDelta;
    PhxInputAxesGroup  Flyer_AxesDelta;

    static readonly string[] UnityInputNames = new string[22]
    {
        "Cancel",               // UI Pause
        "Submit",               // UI Enter
        "Cancel",               // UI Back
                                
        "Fire1",                // Soldier FirePrimary  
        "Fire2",                // Soldier FireSecondary
        "Jump",                 // Soldier Jump  
        "Sprint",               // Soldier Sprint
        "Crouch",               // Soldier Crouch
        "Roll",                 // Soldier Roll  
        "Reload",               // Soldier Reload
        "EnterVehicle",         // Soldier Enter 
        "NextPrimaryWeapon",    // Soldier NextPrimaryWeapon  
        "NextSecondaryWeapon",  // Soldier NextSecondaryWeapon

        "Fire1",                // Vehicle FirePrimary  
        "Fire2",                // Vehicle FireSecondary
        "Crouch",               // Vehicle SwitchSeat   
        "EnterVehicle",         // Vehicle Exit         
                                
        "Fire1",                // Flyer FirePrimary  
        "Fire2",                // Flyer FireSecondary
        "Crouch",               // Flyer SwitchSeat   
        "Jump",                 // Flyer StartLand    
        "EnterVehicle",         // Flyer Exit         
    };

    public PhxPlayerInput()
    {
        PrevFrameButtons = new PhxButtonTime[UnityInputNames.Length];

        Soldier_AxesDelta = PhxInputAxesGroup.New();
        Vehicle_AxesDelta = PhxInputAxesGroup.New();
        Flyer_AxesDelta   = PhxInputAxesGroup.New();

        Soldier_AxesDelta.View.Y.Scale = -1f;
    }

    public PhxButtonEvents GetButtonEvents()
    {
        return ButtonFrameEvents;
    }

    public PhxInputAxesGroup GetSoldierAxesDelta()
    {
        return Soldier_AxesDelta;
    }
    public PhxInputAxesGroup GetVehicleAxesDelta()
    {
        return Vehicle_AxesDelta;
    }
    public PhxInputAxesGroup GetFlyerAxesDelta()
    {
        return Flyer_AxesDelta;
    }

    public unsafe void Tick(float deltaTime)
    {
        //
        // Buttons
        //
        bool* buttons = stackalloc bool[UnityInputNames.Length];

        ulong down = 0;
        ulong changed = 0;
        ulong pressed = 0;
        ulong released = 0;
        ulong tab = 0;
        //ulong doubleTab = 0;
        ulong hold = 0;

        for (int i = 0; i < UnityInputNames.Length; i++)
        {
            buttons[i]  = Input.GetButton(UnityInputNames[i]);

            down       |= ( buttons[i] ? 1UL : 0UL) << i;
            changed    |= ( buttons[i] !=  PrevFrameButtons[i].Down ? 1UL : 0UL) << i;
            pressed    |= ( buttons[i] && !PrevFrameButtons[i].Down ? 1UL : 0UL) << i;
            released   |= (!buttons[i] &&  PrevFrameButtons[i].Down ? 1UL : 0UL) << i;
            tab        |= (!buttons[i] &&  PrevFrameButtons[i].Down && PrevFrameButtons[i].Time <= 0.2f ? 1UL : 0UL) << i;
            hold       |= (!buttons[i] &&  PrevFrameButtons[i].Down && PrevFrameButtons[i].Time >  0.2f ? 1UL : 0UL) << i;

            PrevFrameButtons[i].Down = buttons[i];
            if (PrevFrameButtons[i].Down)
            {
                PrevFrameButtons[i].Time += deltaTime;
            }
            else
            {
                PrevFrameButtons[i].Time = 0f;
            }
        }

        ButtonFrameEvents.Down     = (PhxInput)down;
        ButtonFrameEvents.Changed  = (PhxInput)changed;
        ButtonFrameEvents.Pressed  = (PhxInput)pressed;
        ButtonFrameEvents.Released = (PhxInput)released;
        ButtonFrameEvents.Tab      = (PhxInput)tab;
        ButtonFrameEvents.Hold     = (PhxInput)hold;

        //string logDown = "";
        //string logChanged = "";
        //string logPressed = "";
        //string logReleased = "";
        //string logTab = "";
        //string logHold = "";

        //if (tab != 0)
        //{

        //    string[] names  = Enum.GetNames(typeof(PhxInput));
        //    ulong[]  values = (ulong[])Enum.GetValues(typeof(PhxInput));

        //    // Start at 1, skip 'None'
        //    for (int i = 1; i < names.Length; i++)
        //    {
        //        if ((down & values[i]) == values[i])
        //        {
        //            logDown += $"{names[i]}, ";
        //        }
        //        if ((changed & values[i]) == values[i])
        //        {
        //            logChanged += $"{names[i]}, ";
        //        }
        //        if ((pressed & values[i]) == values[i])
        //        {
        //            logPressed += $"{names[i]}, ";
        //        }
        //        if ((released & values[i]) == values[i])
        //        {
        //            logReleased += $"{names[i]}, ";
        //        }
        //        if ((tab & values[i]) == values[i])
        //        {
        //            logTab += $"{names[i]}, ";
        //        }
        //        if ((hold & values[i]) == values[i])
        //        {
        //            logHold += $"{names[i]}, ";
        //        }
        //    }

        //    if (logDown.Length > 0) Debug.Log($"Down: {logDown}");
        //    if (logChanged.Length > 0) Debug.Log($"Changed: {logChanged}");
        //    if (logPressed.Length > 0) Debug.Log($"Pressed: {logPressed}");
        //    if (logReleased.Length > 0) Debug.Log($"Released: {logReleased}");
        //    if (logTab.Length > 0) Debug.Log($"Tab: {logTab}");
        //    if (logHold.Length > 0) Debug.Log($"Hold: {logHold}");
        //}

        //
        // Axes
        //
        AddAxisDelta(ref Soldier_AxesDelta.Thrust.X, Input.GetAxis("Horizontal"));
        AddAxisDelta(ref Soldier_AxesDelta.Thrust.Y, Input.GetAxis("Vertical"));
        AddAxisDelta(ref Soldier_AxesDelta.View.X,   Input.GetAxis("Mouse X"));
        AddAxisDelta(ref Soldier_AxesDelta.View.Y,   Input.GetAxis("Mouse Y"));

        AddAxisDelta(ref Vehicle_AxesDelta.Thrust.X, Input.GetAxis("Horizontal"));
        AddAxisDelta(ref Vehicle_AxesDelta.Thrust.Y, Input.GetAxis("Vertical"));
        AddAxisDelta(ref Vehicle_AxesDelta.View.X,   Input.GetAxis("Mouse X"));
        AddAxisDelta(ref Vehicle_AxesDelta.View.Y,   Input.GetAxis("Mouse Y"));

        AddAxisDelta(ref Flyer_AxesDelta.Thrust.X, Input.GetAxis("Horizontal"));
        AddAxisDelta(ref Flyer_AxesDelta.Thrust.Y, Input.GetAxis("Vertical"));
        AddAxisDelta(ref Flyer_AxesDelta.View.X,   Input.GetAxis("Mouse X"));
        AddAxisDelta(ref Flyer_AxesDelta.View.Y,   Input.GetAxis("Mouse Y"));
    }

    void AddAxisDelta(ref PhxInputAxis axis, float delta)
    {
        axis.Value = delta > axis.Deadzone || delta < -axis.Deadzone ? delta * axis.Scale : 0f;
    }

    struct PhxButtonTime
    {
        public bool   Down;
        public float  Time;
    }
}