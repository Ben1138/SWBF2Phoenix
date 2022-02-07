using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using LibSWBF2.Utils;


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
    RollLeft = 1 << 5,
    RollRight = 1 << 6,
    Jet = 1 << 7,
    Thrown = 1 << 8,

    Roll = RollLeft | RollRight,

    All = 0xffff
}

public enum PhxAnimValueType
{
    None, 
    Seconds, 
    Frames, 
    FromAnim, 
    FromEdge,

    Impulse,
    ZeroGravity,

    FromSoldier
}

[Flags]
public enum PhxAnimDirection : byte
{
    None = 0,

    Forward = 1 << 0,
    Right = 1 << 1,
    Left = 1 << 2,
    Backward = 1 << 3,

    All = 0xff
}

public struct PhxAnimHandle
{
    public int Index { get; private set; }

    public static PhxAnimHandle None => new PhxAnimHandle { Index = -1, };

    public PhxAnimHandle(int index)
    {
        Index = index;
    }

    public bool IsValid()
    {
        return Index >= 0;
    }
}

public struct PhxAnimValue
{
    public PhxAnimValueType Type;
    public float Value; // NOTE: For Frames, these are in 30 FPS
}

public struct PhxAnim
{
    public CraState Cra;

    public PhxAnimLoopMode Loop;
    public PhxAnimAimType AimType;
    public PhxAnimSyncType SyncType;
    public PhxAnimValue BlendInTime;
    public PhxAnimValue BlendOutTime;
    public PhxAnimValue LowResPose;

    public static PhxAnim CreateDefault()
    {
        return new PhxAnim
        {
            Cra = CraState.None,
            Loop = PhxAnimLoopMode.NoLoop,
            AimType = PhxAnimAimType.None,
            SyncType = PhxAnimSyncType.None,
            BlendInTime = new PhxAnimValue { Value = 0.0f, Type = PhxAnimValueType.Seconds },
            BlendOutTime = new PhxAnimValue { Value = 0.0f, Type = PhxAnimValueType.Seconds },
            LowResPose = new PhxAnimValue { Value = 0.0f, Type = PhxAnimValueType.Seconds }
        };
    }
}

public struct PhxAnimDeflectAnim
{
    public PhxAnim Anim;
    public PhxAnimDirection Directions;
    public bool Offhand;
}

public enum PhxAnimConditionType
{
    None,

    ValueEqual,
    ValueGreater,
    ValueLess,
    ValueGreaterOrEqual,
    ValueLessOrEqual,

    Button,
    ButtonPressed,
    ButtonReleased,

    IsFinished
}

public struct PhxAnimConditionValue
{
    public PhxAnimConditionType Condition;
    public PhxAnimValue CompareValue;
}

public struct PhxAnimCondition
{
    // Time to start checking this condition
    public PhxAnimValue TimeStart;
    public bool TimeStartFromEnd;

    // Time to stop checking this condition
    public PhxAnimValue TimeEnd;
    public bool TimeEndFromEnd;

    // When Type == PhxAnimConditionType.Button
    // Consumes event, such that it's not available
    // for subsequent condition checks in other states
    public PhxInputButtonAction ButtonAction;

    // Flags. These do not consume button events
    public PhxInputControl ButtonsPressed;
    public PhxInputControl ButtonsReleased;

    public PhxAnimPosture Postures;
    public PhxAnimConditionValue TimeInPosture;
    public PhxAnimConditionValue Energy;
    public PhxAnimConditionValue Thrust;        // magnitude
    public PhxAnimConditionValue VelocityXZ;    // magnitude

    public PhxAnimConditionValue VelocityY;     // absolute value
    public PhxAnimConditionValue VelocityYAbs;  // absolute value

    public float ThrustAngleMin;
    public float ThrustAngleMax;

    public float VelocityXZAngleMin;
    public float VelocityXZAngleMax;

    public static PhxAnimCondition CreateDefault()
    {
        return new PhxAnimCondition
        {
            TimeStart = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.Seconds },
            TimeStartFromEnd = false,
            TimeEnd = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.Seconds },
            TimeEndFromEnd = false,
            ButtonAction = PhxInputButtonAction.None,
            ButtonsPressed = PhxInputControl.None,
            ButtonsReleased = PhxInputControl.None,
            Postures = PhxAnimPosture.All,
            TimeInPosture = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            Energy = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            Thrust = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            VelocityXZ = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            VelocityY = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            VelocityYAbs = new PhxAnimConditionValue { Condition = PhxAnimConditionType.None, CompareValue = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None } },
            ThrustAngleMin = -360f,
            ThrustAngleMax = 360f,

            VelocityXZAngleMin = -360f,
            VelocityXZAngleMax = 360f
        };
    }
}

