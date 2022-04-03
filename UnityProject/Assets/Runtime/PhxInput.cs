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


    All = 0xffff
}

public struct PhxButtonEvents       // Description:                                                         Available:
{                                   // ---------------------------------------------------------------------------------------                               
    public PhxInput Down;           // As long as the button is down                                        multiple frames
    public PhxInput Changed;        // When value changed                                                   one frame
    public PhxInput Pressed;        // When button was initially pressed down                               one frame
    public PhxInput Released;       // When button is back up                                               one frame
    public PhxInput Tab;            // When the button was held for a short time and then released          one frame
    //public PhxInput DoubleTab;    // When the button was tapped twice within a short time period          one frame
    public PhxInput Hold;           // When the button was hold for a slightly longer time, then released   one frame
}

public enum PhxInputAxisType
{
    Absolute,
    Relative
}

public struct PhxInputAxis2D
{
    public Vector2           Axis;
    public PhxInputAxisType  Type;
}

public struct PhxInputAxesGroup
{
    public PhxInputAxis2D Thrust;
    public PhxInputAxis2D View;
}

//public enum PhxInputButtonAction
//{
//    None, 
//    Changed, 
//    Tab, 
//    DoubleTab, 
//    Hold, 
//    Down
//}

//public struct PhxInputEvent
//{
//    public PhxInput Input;
//    public PhxInputButtonAction Action;
//}

public class PhxPlayerInput
{
    //public static bool IsAxis(PhxInput input)
    //{
    //    return
    //        input == PhxInput.Thrust ||
    //        input == PhxInput.View;
    //}

    //public static bool IsButton(PhxInput input)
    //{
    //    return !IsAxis(input) || input == PhxInput.FireBoth;
    //}

    PhxButtonTime[]    PrevFrameButtons;
    PhxButtonEvents    ButtonFrameEvents;
    PhxInputAxesGroup  Soldier_Axes;
    PhxInputAxesGroup  Vehicle_Axes;
    PhxInputAxesGroup  Flyer_Axes;

    static readonly string[] UnityInputNames = new string[10]
    {
        "Cancel",
        "Submit",
        "Cancel",

        "Fire1",
        "Fire2",
        "Jump",
        "Sprint",
        "Crouch",
        "Roll",
        "EnterVehicle",
    };

    public PhxPlayerInput()
    {
        PrevFrameButtons = new PhxButtonTime[7];

        // Using Mouse, which is a relative axis
        Soldier_Axes.View.Type = PhxInputAxisType.Relative;
        Vehicle_Axes.View.Type = PhxInputAxisType.Relative;
        Flyer_Axes.View.Type   = PhxInputAxisType.Relative;
    }

    public PhxButtonEvents GetButtonEvents()
    {
        return ButtonFrameEvents;
    }

    public PhxInputAxesGroup GetSoldierAxes()
    {
        return Soldier_Axes;
    }
    public PhxInputAxesGroup GetVehicleAxes()
    {
        return Vehicle_Axes;
    }
    public PhxInputAxesGroup GetFlyerAxes()
    {
        return Flyer_Axes;
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

            down       |= ( buttons[i] ? 1UL : 0UL) << (i);
            changed    |= ( buttons[i] !=  PrevFrameButtons[i].Down ? 1UL : 0UL) << (i);
            pressed    |= ( buttons[i] && !PrevFrameButtons[i].Down ? 1UL : 0UL) << (i);
            released   |= (!buttons[i] &&  PrevFrameButtons[i].Down ? 1UL : 0UL) << (i);
            tab        |= (!buttons[i] &&  PrevFrameButtons[i].Down && PrevFrameButtons[i].Time <= 0.2f ? 1UL : 0UL) << (i);
            hold       |= (!buttons[i] &&  PrevFrameButtons[i].Down && PrevFrameButtons[i].Time >  0.2f ? 1UL : 0UL) << (i);

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

        //
        // Axes
        //
        Soldier_Axes.Thrust.Axis.x = Input.GetAxis("Horizontal");
        Soldier_Axes.Thrust.Axis.y = Input.GetAxis("Vertical");
        Soldier_Axes.View.Axis.x   = Input.GetAxis("Mouse X");
        Soldier_Axes.View.Axis.y   = Input.GetAxis("Mouse Y");

        Vehicle_Axes.Thrust.Axis.x = Input.GetAxis("Horizontal");
        Vehicle_Axes.Thrust.Axis.y = Input.GetAxis("Vertical");
        Vehicle_Axes.View.Axis.x   = Input.GetAxis("Mouse X");
        Vehicle_Axes.View.Axis.y   = Input.GetAxis("Mouse Y");

        Flyer_Axes.Thrust.Axis.x = Input.GetAxis("Horizontal");
        Flyer_Axes.Thrust.Axis.y = Input.GetAxis("Vertical");
        Flyer_Axes.View.Axis.x   = Input.GetAxis("Mouse X");
        Flyer_Axes.View.Axis.y   = Input.GetAxis("Mouse Y");
    }

    struct PhxButtonTime
    {
        public bool   Down;
        public float  Time;
    }
}