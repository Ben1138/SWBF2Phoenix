using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using LibSWBF2.Utils;
using LibSWBF2.Wrappers;


public enum PhxAnimLoopMode
{
    NoLoop,
    Loop,
    LoopFinalFrame
}

public enum PhxAnimAimType
{
    None,
    Head,
    Torso,
    FullBody,
}

public enum PhxAnimSyncType
{
    None,
    AnimDurationRatio,
    ByTime
}

[Flags]
public enum PhxAnimPosture : ushort
{
    None = 0,

    Stand = 1 << 0,
    Crouch = 1 << 1,
    Prone = 1 << 2,
    Sprint = 1 << 3,
    Jump = 1 << 4,
    Roll = 1 << 5,
    Jet = 1 << 6,
    Tumble = 1 << 7,

    Fall = 1 << 8,
    Land = 1 << 9,

    All = 0xffff
}

public enum PhxAnimAction : ushort
{
    None = 0,

    // Movement
    Turn,

    // Weapon
    ShootPrimary,
    ShootPrimary2,
    ShootSecondary,
    ShootSecondary2,
    Charge,
    Reload,
}

public enum PhxAnimTimeMode
{
    Seconds, Frames, FromAnim
}

public static class PhxAnimUtils
{
    public static readonly Dictionary<string, CraConditionType> StrToCondition = new Dictionary<string, CraConditionType>()
    {
        { ">",  CraConditionType.Greater        },
        { ">=", CraConditionType.GreaterOrEqual },

        { "<",  CraConditionType.Less           },
        { "<=", CraConditionType.LessOrEqual    },

        { "=",  CraConditionType.Equal          },
        { "==", CraConditionType.Equal          },
    };

    public static readonly Dictionary<string, PhxInput> StrToInput = new Dictionary<string, PhxInput>()
    {
        { "All",            PhxInput.All                   },
        { "Any",            PhxInput.All                   },
        { "Thrust",         PhxInput.Soldier_Thrust        },
        { "Fire",           PhxInput.Soldier_FirePrimary   },
        { "FirePrimary",    PhxInput.Soldier_FirePrimary   },
        { "FireSecondary",  PhxInput.Soldier_FireSecondary },
        { "FireBoth",       PhxInput.Soldier_FireBoth      },
        { "Jump",           PhxInput.Soldier_Jump          },
        { "Sprint",         PhxInput.Soldier_Sprint        },
        { "Crouch",         PhxInput.Soldier_Crouch        },
        { "Reload",         PhxInput.Soldier_Reload        },
    };

    public static void WriteIntOnEnter(PhxScopedState state, CraMachineValue machineValue, int value)
    {
        WriteIntOnEnter(state.Lower, machineValue, value);
        WriteIntOnEnter(state.Upper, machineValue, value);
    }

    public static void WriteIntOnEnter(CraState state, CraMachineValue machineValue, int value)
    {
        state.WriteOnEnter(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = value } });
    }

    public static void WriteIntOnLeave(PhxScopedState state, CraMachineValue machineValue, int value)
    {
        WriteIntOnLeave(state.Lower, machineValue, value);
        WriteIntOnLeave(state.Upper, machineValue, value);
    }

    public static void WriteIntOnLeave(CraState state, CraMachineValue machineValue, int value)
    {
        state.WriteOnLeave(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = value } });
    }

    public static void WriteBoolOnEnter(PhxScopedState state, CraMachineValue machineValue, bool value)
    {
        WriteBoolOnEnter(state.Lower, machineValue, value);
        WriteBoolOnEnter(state.Upper, machineValue, value);
    }

    public static void WriteBoolOnLeave(CraState state, CraMachineValue machineValue, bool value)
    {
        state.WriteOnLeave(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = value } });
    }

    public static void WriteBoolOnLeave(PhxScopedState state, CraMachineValue machineValue, bool value)
    {
        WriteBoolOnLeave(state.Lower, machineValue, value);
        WriteBoolOnLeave(state.Upper, machineValue, value);
    }

    public static void WriteBoolOnEnter(CraState state, CraMachineValue machineValue, bool value)
    {
        state.WriteOnEnter(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = value } });
    }

    public static void WriteTriggerOnEnter(CraState state, CraMachineValue machineValue, bool value)
    {
        state.WriteOnEnter(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Trigger, ValueBool = value } });
    }
}