public struct PhxAnimConditionOr
{
    public PhxAnimCondition And0;
    public PhxAnimCondition And1;
    public PhxAnimCondition And2;
    public PhxAnimCondition And3;
    public PhxAnimCondition And4;
    public PhxAnimCondition And5;
    public PhxAnimCondition And6;
    public PhxAnimCondition And7;
    public PhxAnimCondition And8;
    public PhxAnimCondition And9;
}

public struct PhxAnimConditions
{
    public PhxAnimConditionOr Or0;
    public PhxAnimConditionOr Or1;
    public PhxAnimConditionOr Or2;
    public PhxAnimConditionOr Or3;
    public PhxAnimConditionOr Or4;
    public PhxAnimConditionOr Or5;
    public PhxAnimConditionOr Or6;
    public PhxAnimConditionOr Or7;
    public PhxAnimConditionOr Or8;
    public PhxAnimConditionOr Or9;
}

public struct PhxAnimTransition
{
    public PhxAnimHandle TargetState;

    // Also serves implicitly as a condition
    public float EnergyCost;

    public PhxAnimConditions Conditions;

    public static PhxAnimTransition CreateDefault()
    {
        return new PhxAnimTransition
        {
            TargetState = PhxAnimHandle.None,
            EnergyCost = 0f,
        };
    }
}

public enum PhxAnimThrustAlignment
{
    No, Yes, Initial
}

public struct PhxAnimAttack
{
    public uint AttackID;
    public int EdgeIndex;

    // Type must be the same for these two
    public PhxAnimValue DamageTimeStart;
    public PhxAnimValue DamageTimeEnd;

    public bool UseDamage;
    public float Damage;
    public float Push;

    public PhxAnimValue DamageLength;
    public PhxAnimValue DamageWidth;

    public static PhxAnimAttack CreateDefault()
    {
        return new PhxAnimAttack
        {
            AttackID = 0,
            EdgeIndex = 0,

            DamageTimeStart = new PhxAnimValue { Value = 0.2f, Type = PhxAnimValueType.FromAnim },
            DamageTimeEnd = new PhxAnimValue { Value = 0.3f, Type = PhxAnimValueType.FromAnim },

            UseDamage = false,
            Damage = 0,
            Push = 0f,

            DamageLength = new PhxAnimValue { Value = 1f, Type = PhxAnimValueType.FromEdge },
            DamageWidth = new PhxAnimValue { Value = 1f, Type = PhxAnimValueType.FromEdge },
        };
    }
}

public struct PhxAnimDeflect
{
    public PhxAnim Animation;

    // In degrees
    public float DeflectAngleMin;
    public float DeflectAngleMax;

    public float EnergyCost;
}

public struct PhxAnimAnimatedMove
{
    public PhxAnimValue VelocityX;
    public PhxAnimValue VelocityY;
    public PhxAnimValue VelocityZ;
    public float VelocityFromThrust;
    public float VelocityFromStrafe;
    public PhxAnimConditions BreakConditions;

    public static PhxAnimAnimatedMove CreateDefault()
    {
        return new PhxAnimAnimatedMove
        {
            VelocityX = new PhxAnimValue { Value = 1f, Type = PhxAnimValueType.FromAnim },
            VelocityY = new PhxAnimValue { Value = 1f, Type = PhxAnimValueType.FromAnim },
            VelocityZ = new PhxAnimValue { Value = 1f, Type = PhxAnimValueType.FromAnim },
            VelocityFromThrust = 0f,
            VelocityFromStrafe = 0f,
        };
    }
}

public unsafe struct PhxAnimState
{
    // Store all posture related animations here.
    // For custom states, only one Animation will be used
    //public NativeArray<PhxAnim> Animations;
    public byte NumAnimations;

    public bool RestartAnimation;
    public bool PlayExplosion;
    public bool MustShowOneFrame;

    public PhxAnimValue Duration;

    public string SoundName;
    public PhxAnimValue SoundStart;

    // Use root animation for velocity
    public bool UseAnimatedMove;
    public PhxAnimAnimatedMove AnimatedMove;

    public PhxAnimThrustAlignment AlignedToThrust;
    public float Gravity;
    public bool UseGravityVelocityTarget;
    public PhxAnimValue GravityVelocityTarget;

    public PhxInputControl InputLocks; // Flags
    public PhxAnimValue InputLockDuration;

    public float ThrustFactor;
    public float StrafeFactor;
    public float TurnFactor;
    public float PitchFactor;

    public PhxAnimPosture AllowedPostures;

    public fixed bool TurnOffLightsaber[10];
    public byte NumTurnOffLightsabers;

    public PhxAnimValue EnergyRestoreRate;

    //public NativeArray<PhxAnimAttack> Attacks;
    public byte NumAttacks;

    //public NativeArray<PhxAnimTransition> Transition;
    public int NumTransitions;

    public bool UseDeflect;
    public PhxAnimDeflect Deflect;

    public static PhxAnimState CreateDefault()
    {
        // Note: Doesn't initialize Arrays!
        return new PhxAnimState
        {
            NumAnimations = 0,

            RestartAnimation = false,
            PlayExplosion = false,
            MustShowOneFrame = false,
            Duration = new PhxAnimValue { Value = -1f, Type = PhxAnimValueType.FromAnim },
            SoundName = null,
            SoundStart = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.Seconds },
            UseAnimatedMove = false,
            AnimatedMove = PhxAnimAnimatedMove.CreateDefault(),
            AlignedToThrust = PhxAnimThrustAlignment.No,
            Gravity = 1f,
            UseGravityVelocityTarget = false,
            GravityVelocityTarget = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.None },
            InputLocks = PhxInputControl.None,
            InputLockDuration = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.Seconds },
            ThrustFactor = 1f,
            StrafeFactor = 1f,
            TurnFactor = 1f,
            PitchFactor = 1f,
            AllowedPostures = PhxAnimPosture.All,
            NumTurnOffLightsabers = 0,
            EnergyRestoreRate = new PhxAnimValue { Value = 0f, Type = PhxAnimValueType.FromSoldier },

            NumAttacks = 0,
            NumTransitions = 0,

            UseDeflect = false,
            Deflect = new PhxAnimDeflect { Animation = PhxAnim.CreateDefault(), DeflectAngleMin = -180, DeflectAngleMax = 180, EnergyCost = 0f }
        };
    }
}

public class PhxAnimStateMachineManager
{
    const int BlendTimeMatrixSize = 256;
    NativeArray<PhxAnimValue> BlendTimeMatrix;

    PhxAnimationResolver Resolver;
    CraBuffer<HumanData> StateMachines;

    public PhxAnimStateMachineManager()
    {
        Resolver = new PhxAnimationResolver();
        BlendTimeMatrix = new NativeArray<PhxAnimValue>(BlendTimeMatrixSize * BlendTimeMatrixSize, Allocator.Persistent);
        StateMachines = new CraBuffer<HumanData>(new CraBufferSettings { Capacity = 1024, GrowFactor = 1.5f });
    }


    public PhxAnimHandle StateMachine_NewHuman(Transform root, string character, string[] weapons, string combo)
    {
        string weapon = weapons[0];

        PhxAnimHandle h = new PhxAnimHandle(StateMachines.Alloc());
        HumanData data = StateMachines.Get(h.Index);

        data.CraMachine = CraStateMachine.CreateNew();
        data.LayerLower = data.CraMachine.NewLayer();
        data.LayerUpper = data.CraMachine.NewLayer();

        // Stand
        {
            data.Stand = PhxAnimState.CreateDefault();
            data.Stand.Animations = new NativeArray<PhxAnim>(25, Allocator.Persistent);

            // TODO: Add states defined in Combo
            PhxAnim[] standAnims = new PhxAnim[25];
            standAnims[0].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "idle_emote", "StandIdle");
            standAnims[1].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "idle_checkweapon", "StandIdleCheckweapon");
            standAnims[2].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "idle_lookaround", "StandIdleLookaround");
            standAnims[3].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "walkforward", "StandWalkForward");
            standAnims[4].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "runforward", "StandRunForward");
            standAnims[5].Cra  = CreateState(data.LayerLower, root, character, weapon, "stand", "runbackward", "StandRunBackward");
            standAnims[6].Cra  = CreateState(data.LayerUpper, root, character, weapon, "stand", "reload", "StandReload");
            standAnims[7].Cra  = CreateState(data.LayerUpper, root, character, weapon, "stand", "shoot", "StandShootPrimary");
            standAnims[8].Cra  = CreateState(data.LayerUpper, root, character, weapon, "stand", "shoot_secondary", "StandShootSecondary");
            standAnims[9].Cra  = CreateState(data.LayerLower, root, character, weapon, "standalert", "idle_emote", "StandAlertIdle");
            standAnims[10].Cra = CreateState(data.LayerLower, root, character, weapon, "standalert", "walkforward", "StandAlertWalkForward");
            standAnims[11].Cra = CreateState(data.LayerLower, root, character, weapon, "standalert", "runforward", "StandAlertRunForward");
            standAnims[12].Cra = CreateState(data.LayerLower, root, character, weapon, "standalert", "runbackward", "StandAlertRunBackward");
            standAnims[13].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "turnleft", "StandTurnLeft");
            standAnims[14].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "turnright", "StandTurnRight");
            standAnims[15].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "hitfront", "StandHitFront");
            standAnims[16].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "hitback", "StandHitBack");
            standAnims[17].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "hitleft", "StandHitLeft");
            standAnims[18].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "hitright", "StandHitRight");
            standAnims[19].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "getupfront", "StandGetupFront");
            standAnims[20].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "getupback", "StandGetupBack");
            standAnims[21].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "death_forward", "StandDeathForward");
            standAnims[22].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "death_backward", "StandDeathBackward");
            standAnims[23].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "death_left", "StandDeathLeft");
            standAnims[24].Cra = CreateState(data.LayerLower, root, character, weapon, "stand", "death_right", "StandDeathRight");

            data.Stand.Animations.CopyFrom(standAnims);
            data.Stand.NumAnimations = (byte)standAnims.Length;
        }

        // Crouch
        {
            data.Crouch = PhxAnimState.CreateDefault();
            data.Crouch.Animations = new NativeArray<PhxAnim>(14, Allocator.Persistent);

            PhxAnim[] crouchAnims = new PhxAnim[14];
            crouchAnims[0].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "idle_emote", "CrouchIdle");
            crouchAnims[1].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "idle_takeknee", "CrouchIdle");
            crouchAnims[2].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "hitfront", "CrouchHitFront");
            crouchAnims[3].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "hitleft", "CrouchHitLeft");
            crouchAnims[4].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "hitright", "CrouchHitRight");
            crouchAnims[5].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "reload", "CrouchReload");
            crouchAnims[6].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "shoot", "CrouchShoot");
            crouchAnims[7].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "turnleft", "CrouchTurnLeft");
            crouchAnims[8].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "turnright", "CrouchTurnRight");
            crouchAnims[9].Cra  = CreateState(data.LayerLower, root, character, weapon, "crouch", "walkforward", "CrouchWalkForward");
            crouchAnims[10].Cra = CreateState(data.LayerLower, root, character, weapon, "crouch", "walkbackward", "CrouchWalkBackward");
            crouchAnims[11].Cra = CreateState(data.LayerLower, root, character, weapon, "crouchalert", "idle_emote", "CrouchAlertIdle");
            crouchAnims[12].Cra = CreateState(data.LayerLower, root, character, weapon, "crouchalert", "walkforward", "CrouchAlertWalkForward");
            crouchAnims[13].Cra = CreateState(data.LayerLower, root, character, weapon, "crouchalert", "walkbackward", "CrouchAlertWalkBackward");

            data.Crouch.Animations.CopyFrom(crouchAnims);
            data.Crouch.NumAnimations = 14;
        }

        // TODO: continue here
        data.Sprint = PhxAnimState.CreateDefault();
        data.Jump = PhxAnimState.CreateDefault();
        data.RollRight = PhxAnimState.CreateDefault();
        data.RollLeft = PhxAnimState.CreateDefault();
        data.Jet = PhxAnimState.CreateDefault();
        data.Thrown = PhxAnimState.CreateDefault();

        StateMachines.Set(h.Index, data);
        return h;
    }

    CraState CreateState(CraLayer layer, Transform root, string character, string weapon, string posture, string anim, string stateName =null)
    {
        PhxAnimDesc animDesc = new PhxAnimDesc { Character = character, Weapon = weapon, Posture = posture, Animation = anim };
        if (!Resolver.ResolveAnim(animDesc, out CraClip clip, out PhxAnimScope scope, out bool loop))
        {
            return CraState.None;
        }
        Debug.Assert(clip.IsValid());
        CraPlayer player = CraPlayer.CreateNew();
        player.SetLooping(loop);
        player.SetClip(clip);
        switch(scope)
        {
            case PhxAnimScope.Lower:
                player.Assign(root, new CraMask(true, "bone_pelvis"));
                break;
            case PhxAnimScope.Upper:
                player.Assign(root, new CraMask(true, "root_a_spine"));
                break;
            case PhxAnimScope.Full:
                player.Assign(root);
                break;
        }
        Debug.Assert(player.IsValid());
        return layer.NewState(player, stateName);
    }


    struct HumanData
    {
        public CraStateMachine CraMachine;
        public CraLayer LayerLower;
        public CraLayer LayerUpper;

        // Postures
        public PhxAnimState Stand;
        public PhxAnimState Crouch;
        public PhxAnimState Prone;
        public PhxAnimState Sprint;
        public PhxAnimState Jump;
        public PhxAnimState RollRight;
        public PhxAnimState RollLeft;
        public PhxAnimState Jet;
        public PhxAnimState Thrown;

        public PhxAnimPosture Current;
    }
}
