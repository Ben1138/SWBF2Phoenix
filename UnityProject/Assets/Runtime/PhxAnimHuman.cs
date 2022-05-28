using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using LibSWBF2.Enums;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;


public struct PhxScopedState
{
    public CraState Lower;
    public CraState Upper;
}

public struct PhxAnimWeapon
{
    public string AnimationBank;
    public string Combo;
    public string Parent;
    public bool SupportsReload;
    public bool SupportsAlert;
}

// For each weapon, will shall generate one animation set
// Also, a Set should include a basepose (skeleton setup)
public struct PhxAnimHumanSet
{
    // Lower
    public PhxScopedState StandTurnLeft;
    public PhxScopedState StandTurnRight;

    // Lower + Upper
    public PhxScopedState StandIdle;
    public PhxScopedState StandWalkForward;
    public PhxScopedState StandRunForward;
    public PhxScopedState StandRunBackward;
    public PhxScopedState StandAlertIdle;
    public PhxScopedState StandAlertWalkForward;
    public PhxScopedState StandAlertRunForward;
    public PhxScopedState StandAlertRunBackward;

    // Upper
    public PhxScopedState StandHitFront;
    public PhxScopedState StandHitBack;
    public PhxScopedState StandHitLeft;
    public PhxScopedState StandHitRight;
    public PhxScopedState StandReload;
    public PhxScopedState StandShootPrimary;
    public PhxScopedState StandShootSecondary;

    // Lower + Upper
    public PhxScopedState StandIdleCheckweapon;
    public PhxScopedState StandIdleLookaround;
    public PhxScopedState StandGetupFront;
    public PhxScopedState StandGetupBack;
    public PhxScopedState StandDeathForward;
    public PhxScopedState StandDeathBackward;
    public PhxScopedState StandDeathLeft;
    public PhxScopedState StandDeathRight;
    public PhxScopedState StandDeadhero;

    // Lower
    public PhxScopedState CrouchTurnLeft;
    public PhxScopedState CrouchTurnRight;
    public PhxScopedState CrouchIdleTakeknee;

    // Lower + Upper
    public PhxScopedState CrouchIdle;
    public PhxScopedState CrouchWalkForward;
    public PhxScopedState CrouchWalkBackward;
    public PhxScopedState CrouchAlertIdle;
    public PhxScopedState CrouchAlertWalkForward;
    public PhxScopedState CrouchAlertWalkBackward;

    // Upper
    public PhxScopedState CrouchHitFront;
    public PhxScopedState CrouchHitLeft;
    public PhxScopedState CrouchHitRight;
    public PhxScopedState CrouchReload;
    public PhxScopedState CrouchShoot;

    // Lower + Upper
    public PhxScopedState TumbleBounceFrontSoft;
    public PhxScopedState TumbleBounceBackSoft;
    public PhxScopedState TumbleFlail;
    public PhxScopedState TumbleFlyingFront;
    public PhxScopedState TumbleFlyingBack;
    public PhxScopedState TumbleFlyingLeft;
    public PhxScopedState TumbleFlyingRight;
    public PhxScopedState TumbleLandFrontSoft;
    public PhxScopedState TumbleLandBackSoft;
    public PhxScopedState TumbleTumbleFront;
    public PhxScopedState TumbleTumbleBack;

    // Lower + Upper
    public PhxScopedState Jump;
    public PhxScopedState Fall;
    public PhxScopedState LandSoft;
    public PhxScopedState LandHard;
    public PhxScopedState Roll;
    public PhxScopedState Choking;

    // Lower
    public PhxScopedState JetpackHover;

    // Lower + Upper
    public PhxScopedState Sprint;
}

public enum PhxAimType
{
    None,       // body oriented in move direction
    Head,       // body in move direction, head turned to aim
    Torso,      // body in move direction, torso turned to aim
    FullBody    // body in aim direction, override walk/run anims
}


public struct PhxAnimAttackOutput
{
    public CraMachineValue AttackID;
    public CraMachineValue AttackEdge;
    public CraMachineValue AttackDamageTimeStart;
    public CraMachineValue AttackDamageTimeEnd;
    public CraMachineValue AttackDamageTimeMode; // PhxAnimTimeMode
    public CraMachineValue AttackDamageLength;
    public CraMachineValue AttackDamageLengthFromEdge; // bool
    public CraMachineValue AttackDamageWidth;
    public CraMachineValue AttackDamageWidthFromEdge;
    public CraMachineValue AttackDamage;
    public CraMachineValue AttackPush;
}

public class PhxAnimHuman
{
    PhxEnvironment Env => PhxGame.GetEnvironment();

    public CraStateMachine Machine { get; private set; }
    public CraLayer        LayerLower { get; private set; }
    public CraLayer        LayerUpper { get; private set; }

    public CraMachineValue InThrustX { get; private set; }
    public CraMachineValue InThrustY { get; private set; }
    public CraMachineValue InThrustAngle { get; private set; }
    public CraMachineValue InThrustMagnitude { get; private set; }
    public CraMachineValue InWorldVelocity { get; private set; }
    public CraMachineValue InMoveVelocity { get; private set; } // without Z!
    public CraMachineValue InAction { get; private set; }
    public CraMachineValue InTurnLeft { get; private set; } // Make one int
    public CraMachineValue InTurnRight { get; private set; }
    public CraMachineValue InDownEvents { get; private set; }
    public CraMachineValue InChangedEvents { get; private set; }
    public CraMachineValue InPressedEvents { get; private set; }
    public CraMachineValue InReleasedEvents { get; private set; }
    public CraMachineValue InTabEvents { get; private set; }
    public CraMachineValue InHoldEvents { get; private set; }
    public CraMachineValue InEnergy { get; private set; }
    public CraMachineValue InGrounded { get; private set; }
    public CraMachineValue InMultiJump { get; private set; }
    public CraMachineValue InLandHardness { get; private set; }

    public CraMachineValue OutPosture { get; private set; }
    public CraMachineValue OutStrafeBackwards { get; private set; }
    public CraMachineValue OutAction { get; private set; }
    public CraMachineValue OutInputLocks { get; private set; }
    public CraMachineValue OutInputLockDuration { get; private set; }
    public CraMachineValue OutAimType { get; private set; }
    public CraMachineValue OutAnimatedMove { get; private set; }
    public CraMachineValue OutVelocityX { get; private set; }
    public CraMachineValue OutVelocityZ { get; private set; }
    public CraMachineValue OutVelocityXFromAnim { get; private set; }
    public CraMachineValue OutVelocityZFromAnim { get; private set; }
    public CraMachineValue OutVelocityFromThrust { get; private set; }
    public CraMachineValue OutVelocityFromStrafe { get; private set; }

    public CraMachineValue OutSound { get; private set; }
    public PhxAnimAttackOutput[] OutAttacks { get; private set; }


    const float RunVelocityThreshold = 6.8f;

    PhxAnimHumanSet[] Sets;
    byte ActiveSet;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponAnimToSetIdx;

    // In these states, animation playback speed is controlled by current velocity
    HashSet<CraState> MovementStates;

    HashSet<CraState> ComboStates;


    static readonly Dictionary<string, PhxAnimPosture> StrToPosture = new Dictionary<string, PhxAnimPosture>()
    {
        { "All",       PhxAnimPosture.All    },
        { "Any",       PhxAnimPosture.All    },

        { "Stand",     PhxAnimPosture.Stand  },
        { "Crouch",    PhxAnimPosture.Crouch },
        { "Prone",     PhxAnimPosture.Prone  },
        { "Sprint",    PhxAnimPosture.Sprint },
        { "Jump",      PhxAnimPosture.Jump   },
        { "RollLeft",  PhxAnimPosture.Roll   },
        { "RollRight", PhxAnimPosture.Roll   },
        { "Roll",      PhxAnimPosture.Roll   },
        { "Jet",       PhxAnimPosture.Jet    },
    };

    static readonly Dictionary<string, PhxAimType> StrToAim = new Dictionary<string, PhxAimType>()
    {
        { "None",     PhxAimType.None     },
        { "Head",     PhxAimType.Head     },
        { "Torso",    PhxAimType.Torso    },
        { "FullBody", PhxAimType.FullBody },
    };


    public PhxAnimHuman(PhxAnimationResolver resolver, Transform root, string characterAnimBank, PhxAnimWeapon[] weapons)
    {
        Debug.Assert(weapons != null);
        Debug.Assert(weapons.Length > 0);

        Machine = CraStateMachine.CreateNew();
        WeaponAnimToSetIdx = new Dictionary<string, int>();
        MovementStates = new HashSet<CraState>();
        ComboStates = new HashSet<CraState>();

        Resolver = resolver;

        LayerLower = Machine.NewLayer();
        LayerUpper = Machine.NewLayer();

        InThrustX             = Machine.NewMachineValue(CraValueType.Float,   "In Thrust X");
        InThrustY             = Machine.NewMachineValue(CraValueType.Float,   "In Thrust Y");
        InThrustAngle         = Machine.NewMachineValue(CraValueType.Float,   "In Thrust Angle");
        InThrustMagnitude     = Machine.NewMachineValue(CraValueType.Float,   "In Thrust Magnitude");
        InWorldVelocity       = Machine.NewMachineValue(CraValueType.Float,   "In World Velocity");
        InMoveVelocity        = Machine.NewMachineValue(CraValueType.Float,   "In Move Velocity");
        InAction              = Machine.NewMachineValue(CraValueType.Int,     "In Action",                 (CraMachineValue value) => { return ((PhxAnimAction)value.GetInt()).ToString(); });
        InTurnLeft            = Machine.NewMachineValue(CraValueType.Trigger, "In Turn Left");             
        InTurnRight           = Machine.NewMachineValue(CraValueType.Trigger, "In Turn Right");            
        InDownEvents          = Machine.NewMachineValue(CraValueType.Int,     "In Down Events",            (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InChangedEvents       = Machine.NewMachineValue(CraValueType.Int,     "In Changed Events",         (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InPressedEvents       = Machine.NewMachineValue(CraValueType.Int,     "In Pressed Events",         (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InReleasedEvents      = Machine.NewMachineValue(CraValueType.Int,     "In Released Events",        (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InTabEvents           = Machine.NewMachineValue(CraValueType.Int,     "In Tab Events",             (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InHoldEvents          = Machine.NewMachineValue(CraValueType.Int,     "In Hold Events",            (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        InEnergy              = Machine.NewMachineValue(CraValueType.Float,   "In Energy");
        InGrounded            = Machine.NewMachineValue(CraValueType.Bool,    "In Grounded");
        InMultiJump           = Machine.NewMachineValue(CraValueType.Bool,    "In Multi Jump");
        InLandHardness        = Machine.NewMachineValue(CraValueType.Int,     "In Land Hardness");

        //InShootPrimary.SetTriggerMaxLifeTime(0.5f);
        //InShootSecondary.SetTriggerMaxLifeTime(0.5f);
        //InReload.SetTriggerMaxLifeTime(0.5f);

        OutPosture            = Machine.NewMachineValue(CraValueType.Int,     "Out Posture",               (CraMachineValue value) => { return ((PhxAnimPosture)value.GetInt()).ToString(); });
        OutStrafeBackwards    = Machine.NewMachineValue(CraValueType.Bool,    "Out Strafe Backwards");
        OutInputLocks         = Machine.NewMachineValue(CraValueType.Int,     "Out Input Locks",           (CraMachineValue value) => { return ((PhxInput)value.GetInt()).ToString(); });
        OutInputLockDuration  = Machine.NewMachineValue(CraValueType.Float,   "Out Input Lock Duration");
        OutAimType            = Machine.NewMachineValue(CraValueType.Int,     "Out Aim Type",              (CraMachineValue value) => { return ((PhxAimType)value.GetInt()).ToString(); });
        OutAnimatedMove       = Machine.NewMachineValue(CraValueType.Bool,    "Out Animated Move");
        OutVelocityX          = Machine.NewMachineValue(CraValueType.Float,   "Out Velocity X");
        OutVelocityZ          = Machine.NewMachineValue(CraValueType.Float,   "Out Velocity Z");
        OutVelocityXFromAnim  = Machine.NewMachineValue(CraValueType.Bool,    "Out Velocity X From Anim");
        OutVelocityZFromAnim  = Machine.NewMachineValue(CraValueType.Bool,    "Out Velocity Z From Anim");
        OutVelocityFromThrust = Machine.NewMachineValue(CraValueType.Float,   "Out Velocity From Thrust");
        OutVelocityFromStrafe = Machine.NewMachineValue(CraValueType.Float,   "Out Velocity From Strafe");
        OutSound              = Machine.NewMachineValue(CraValueType.Int,     "Out Sound");
        OutAction             = Machine.NewMachineValue(CraValueType.Int,     "Out Action",                (CraMachineValue value) => { return ((PhxAnimAction)value.GetInt()).ToString(); });

        OutAttacks = new PhxAnimAttackOutput[4];
        for (int i = 0; i < OutAttacks.Length; ++i)
        {
            OutAttacks[i].AttackID                   = Machine.NewMachineValue(CraValueType.Int,   $"[{i}] Attack ID");
            OutAttacks[i].AttackEdge                 = Machine.NewMachineValue(CraValueType.Int,   $"[{i}] Attack Edge");
            OutAttacks[i].AttackDamageTimeStart      = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Time Start");
            OutAttacks[i].AttackDamageTimeEnd        = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Time End");
            OutAttacks[i].AttackDamageTimeMode       = Machine.NewMachineValue(CraValueType.Int,   $"[{i}] Attack Damage Time Mode");
            OutAttacks[i].AttackDamageLength         = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Length");
            OutAttacks[i].AttackDamageLengthFromEdge = Machine.NewMachineValue(CraValueType.Bool,  $"[{i}] Attack Damage Length From Edge");
            OutAttacks[i].AttackDamageWidth          = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Width");
            OutAttacks[i].AttackDamageWidthFromEdge  = Machine.NewMachineValue(CraValueType.Bool,  $"[{i}] Attack Damage Width From Edge");
            OutAttacks[i].AttackDamage               = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage");
            OutAttacks[i].AttackPush                 = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack IPush");

            OutAttacks[i].AttackID.SetInt(-1);
        }

        Sets = new PhxAnimHumanSet[weapons.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            WeaponAnimToSetIdx.Add(weapons[i].AnimationBank, i);
            Sets[i] = GenerateSet(root, characterAnimBank, weapons[i]);

            // TODO: Do not generate reload states when current weapon doesn't have/support reload (same for Alert)
            Transitions_Stand         (in Sets[i]);
            Transitions_StandReload   (in Sets[i]);
            Transitions_StandTurn     (in Sets[i]);
            Transitions_StandToFall   (in Sets[i]);
            Transitions_StandToCrouch (in Sets[i]);
            Transitions_Crouch        (in Sets[i]);
            Transitions_CrouchReload  (in Sets[i]);
            Transitions_CrouchTurn    (in Sets[i]);
            Transitions_CrouchToStand (in Sets[i]);
            Transitions_CrouchToFall  (in Sets[i]);
            Transitions_Sprint        (in Sets[i]);
            Transitions_Jump          (in Sets[i]);
            Transitions_Land          (in Sets[i]);
            Transitions_Roll          (in Sets[i]);
        }

        LayerLower.SetActiveState(Sets[ActiveSet].StandIdle.Lower);
        LayerUpper.SetActiveState(Sets[ActiveSet].StandIdle.Upper);
    }

    public bool IsMovementState(CraState state)
    {
        return MovementStates.Contains(state);
    }

    void AddMovementState(PhxScopedState state)
    {
        MovementStates.Add(state.Upper);
        MovementStates.Add(state.Lower);
    }

    public bool IsComboState(CraState state)
    {
        return ComboStates.Contains(state);
    }

    void AddComboState(PhxScopedState state)
    {
        ComboStates.Add(state.Upper);
        ComboStates.Add(state.Lower);
    }

    void Transition(in PhxScopedState from, in PhxScopedState to, float transitionTime, params CraConditionOr[] args)
    {
        if (from.Lower.IsValid() && to.Lower.IsValid())
        {
            Transition(from.Lower, to.Lower, transitionTime, args);
        }
        if (from.Upper.IsValid() && to.Upper.IsValid())
        {
            Transition(from.Upper, to.Upper, transitionTime, args);
        }
    }

    unsafe void Transition(CraState from, CraState to, float transitionTime, params CraConditionOr[] args)
    {
        Debug.Assert(from.IsValid());
        Debug.Assert(to.IsValid());

        CraTransitionData transition = new CraTransitionData
        {
            Target = to,
            TransitionTime = transitionTime
        };

        // No fixed() needed, since 'transition' is allocated on the stack
        CraConditionOr* or = &transition.Or0;
        for (int i = 0; i < args.Length; ++i)
        {
            if (i >= 10)
            {
                Debug.LogError("Cra currently doesn't support more than 10 'Or' conditions!");
                break;
            }

            or[i] = args[i];
        }

        from.NewTransition(transition);
    }

    void Transitions_Stand(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.StandWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandWalkForward, set.StandIdle, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
            }
        );

        Transition(set.StandWalkForward, set.StandRunForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = RunVelocityThreshold },
                }
            }
        );

        Transition(set.StandRunForward, set.StandWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = RunVelocityThreshold },
                }
            }
        );

        Transition(set.StandIdle, set.StandRunBackward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandWalkForward, set.StandRunBackward, 0.25f,
            new CraConditionOr
            {
                //And0 = new CraCondition
                //{
                //    Type = CraConditionType.Greater,
                //    Input = InMoveVelocity,
                //    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                //},
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandRunForward, set.StandRunBackward, 0.25f,
            new CraConditionOr
            {
                //And0 = new CraCondition
                //{
                //    Type = CraConditionType.Greater,
                //    Input = InMoveVelocity,
                //    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                //},
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandRunBackward, set.StandIdle, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandRunBackward, set.StandWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandRunBackward, set.StandRunForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = RunVelocityThreshold },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );
    }

    void Transitions_StandReload(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.StandReload, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.StandReload, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
            }
        );

        Transition(set.StandReload.Lower, set.StandWalkForward.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandReload.Lower, set.StandRunBackward.Lower, 0.15f,
            new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.StandWalkForward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.StandRunForward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.StandRunBackward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandWalkForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandRunForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandRunBackward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                }
            }
        );
    }

    void Transitions_StandTurn(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle.Lower, set.StandTurnLeft.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InTurnLeft
                }
            }
        );

        Transition(set.StandIdle.Lower, set.StandTurnRight.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InTurnRight
                }
            }
        );

        Transition(set.StandTurnLeft.Lower, set.StandIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.StandTurnRight.Lower, set.StandIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
            }
        );
    }

    void Transitions_Crouch(in PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle, set.CrouchWalkForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.GreaterOrEqual,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchWalkForward, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchIdle, set.CrouchWalkBackward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchWalkBackward, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );
    }

    void Transitions_CrouchReload(in PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle, set.CrouchReload, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.CrouchReload, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchReload.Lower, set.CrouchWalkForward.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.GreaterOrEqual,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchReload.Lower, set.CrouchWalkBackward.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );

        Transition(set.CrouchWalkForward.Upper, set.CrouchReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.CrouchWalkBackward.Upper, set.CrouchReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Reload }
                }
            }
        );

        Transition(set.CrouchReload.Upper, set.CrouchWalkForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.CrouchReload.Upper, set.CrouchWalkBackward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                }
            }
        );
    }

    void Transitions_CrouchTurn(in PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle.Lower, set.CrouchTurnLeft.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InTurnLeft
                }
            }
        );

        Transition(set.CrouchIdle.Lower, set.CrouchTurnRight.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMoveVelocity,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InTurnRight
                }
            }
        );

        Transition(set.CrouchTurnLeft.Lower, set.CrouchIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.CrouchTurnRight.Lower, set.CrouchIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished
                },
            }
        );
    }

    void Transitions_StandToCrouch(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.CrouchIdle, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                }
            }
        );

        Transition(set.StandWalkForward, set.CrouchWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                }
            }
        );

        Transition(set.StandRunForward, set.CrouchWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                }
            }
        );
    }

    void Transitions_CrouchToStand(in PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle, set.StandIdle, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                }
            }
        );

        Transition(set.CrouchWalkForward, set.StandWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InThrustX,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.CrouchWalkForward, set.StandRunForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.CrouchWalkBackward, set.StandRunBackward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Crouch }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                }
            }
        );
    }

    void Transitions_Sprint(in PhxAnimHumanSet set)
    {
        Transition(set.StandRunForward, set.Sprint, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Sprint }
                }
            }
        );

        Transition(set.Sprint, set.StandRunForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InThrustY,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InReleasedEvents,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxInput.Soldier_Sprint }
                }
            }
        );
    }

    void Transitions_Jump(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.StandWalkForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.StandRunForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.StandRunBackward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.Sprint, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.Jump, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InMultiJump,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Jump }
                }
            }
        );

        Transition(set.Jump, set.Fall, 1.0f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                },
                And1 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished,
                }
            }
        );

        Transition(set.Jump, set.LandSoft, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished,
                }
            }
        );
    }

    void Transitions_StandToFall(in PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );

        Transition(set.StandWalkForward, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );

        Transition(set.StandRunForward, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );

        Transition(set.StandRunBackward, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );
    }

    void Transitions_CrouchToFall(in PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );

        Transition(set.CrouchWalkForward, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );

        Transition(set.CrouchWalkBackward, set.Fall, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                }
            }
        );
    }

    void Transitions_Land(in PhxAnimHumanSet set)
    {
        Transition(set.Fall, set.LandSoft, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InLandHardness,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = 1 }
                }
            }
        );

        Transition(set.Fall, set.LandHard, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InGrounded,
                    Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InLandHardness,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = 2 }
                }
            }
        );

        Transition(set.LandSoft, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished,
                }
            }
        );

        Transition(set.LandHard, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Input = CraMachineValue.None,
                    Type = CraConditionType.IsFinished,
                }
            }
        );
    }

    void Transitions_Roll(in PhxAnimHumanSet set)
    {
        Transition(set.StandWalkForward, set.Roll, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Roll }
                }
            }
        );

        Transition(set.StandRunForward, set.Roll, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Roll }
                }
            }
        );

        Transition(set.StandRunBackward, set.Roll, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Roll }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InThrustAngle,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 135 }
                },
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Roll }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.GreaterOrEqual,
                    Input = InThrustAngle,
                    Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 225 }
                },
            }
        );

        Transition(set.Sprint, set.Roll, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InAction,
                    Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAnimAction.Roll }
                }
            }
        );

        Transition(set.Roll, set.StandIdle, 0.5f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                }
            }
        );
    }

    public void SetActiveWeaponBank(string weaponAnimBank)
    {
        if (!WeaponAnimToSetIdx.TryGetValue(weaponAnimBank, out int idx))
        {
            Debug.LogError($"Unknown weapon animation bank '{weaponAnimBank}'!");
            return;
        }

        // TODO: How to keep current states?
        LayerLower.SetActiveState(Sets[idx].StandIdle.Lower, 0.15f);
        LayerUpper.SetActiveState(Sets[idx].StandIdle.Upper, 0.15f);
    }

    public void SetActive(bool status = true)
    {
        Machine.SetActive(status);
    }

    CraState CreateState(Transform root, CraClip clip, bool loop, PhxAnimScope assignScope)
    {
        Debug.Assert(clip.IsValid());
        Debug.Assert(assignScope == PhxAnimScope.Lower || assignScope == PhxAnimScope.Upper);

        CraPlayer player = CraPlayer.CreateNew();
        player.SetLooping(loop);
        player.SetClip(clip);
        CraState state = CraState.None;
        string splitBoneName = PhxUtils.FindTransformRecursive(root, "bone_a_spine") != null ? "bone_a_spine" : "bone_b_spine";
        switch (assignScope)
        {
            case PhxAnimScope.Lower:
                player.Assign(root, new CraMask(CraMaskOperation.Difference, true, splitBoneName));
                state = LayerLower.NewState(player);
                break;
            case PhxAnimScope.Upper:
                player.Assign(root, new CraMask(CraMaskOperation.Intersection, true, splitBoneName));
                state = LayerUpper.NewState(player);
                break;
        }
        Debug.Assert(player.IsValid());
        return state;
    }

    PhxScopedState CreateScopedState(Transform root, string character, string weapon, string posture, string anim, bool loop)
    {
        PhxAnimDesc animDesc = new PhxAnimDesc { Character = character, Weapon = weapon, Posture = posture, Animation = anim };

        PhxScopedState res;
        res.Lower = CraState.None;
        res.Upper = CraState.None;

        var inputDesc = animDesc;
        if (!Resolver.ResolveAnim(ref animDesc, out CraClip clip, out PhxAnimScope animScope))
        {
            Debug.LogError($"Couldn't resolve '{inputDesc}'!");
            return res;
        }
        Debug.Assert(clip.IsValid());
        Debug.Assert(animScope != PhxAnimScope.None);

        if (animScope == PhxAnimScope.Upper)
        {
            animDesc.Scope = null;
            if (!Resolver.ResolveAnim(ref animDesc, out CraClip clipRifle, out _, PhxAnimScope.Lower))
            {
                Debug.LogError($"Couldn't resolve '{inputDesc}' on scope Lower!");
                return res;
            }
            res.Lower = CreateState(root, clipRifle, loop, PhxAnimScope.Lower);
        }
        else
        {
            res.Lower = CreateState(root, clip, loop, PhxAnimScope.Lower);
        }
        res.Upper = CreateState(root, clip, loop, PhxAnimScope.Upper);
        //ResetButtonTriggersOnEnter(res.Lower);

        Debug.Assert(res.Lower.IsValid());
        Debug.Assert(res.Upper.IsValid());

        res.Lower.SetSyncState(res.Upper);
        res.Upper.SetSyncState(res.Lower);

        return res;
    }

    PhxAnimHumanSet GenerateSet(Transform root, string character, PhxAnimWeapon weapon)
    {
        // TODO: Do not generate Reload/Alert states if not supported!
        string weaponName = weapon.AnimationBank;
        PhxAnimHumanSet set = new PhxAnimHumanSet();

        set.CrouchIdle              = CreateScopedState(root, character, weaponName, "crouch",      "idle_emote",       true);
        set.CrouchIdleTakeknee      = CreateScopedState(root, character, weaponName, "crouch",      "idle_takeknee",    false);
        set.CrouchHitFront          = CreateScopedState(root, character, weaponName, "crouch",      "hitfront",         false);
        set.CrouchHitLeft           = CreateScopedState(root, character, weaponName, "crouch",      "hitleft",          false);
        set.CrouchHitRight          = CreateScopedState(root, character, weaponName, "crouch",      "hitright",         false);
        set.CrouchReload            = CreateScopedState(root, character, weaponName, "crouch",      "reload",           false);
        set.CrouchShoot             = CreateScopedState(root, character, weaponName, "crouch",      "shoot",            false);
        set.CrouchTurnLeft          = CreateScopedState(root, character, weaponName, "crouch",      "turnleft",         false);
        set.CrouchTurnRight         = CreateScopedState(root, character, weaponName, "crouch",      "turnright",        false);
        set.CrouchWalkForward       = CreateScopedState(root, character, weaponName, "crouch",      "walkforward",      true);
        set.CrouchWalkBackward      = CreateScopedState(root, character, weaponName, "crouch",      "walkbackward",     true);
        set.CrouchAlertIdle         = CreateScopedState(root, character, weaponName, "crouchalert", "idle_emote",       true);
        set.CrouchAlertWalkForward  = CreateScopedState(root, character, weaponName, "crouchalert", "walkforward",      true);
        set.CrouchAlertWalkBackward = CreateScopedState(root, character, weaponName, "crouchalert", "walkbackward",     true);

        set.StandIdle               = CreateScopedState(root, character, weaponName, "stand",       "idle_emote",       true);
        set.StandIdleCheckweapon    = CreateScopedState(root, character, weaponName, "stand",       "idle_checkweapon", false);
        set.StandIdleLookaround     = CreateScopedState(root, character, weaponName, "stand",       "idle_lookaround",  false);
        set.StandWalkForward        = CreateScopedState(root, character, weaponName, "stand",       "walkforward",      true);
        set.StandRunForward         = CreateScopedState(root, character, weaponName, "stand",       "runforward",       true);
        set.StandRunBackward        = CreateScopedState(root, character, weaponName, "stand",       "runbackward",      true);
        set.StandReload             = CreateScopedState(root, character, weaponName, "stand",       "reload",           false);
        set.StandShootPrimary       = CreateScopedState(root, character, weaponName, "stand",       "shoot",            false);
        set.StandShootSecondary     = CreateScopedState(root, character, weaponName, "stand",       "shoot_secondary",  false);
        set.StandAlertIdle          = CreateScopedState(root, character, weaponName, "standalert",  "idle_emote",       true);
        set.StandAlertWalkForward   = CreateScopedState(root, character, weaponName, "standalert",  "walkforward",      true);
        set.StandAlertRunForward    = CreateScopedState(root, character, weaponName, "standalert",  "runforward",       true);
        set.StandAlertRunBackward   = CreateScopedState(root, character, weaponName, "standalert",  "runbackward",      true);
        set.StandTurnLeft           = CreateScopedState(root, character, weaponName, "stand",       "turnleft",         false);
        set.StandTurnRight          = CreateScopedState(root, character, weaponName, "stand",       "turnright",        false);
        set.StandHitFront           = CreateScopedState(root, character, weaponName, "stand",       "hitfront",         false);
        set.StandHitBack            = CreateScopedState(root, character, weaponName, "stand",       "hitback",          false);
        set.StandHitLeft            = CreateScopedState(root, character, weaponName, "stand",       "hitleft",          false);
        set.StandHitRight           = CreateScopedState(root, character, weaponName, "stand",       "hitright",         false);
        set.StandGetupFront         = CreateScopedState(root, character, weaponName, "stand",       "getupfront",       false);
        set.StandGetupBack          = CreateScopedState(root, character, weaponName, "stand",       "getupback",        false);
        set.StandDeathForward       = CreateScopedState(root, character, weaponName, "stand",       "death_forward",    false);
        set.StandDeathBackward      = CreateScopedState(root, character, weaponName, "stand",       "death_backward",   false);
        set.StandDeathLeft          = CreateScopedState(root, character, weaponName, "stand",       "death_left",       false);
        set.StandDeathRight         = CreateScopedState(root, character, weaponName, "stand",       "death_right",      false);
        set.StandDeadhero           = CreateScopedState(root, character, weaponName, "stand",       "idle_emote",       false);

        set.TumbleBounceFrontSoft   = CreateScopedState(root, character, weaponName, "thrown",      "bouncefrontsoft",  false);
        set.TumbleBounceBackSoft    = CreateScopedState(root, character, weaponName, "thrown",      "bouncebacksoft",   false);
        set.TumbleFlail             = CreateScopedState(root, character, weaponName, "thrown",      "flail",            false);
        set.TumbleFlyingFront       = CreateScopedState(root, character, weaponName, "thrown",      "flyingfront",      false);
        set.TumbleFlyingBack        = CreateScopedState(root, character, weaponName, "thrown",      "flyingback",       false);
        set.TumbleFlyingLeft        = CreateScopedState(root, character, weaponName, "thrown",      "flyingleft",       false);
        set.TumbleFlyingRight       = CreateScopedState(root, character, weaponName, "thrown",      "flyingright",      false);
        set.TumbleLandFrontSoft     = CreateScopedState(root, character, weaponName, "thrown",      "landfrontsoft",    false);
        set.TumbleLandBackSoft      = CreateScopedState(root, character, weaponName, "thrown",      "landbacksoft",     false);
        set.TumbleTumbleFront       = CreateScopedState(root, character, weaponName, "thrown",      "tumblefront",      false);
        set.TumbleTumbleBack        = CreateScopedState(root, character, weaponName, "thrown",      "tumbleback",       false);

        set.Sprint                  = CreateScopedState(root, character, weaponName, "sprint",        null, true);
        set.JetpackHover            = CreateScopedState(root, character, weaponName, "jetpack_hover", null, true);
        set.Jump                    = CreateScopedState(root, character, weaponName, "jump",          null, false);
        set.Fall                    = CreateScopedState(root, character, weaponName, "fall",          null, true);
        set.LandSoft                = CreateScopedState(root, character, weaponName, "landsoft",      null, false);
        set.LandHard                = CreateScopedState(root, character, weaponName, "landhard",      null, false);
        set.Roll                    = CreateScopedState(root, character, weaponName, "diveforward",   null, false);

        WriteParamsOnEnter(set.CrouchIdle,              new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchIdleTakeknee,      new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchHitFront,          new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchHitLeft,           new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchHitRight,          new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchReload,            new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.Reload,         Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchShoot,             new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Torso, Action = PhxAnimAction.ShootPrimary,   Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchTurnLeft,          new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.Turn,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchTurnRight,         new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.None,  Action = PhxAnimAction.Turn,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchWalkForward,       new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchWalkBackward,      new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchAlertIdle,         new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchAlertWalkForward,  new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.CrouchAlertWalkBackward, new PhxStateParams { Posture = PhxAnimPosture.Crouch, AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });

        WriteParamsOnEnter(set.StandIdle,               new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandIdleCheckweapon,    new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandIdleLookaround,     new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandWalkForward,        new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandRunForward,         new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandRunBackward,        new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Head,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandReload,             new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.Reload,         Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandShootPrimary,       new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.ShootPrimary,   Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandShootSecondary,     new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.ShootSecondary, Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandAlertIdle,          new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandAlertWalkForward,   new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandAlertRunForward,    new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandAlertRunBackward,   new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.Torso, Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandTurnLeft,           new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.Turn,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandTurnRight,          new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.Turn,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandHitFront,           new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandHitBack,            new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandHitLeft,            new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandHitRight,           new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.StandGetupFront,         new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandGetupBack,          new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandDeathForward,       new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandDeathBackward,      new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandDeathLeft,          new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandDeathRight,         new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.StandDeadhero,           new PhxStateParams { Posture = PhxAnimPosture.Stand,  AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
                                                                                                                                                                                                        
        WriteParamsOnEnter(set.TumbleBounceFrontSoft,   new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleBounceBackSoft,    new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleFlail,             new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleFlyingFront,       new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleFlyingBack,        new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleFlyingLeft,        new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleFlyingRight,       new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleLandFrontSoft,     new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleLandBackSoft,      new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleTumbleFront,       new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.TumbleTumbleBack,        new PhxStateParams { Posture = PhxAnimPosture.Tumble, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
                                                                                                                                                                                                        
        WriteParamsOnEnter(set.Sprint,                  new PhxStateParams { Posture = PhxAnimPosture.Sprint, AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.JetpackHover,            new PhxStateParams { Posture = PhxAnimPosture.Jet,    AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.Jump,                    new PhxStateParams { Posture = PhxAnimPosture.Jump,   AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.Fall,                    new PhxStateParams { Posture = PhxAnimPosture.Fall,   AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });
        WriteParamsOnEnter(set.LandSoft,                new PhxStateParams { Posture = PhxAnimPosture.Land,   AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.LandHard,                new PhxStateParams { Posture = PhxAnimPosture.Land,   AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.All  });
        WriteParamsOnEnter(set.Roll,                    new PhxStateParams { Posture = PhxAnimPosture.Roll,   AimType = PhxAimType.None,  Action = PhxAnimAction.None,           Locked = PhxInput.None });

        PhxAnimUtils.WriteBoolOnEnter (set.Roll, OutAnimatedMove,       true);
        PhxAnimUtils.WriteFloatOnEnter(set.Roll, OutVelocityX,          1f);
        PhxAnimUtils.WriteFloatOnEnter(set.Roll, OutVelocityZ,          1f);
        PhxAnimUtils.WriteBoolOnEnter (set.Roll, OutVelocityXFromAnim,  true);
        PhxAnimUtils.WriteBoolOnEnter (set.Roll, OutVelocityZFromAnim,  true);
        PhxAnimUtils.WriteFloatOnEnter(set.Roll, OutVelocityFromThrust, 0f);
        PhxAnimUtils.WriteFloatOnEnter(set.Roll, OutVelocityFromStrafe, 0f);
        PhxAnimUtils.WriteBoolOnLeave (set.Roll, OutAnimatedMove,       false);

        PhxAnimUtils.WriteBoolOnEnter(set.StandRunBackward.Lower,        OutStrafeBackwards, true);
        PhxAnimUtils.WriteBoolOnEnter(set.StandAlertRunBackward.Lower,   OutStrafeBackwards, true);
        PhxAnimUtils.WriteBoolOnEnter(set.CrouchAlertWalkBackward.Lower, OutStrafeBackwards, true);
        PhxAnimUtils.WriteBoolOnLeave(set.StandRunBackward.Lower,        OutStrafeBackwards, false);
        PhxAnimUtils.WriteBoolOnLeave(set.StandAlertRunBackward.Lower,   OutStrafeBackwards, false);
        PhxAnimUtils.WriteBoolOnLeave(set.CrouchAlertWalkBackward.Lower, OutStrafeBackwards, false);

        // In these states, animation playback speed is controlled by current velocity
        AddMovementState(set.CrouchWalkForward);
        AddMovementState(set.CrouchWalkBackward);
        AddMovementState(set.CrouchAlertWalkForward);
        AddMovementState(set.CrouchAlertWalkBackward);
        AddMovementState(set.StandWalkForward);
        AddMovementState(set.StandRunForward);
        AddMovementState(set.StandRunBackward);
        AddMovementState(set.StandAlertWalkForward);
        AddMovementState(set.StandAlertRunForward);
        AddMovementState(set.StandAlertRunBackward);
        AddMovementState(set.Sprint);

        //set.Roll.Lower.GetPlayer().SetPlayRange(new CraPlayRange { MinTime = 0f, MaxTime = 0.9f });
        //set.Roll.Upper.GetPlayer().SetPlayRange(new CraPlayRange { MinTime = 0f, MaxTime = 0.9f });

        if (!string.IsNullOrEmpty(weapon.Combo))
        {
            CreateCombo(in set, root, character, weaponName, weapon.Combo);
        }

#if UNITY_EDITOR
        foreach (var field in set.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (field.FieldType == typeof(PhxScopedState))
            {
                PhxScopedState state = (PhxScopedState)field.GetValue(set);
                if (state.Lower.IsValid()) state.Lower.SetName($"Lower {weaponName} {field.Name}");
                if (state.Upper.IsValid()) state.Upper.SetName($"Upper {weaponName} {field.Name}");
            }
        }
#endif
        return set;
    }

    void CreateCombo(in PhxAnimHumanSet set, Transform root, string character, string weapon, string comboName)
    {
        var con = Env.EnvCon;
        Config combo = con.FindConfig(EConfigType.Combo, comboName);
        if (combo == null)
        {
            Debug.LogError($"Couldn't find combo {comboName}!");
        }
        else
        {
            // Since transitions may refer to yet undefined states (or in circles), we need
            // to store all the transitions and apply them AFTER the full combo has been parsed!
            List<PhxComboTransitionCache> Transitions = new List<PhxComboTransitionCache>();
            Dictionary<string, PhxComboState> ComboStates = new Dictionary<string, PhxComboState>();

            // For proper Debug output only
            HashSet<string> IgnoredStates = new HashSet<string>();

            Field[] fields = combo.GetFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                Field field = fields[i];
                if (field.GetNameHash() == Hash_State)
                {
                    string stateName = field.GetString();
                    bool isIdle = PhxUtils.StrEquals(stateName, "IDLE");

                    if (ComboStates.ContainsKey(stateName))
                    {
                        Debug.LogError($"State '{stateName}' is defined more than once in combo '{comboName}'!");
                    }
                    if (!CreateComboState(root, character, weapon, comboName, field, out PhxComboState comboState) && !isIdle)
                    {
                        // State has no Animation() assigned. Silently ignore for now, since their only purpose seems to be to transition towards IDLE.
                        //Debug.LogError($"Failed to create combo state '{stateName}' in Combo '{comboName}'!");
                        IgnoredStates.Add(stateName);
                        continue;
                    }

                    if (isIdle)
                    {
                        comboState.State = set.StandIdle;
                    }

                    List<PhxComboTransitionCache> localTransitions = new List<PhxComboTransitionCache>();

                    int   attackCount = 0;
                    Field[] stateScope = field.Scope.GetFields();
                    for (int j = 0; j < stateScope.Length; ++j)
                    {
                        Field stateField = stateScope[j];
                        if (stateField.GetNameHash() == Hash_Animation)
                        {
                            Field[] animationScope = stateField.Scope.GetFields();
                            for (int k = 0; k < animationScope.Length; ++k)
                            {
                                Field animationField = animationScope[k];
                                if (animationField.GetNameHash() == Hash_AimType)
                                {
                                    string aimStr = animationField.GetString();
                                    if (StrToAim.TryGetValue(aimStr, out PhxAimType aim))
                                    {
                                        comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                                        {
                                            MachineValue = OutAimType,
                                            Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)aim }
                                        });
                                        comboState.State.Upper.WriteOnLeave(new CraWriteOutput
                                        {
                                            MachineValue = OutAimType,
                                            Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxAimType.None }
                                        });
                                    }
                                    else
                                    {
                                        Debug.LogError($"Unknown AimType '{aimStr}'!");
                                    }
                                }
                                else if (animationField.GetNameHash() == Hash_BlendInTime)
                                {
                                    // TODO
                                }
                                else if (animationField.GetNameHash() == Hash_BlendOutTime)
                                {
                                    // TODO
                                }
                            }
                        }
                        else if (stateField.GetNameHash() == Hash_Duration)
                        {
                            float time = GetTimeValue(stateField, comboState.State.Lower.GetPlayer().GetClip().GetDuration());
                            if (time <= 0f)
                            {
                                comboState.State.Lower.GetPlayer().SetLooping(true);
                                comboState.State.Upper.GetPlayer().SetLooping(true);
                            }
                            else
                            {
                                CraPlayRange range = new CraPlayRange { MinTime = 0f, MaxTime = time };
                                comboState.State.Lower.GetPlayer().SetPlayRange(range);
                                comboState.State.Upper.GetPlayer().SetPlayRange(range);
                            }
                        }
                        else if (stateField.GetNameHash() == Hash_Transition)
                        {
                            localTransitions.Add(new PhxComboTransitionCache 
                            { 
                                TransitionField = stateField, 
                                SourceStateIsIdle = isIdle
                            });
                        }
                        else if (stateField.GetNameHash() == Hash_Posture)
                        {
                            if (comboState.Posture != PhxAnimPosture.None)
                            {
                                Debug.LogError($"State '{stateName}' has more than one Posture entry in Combo '{comboName}'!");
                                continue;
                            }

                            // TODO: Use as condition in transitions?
                            for (byte pi = 0; pi < stateField.GetNumValues(); ++pi)
                            {
                                string postureStr = stateField.GetString(pi);
                                bool negate = postureStr.StartsWith("!");
                                if (negate)
                                {
                                    postureStr = postureStr.Substring(1, postureStr.Length - 1);
                                }
                                if (!StrToPosture.TryGetValue(postureStr, out PhxAnimPosture p))
                                {
                                    Debug.LogError($"Unknown posture '{postureStr}' in state '{stateName}' in Combo '{comboName}'!");
                                    continue;
                                }
                                if (negate)
                                {
                                    comboState.Posture &= ~p;
                                }
                                else
                                {
                                    comboState.Posture |= p;
                                }
                            }
                        }
                        else if (stateField.GetNameHash() == Hash_Sound)
                        {
                            string soundName = stateField.GetString();
                            uint soundHash = HashUtils.GetFNV(soundName);
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutSound,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)soundHash }
                            });
                            comboState.State.Upper.WriteOnLeave(new CraWriteOutput
                            {
                                MachineValue = OutSound,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = 0 }
                            });
                        }
                        else if (stateField.GetNameHash() == Hash_InputLock)
                        {
                            float time = string.IsNullOrEmpty(stateField.GetString(0)) ? stateField.GetFloat(0) : 0f;
                            PhxInput locked = PhxInput.None;
                            for (byte vi = 0; vi < stateField.GetNumValues(); vi++)
                            {
                                string input = stateField.GetString(vi);
                                if (string.IsNullOrEmpty(input))
                                {
                                    continue;
                                }
                                if (PhxUtils.StrEquals(input, "Frames") || PhxUtils.StrEquals(input, "Seconds"))
                                {
                                    if (PhxUtils.StrEquals(input, "Frames"))
                                    {
                                        time /= 30f;
                                    }
                                    continue;
                                }

                                bool bInvert = input.StartsWith("!");
                                if (bInvert)
                                {
                                    input = input.Substring(1);
                                }

                                if (!PhxAnimUtils.StrToInput.TryGetValue(input, out PhxInput lockedInput))
                                {
                                    Debug.LogError($"Unknown input '{input}'!");
                                }
                                else if (!bInvert)
                                {
                                    locked |= lockedInput;
                                }
                                else
                                {
                                    locked &= ~lockedInput;
                                }
                            }
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutInputLocks,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)locked }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutInputLockDuration,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = time }
                            });                         
                            comboState.State.Upper.WriteOnLeave(new CraWriteOutput
                            {
                                MachineValue = OutInputLocks,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)PhxInput.None }
                            });
                        }
                        else if (stateField.GetNameHash() == Hash_AnimatedMove)
                        {
                            float  velocityX           = 1.0f;
                            bool   velocityXFromAnim   = true;
                            float  velocityZ           = 1.0f;
                            bool   velocityZFromAnim   = true;
                            float  velocityFromThrust  = 0f;
                            float  velocityFromStrafe  = 0f;

                            comboState.IsAnimatedMove = true;
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAnimatedMove,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                            });
                            comboState.State.Upper.WriteOnLeave(new CraWriteOutput
                            {
                                MachineValue = OutAnimatedMove,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                            });

                            Field[] animatedMoveScope = stateField.Scope.GetFields();
                            for (int k = 0; k < animatedMoveScope.Length; ++k)
                            {
                                Field animMoveField = animatedMoveScope[k];
                                if (animMoveField.GetNameHash() == Hash_VelocityX)
                                {
                                    if (animMoveField.GetNumValues() > 0)
                                    {
                                        velocityX = animMoveField.GetFloat();
                                    }
                                    velocityXFromAnim = animMoveField.GetNumValues() > 1;
                                }
                                else if (animMoveField.GetNameHash() == Hash_VelocityZ)
                                {
                                    if (animMoveField.GetNumValues() > 0)
                                    {
                                        velocityZ = animMoveField.GetFloat();
                                    }
                                    velocityZFromAnim = animMoveField.GetNumValues() > 1;
                                }
                                else if (animMoveField.GetNameHash() == Hash_VelocityFromThrust)
                                {
                                    if (animMoveField.GetNumValues() > 0)
                                    {
                                        velocityFromThrust = animMoveField.GetFloat();
                                    }
                                }
                                else if (animMoveField.GetNameHash() == Hash_VelocityFromStrafe)
                                {
                                    if (animMoveField.GetNumValues() > 0)
                                    {
                                        velocityFromStrafe = animMoveField.GetFloat();
                                    }
                                }
                            }

                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityX,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = velocityX }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityXFromAnim,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = velocityXFromAnim }
                            });

                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityZ,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = velocityZ }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityZFromAnim,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = velocityZFromAnim }
                            });


                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityFromThrust,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = velocityFromThrust }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutVelocityFromStrafe,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = velocityFromStrafe }
                            });
                        }
                        else if (stateField.GetNameHash() == Hash_Attack)
                        {
                            if (attackCount >= 4)
                            {
                                Debug.LogError($"PhxAnimHuman does currently not support more thasn 4 Attacks!");
                                continue;
                            }

                            int    id              = 0;
                            int    edge            = 0;
                            float  timeStart       = 0.2f;
                            float  timeEnd         = 0.3f;
                            int    timeMode        = (int)PhxAnimTimeMode.FromAnim;
                            float  length          = 1f;
                            bool   lengthFromEdge  = true;
                            float  width           = 1f;
                            bool   widthFromEdge   = true;
                            float  damage          = 0f;
                            float  push            = 0f;

                            Field[] attackScope = stateField.Scope.GetFields();
                            for (int ai = 0; ai < attackScope.Length; ++ai)
                            {
                                Field attackField = attackScope[ai];
                                if (attackField.GetNameHash() == Hash_AttackId)
                                {
                                    // Hash function doesn't really matter here,
                                    // since we just want to compare attacks
                                    id = (int)HashUtils.GetFNV(attackField.GetString());
                                }
                                else if (attackField.GetNameHash() == Hash_Edge)
                                {
                                    edge = (int)attackField.GetFloat();
                                }
                                else if (attackField.GetNameHash() == Hash_DamageTime)
                                {
                                    Debug.Assert(attackField.GetNumValues() >= 2);

                                    timeStart = attackField.GetFloat(0);
                                    timeEnd = attackField.GetFloat(1);
                                    if (attackField.GetNumValues() >= 3)
                                    {
                                        string mode = attackField.GetString(2);
                                        switch (mode)
                                        {
                                            case "Seconds":  timeMode = (int)PhxAnimTimeMode.Seconds;  break;
                                            case "Frames":   timeMode = (int)PhxAnimTimeMode.Frames;   break;
                                            case "FromAnim": timeMode = (int)PhxAnimTimeMode.FromAnim; break;
                                            default:
                                            {
                                                Debug.LogError($"Unknown time mode '{mode}'!");
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (attackField.GetNameHash() == Hash_DamageLength)
                                {
                                    length = attackField.GetFloat(0);
                                    lengthFromEdge = attackField.GetNumValues() > 1 && PhxUtils.StrEquals(attackField.GetString(1), "FromEdge");
                                }
                                else if (attackField.GetNameHash() == Hash_DamageWidth)
                                {
                                    width = attackField.GetFloat(0);
                                    widthFromEdge = attackField.GetNumValues() > 1 && PhxUtils.StrEquals(attackField.GetString(1), "FromEdge");
                                }
                                else if (attackField.GetNameHash() == Hash_Damage)
                                {
                                    damage = attackField.GetFloat();
                                }
                                else if (attackField.GetNameHash() == Hash_Push)
                                {
                                    push = attackField.GetFloat();
                                }
                            }

                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput 
                            { 
                                MachineValue = OutAttacks[attackCount].AttackID,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = id }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackEdge,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = edge }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageTimeStart,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = timeStart }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageTimeEnd,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = timeEnd }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageTimeMode,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = timeMode }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageLength,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = length }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageLengthFromEdge,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = lengthFromEdge }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageWidth,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = width }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamageWidthFromEdge,
                                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = widthFromEdge }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackDamage,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = damage }
                            });
                            comboState.State.Upper.WriteOnEnter(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackPush,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = push }
                            });


                            comboState.State.Upper.WriteOnLeave(new CraWriteOutput
                            {
                                MachineValue = OutAttacks[attackCount].AttackID,
                                Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = -1 }
                            });


                            attackCount++;
                        }
                    }

                    for (int j = 0; j < localTransitions.Count; ++j)
                    {
                        var lt = localTransitions[j];
                        lt.SourceState = comboState;
                        localTransitions[j] = lt;
                    }
                    ComboStates.Add(stateName, comboState);
                    Transitions.AddRange(localTransitions);
                }
            }

            // Lower posture transitions Combo State --> Walk
            foreach (var comboState in ComboStates)
            {
                PhxComboState state = comboState.Value;

                if (state.IsAnimatedMove)
                {
                    Transition(set.StandWalkForward.Lower, state.State.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Equal,
                                Input = OutAnimatedMove,
                                Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                            }
                        }
                    );

                    Transition(set.StandRunForward.Lower, state.State.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Equal,
                                Input = OutAnimatedMove,
                                Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                            }
                        }
                    );

                    Transition(set.StandRunBackward.Lower, state.State.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Equal,
                                Input = OutAnimatedMove,
                                Compare = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                            }
                        }
                    );
                }
                else if ((state.Posture & PhxAnimPosture.Stand) != 0)
                {
                    // TODO: Add transitions to all lower walk states and back
                    // Make sure lower walk states can't transition further (e.g. to sprint or crouch)

                    Transition(state.State.Lower, set.StandWalkForward.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Greater,
                                Input = InMoveVelocity,
                                Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                            },
                            And1 = new CraCondition
                            {
                                Type = CraConditionType.GreaterOrEqual,
                                Input = InThrustY,
                                Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                            }
                        }
                    );

                    Transition(state.State.Lower, set.StandRunBackward.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Less,
                                Input = InThrustY,
                                Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0f },
                            }
                        }
                    );
                }
            }

            for (int ti = 0; ti < Transitions.Count; ++ti)
            {
                PhxComboState sourceState = Transitions[ti].SourceState;

                if (!sourceState.State.Lower.IsValid() || !sourceState.State.Upper.IsValid())
                {
                    // TODO: This should never happen later on
                    Debug.Assert(false);
                    continue;
                }

                CraPlayRange stateRange = sourceState.State.Upper.GetPlayer().GetPlayRange();
                float stateDuration = stateRange.MaxTime - stateRange.MinTime;
                PhxComboTransitionCondition[] conditions = GetComboTransitionConditions(Transitions[ti].TransitionField.Scope, stateDuration);

                PhxComboState targetState = new PhxComboState();
                string targetStateName = Transitions[ti].TransitionField.GetString();
                if (PhxUtils.StrEquals(targetStateName, "IDLE"))
                {
                    if ((sourceState.Posture & PhxAnimPosture.Jump) != 0 || (sourceState.Posture & PhxAnimPosture.Fall) != 0)
                    {
                        targetState.State = set.Fall;
                    }
                    else
                    {
                        targetState.State = set.StandIdle;
                    }
                }
                else
                {
                    if (!ComboStates.TryGetValue(targetStateName, out targetState))
                    {
                        if (!IgnoredStates.Contains(targetStateName))
                        {
                            Debug.LogError($"Tried to transition to unknown state '{targetStateName}' in combo '{comboName}'!");
                        }
                        continue;
                    }
                }

                if (!targetState.State.Lower.IsValid() || !targetState.State.Upper.IsValid())
                {
                    // TODO: This should never happen later on
                    Debug.Assert(false);
                    continue;
                }

                // Assumption: When the target state has the same animation assigned as
                // the source state, continue playing on target where we left off in source.
                // E.g.: In rep_hero_obiwan.combo, ATTACK3 and RECOVER3 have the same animation,
                // but it doesn't restart when entering RECOVER3
                void CheckSameAnimation(CraState src, CraState tar)
                {
                    if (tar.GetPlayer().GetClip() == src.GetPlayer().GetClip())
                    {
                        tar.SetSyncState(src);
                    }
                }
                CheckSameAnimation(sourceState.State.Lower, targetState.State.Lower);
                CheckSameAnimation(sourceState.State.Upper, targetState.State.Upper);

                // Since SourceState can be IDLE, and IDLE can represent multiple states, we have to eval each OR condition individually...
                for (int ci = 0; ci < conditions.Length; ci++)
                {
                    ref var cond = ref conditions[ci];
                    if (Transitions[ti].SourceStateIsIdle)
                    {
                        if ((cond.SourcePosture & PhxAnimPosture.Stand) != 0)
                        {
                            Debug.Assert(sourceState.State.Lower == set.StandIdle.Lower);
                            Debug.Assert(sourceState.State.Upper == set.StandIdle.Upper);
                            Transition(set.StandIdle, targetState.State, 0.15f, cond.Or);
                        }
                        if ((cond.SourcePosture & PhxAnimPosture.Crouch) != 0)
                        {
                            Transition(set.CrouchIdle, targetState.State, 0.15f, cond.Or);
                        }
                        if ((cond.SourcePosture & PhxAnimPosture.Sprint) != 0)
                        {
                            Transition(set.Sprint, targetState.State, 0.15f, cond.Or);
                        }
                        if ((cond.SourcePosture & PhxAnimPosture.Jump) != 0)
                        {
                            Transition(set.Jump, targetState.State, 0.15f, cond.Or);
                            Transition(set.Fall, targetState.State, 0.15f, cond.Or);
                        }

                        Transition(sourceState.State, targetState.State, 0.15f, cond.Or);
                        Transition(set.StandWalkForward.Upper, targetState.State.Upper, 0.15f, cond.Or);
                        Transition(set.StandRunForward.Upper, targetState.State.Upper, 0.15f, cond.Or);
                        Transition(set.StandRunBackward.Upper, targetState.State.Upper, 0.15f, cond.Or);
                    }
                    else
                    {
                        Transition(sourceState.State, targetState.State, 0.15f, cond.Or);
                    }
                }
            }
        }
    }

    bool CreateComboState(Transform root, string character, string weapon, string comboName, Field state, out PhxComboState comboState)
    {
        comboState = new PhxComboState();

        string stateName = state.GetString();
        Field[] stateScope = state.Scope.GetFields();
        for (int i = 0; i < stateScope.Length; ++i)
        {
            Field stateField = stateScope[i];
            if (stateField.GetNameHash() == Hash_Animation)
            {
                string animname = stateField.GetString();
                string[] split = animname.Split(new char[] { '_' }, 2);
                Debug.Assert(split.Length >= 2);
                comboState.State = CreateScopedState(root, character, weapon, split[0], split[1], true);
                if (comboState.State.Lower.IsValid() && comboState.State.Upper.IsValid())
                {
                    comboState.State.Lower.GetPlayer().SetLooping(false);
                    comboState.State.Upper.GetPlayer().SetLooping(false);
#if UNITY_EDITOR
                    comboState.State.Lower.SetName($"COMBO Lower {weapon} {stateName}");
                    comboState.State.Upper.SetName($"COMBO Upper {weapon} {stateName}");
#endif
                    AddComboState(comboState.State);
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to create state '{stateName}' in Combo '{comboName}'!");
                }

                break;
            }
        }
        comboState = new PhxComboState();
        return false;
    }

    unsafe PhxComboTransitionCondition[] GetComboTransitionConditions(Scope transitionScope, float stateDuration)
    {
        List<PhxComboTransitionCondition> conditions = new List<PhxComboTransitionCondition>();
        Field[] orScope = transitionScope.GetFields();
        if (orScope.Length == 0)
        {
            PhxComboTransitionCondition newCond = new PhxComboTransitionCondition();
            newCond.Or.And0 = new CraCondition
            {
                Input = CraMachineValue.None,
                Type = CraConditionType.IsFinished
            };
            conditions.Add(newCond);
        }
        else
        {
            for (int oi = 0; oi < orScope.Length; ++oi)
            {
                float energyCost = -9999f;

                Field orField = orScope[oi];
                if (orField.GetNameHash() == Hash_EnergyCost)
                {
                    energyCost = orField.GetFloat();
                }
                else if (orField.GetNameHash() == Hash_If || orField.GetNameHash() == Hash_Or)
                {
                    PhxComboTransitionCondition newCond = new PhxComboTransitionCondition();
                    CraCondition* and = &newCond.Or.And0;

                    int andIdx = 0;
                    if (energyCost > -9999f)
                    {
                        and[andIdx].Input = InEnergy;
                        and[andIdx].Type = CraConditionType.GreaterOrEqual;
                        and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = energyCost };
                        andIdx++;
                    }

                    Field[] andScope = orField.Scope.GetFields();
                    for (int ai = 0; ai < andScope.Length; ++ai)
                    {
                        if (andIdx >= 10)
                        {
                            Debug.LogError("Cra currently doesn't support more than 10 'And' conditions!");
                            break;
                        }

                        Field andField = andScope[ai];
                        if (andField.GetNameHash() == Hash_Button)
                        {
                            string comboButton = andField.GetString(0);
                            CraMachineValue input = InTabEvents;
                            if (andField.GetNumValues() > 1)
                            {
                                string buttonAction = andField.GetString(1);
                                switch (buttonAction)
                                {
                                    case "Down" : input = InDownEvents;    break;
                                    case "Press": input = InPressedEvents; break;
                                    case "Tab"  : input = InTabEvents;     break;
                                    case "Hold" : input = InHoldEvents;    break;
                                    default:
                                    {
                                        Debug.LogError($"Unknown/unsupported Button action: {buttonAction}");
                                        break;
                                    }
                                }
                            }

                            if (!PhxAnimUtils.StrToInput.TryGetValue(comboButton, out PhxInput button))
                            {
                                Debug.LogError($"Cannot resolve unknown Combo Button '{comboButton}' to a PhxInput!");
                                continue;
                            }

                            and[andIdx].Input = input;
                            and[andIdx].Type = CraConditionType.AllFlags;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)button };
                            andIdx++;
                        }
                        else if (andField.GetNameHash() == Hash_Posture)
                        {
                            PhxAnimPosture posture = PhxAnimPosture.None;
                            for (byte pi = 0; pi < andField.GetNumValues(); ++pi)
                            {
                                string postureStr = andField.GetString(pi);
                                bool negate = postureStr.StartsWith("!");
                                if (negate)
                                {
                                    postureStr = postureStr.Substring(1, postureStr.Length - 1);
                                }
                                if (!StrToPosture.TryGetValue(postureStr, out PhxAnimPosture p))
                                {
                                    Debug.LogError($"Unknown posture '{postureStr}' in transition condition!");
                                    continue;
                                }
                                if (negate)
                                {
                                    posture &= ~p;
                                }
                                else
                                {
                                    posture |= p;
                                }
                            }

                            if (posture == PhxAnimPosture.Jump)
                            {
                                posture |= PhxAnimPosture.Fall;
                            }
                            newCond.SourcePosture |= posture;

                            and[andIdx].Input = OutPosture;
                            and[andIdx].Type = CraConditionType.AnyFlag;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Int, ValueInt = (int)posture };
                            andIdx++;
                        }
                        else if (andField.GetNameHash() == Hash_Thrust)
                        {
                            string condStr = andField.GetString(0);
                            float condValue = andField.GetFloat(1);
                            if (!PhxAnimUtils.StrToCondition.TryGetValue(condStr, out CraConditionType cond))
                            {
                                Debug.LogError($"Unknown condition '{condStr}'!");
                                continue;
                            }

                            and[andIdx].Input = InThrustMagnitude;
                            and[andIdx].Type = cond;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = condValue };
                            andIdx++;
                        }
                        else if (andField.GetNameHash() == Hash_ThrustAngle)
                        {
                            float condMin = andField.GetFloat(0);
                            float condMax = andField.GetFloat(1);

                            and[andIdx].Input = InThrustAngle;
                            and[andIdx].Type = CraConditionType.GreaterOrEqual;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = condMin };
                            andIdx++;

                            and[andIdx].Input = InThrustAngle;
                            and[andIdx].Type = CraConditionType.LessOrEqual;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = condMax };
                            andIdx++;
                        }
                        else if (andField.GetNameHash() == Hash_Break)
                        {
                            float time = GetTimeValue(andField, stateDuration);
                            and[andIdx].Input = CraMachineValue.None;
                            and[andIdx].Type = CraConditionType.TimeMin;
                            and[andIdx].Compare = new CraValueUnion { Type = CraValueType.Float, ValueFloat = time };
                            andIdx++;
                        }
                        else if (andField.GetNameHash() == Hash_TimeStart)
                        {
                            float time = GetTimeValue(andField, stateDuration);
                            newCond.Or.CheckTimeMin = time;
                        }
                        else if (andField.GetNameHash() == Hash_TimeEnd)
                        {
                            float time = GetTimeValue(andField, stateDuration);
                            newCond.Or.CheckTimeMax = time;
                        }
                    }

                    if (andIdx > 0)
                    {
                        conditions.Add(newCond);
                    }
                }
            }
        }

        return conditions.ToArray();
    }

    float GetTimeValue(Field timeField, float duration)
    {
        float time = timeField.GetFloat(0);
        for (byte i = 1; i < timeField.GetNumValues(); i++)
        {
            string modeStr = timeField.GetString(i);
            if (PhxUtils.StrEquals(modeStr, "Frames"))
            {
                // convert from battlefront frames (30 fps) to time
                time /= 30f;
            }
            else if (PhxUtils.StrEquals(modeStr, "FromAnim"))
            {
                // time is a multiplier of duration
                time *= duration;
            }
            else if (PhxUtils.StrEquals(modeStr, "FromEnd"))
            {
                // start measuring time from end
                time = duration + time;
            }
        }
        return time;
    }

    void WriteParamsOnEnter(PhxScopedState state, in PhxStateParams param)
    {
        PhxAnimUtils.WriteIntOnEnter(state.Upper, OutPosture,    (int)param.Posture);
        PhxAnimUtils.WriteIntOnEnter(state.Upper, OutAimType,    (int)param.AimType);
        PhxAnimUtils.WriteIntOnEnter(state.Upper, OutAction,     (int)param.Action);
        PhxAnimUtils.WriteIntOnEnter(state.Upper, OutInputLocks, (int)param.Locked);
    }


    struct PhxComboTransitionCondition
    {
        public CraConditionOr Or;
        public PhxAnimPosture SourcePosture;
    }

    struct PhxComboTransitionCache
    {
        public PhxComboState SourceState;
        public Field TransitionField;
        public bool SourceStateIsIdle;
    }

    struct PhxComboState
    {
        public PhxScopedState State;
        public PhxAnimPosture Posture;
        public bool           IsAnimatedMove;
    }

    struct PhxStateParams
    {
        public PhxAnimPosture Posture;
        public PhxAimType     AimType;
        public PhxAnimAction  Action;
        public PhxInput       Locked;
    }

    static readonly uint Hash_State               = HashUtils.GetFNV("State");
    static readonly uint Hash_Duration            = HashUtils.GetFNV("Duration");
    static readonly uint Hash_Animation           = HashUtils.GetFNV("Animation");
    static readonly uint Hash_AimType             = HashUtils.GetFNV("AimType");
    static readonly uint Hash_BlendInTime         = HashUtils.GetFNV("BlendInTime");
    static readonly uint Hash_BlendOutTime        = HashUtils.GetFNV("BlendOutTime");
    static readonly uint Hash_EnergyRestoreRate   = HashUtils.GetFNV("EnergyRestoreRate");
    static readonly uint Hash_InputLock           = HashUtils.GetFNV("InputLock");
    static readonly uint Hash_Transition          = HashUtils.GetFNV("Transition");
    static readonly uint Hash_Sound               = HashUtils.GetFNV("Sound");
    static readonly uint Hash_Attack              = HashUtils.GetFNV("Attack");
    static readonly uint Hash_AttackId            = HashUtils.GetFNV("AttackId");
    static readonly uint Hash_Edge                = HashUtils.GetFNV("Edge");
    static readonly uint Hash_DamageTime          = HashUtils.GetFNV("DamageTime");
    static readonly uint Hash_Damage              = HashUtils.GetFNV("Damage");
    static readonly uint Hash_Push                = HashUtils.GetFNV("Push");
    static readonly uint Hash_DamageLength        = HashUtils.GetFNV("DamageLength");
    static readonly uint Hash_DamageWidth         = HashUtils.GetFNV("DamageWidth");
    static readonly uint Hash_AnimatedMove        = HashUtils.GetFNV("AnimatedMove");
    static readonly uint Hash_VelocityX           = HashUtils.GetFNV("VelocityX");
    static readonly uint Hash_VelocityZ           = HashUtils.GetFNV("VelocityZ");
    static readonly uint Hash_VelocityFromThrust  = HashUtils.GetFNV("VelocityFromThrust");
    static readonly uint Hash_VelocityFromStrafe  = HashUtils.GetFNV("VelocityFromStrafe");
    static readonly uint Hash_If                  = HashUtils.GetFNV("If");
    static readonly uint Hash_Or                  = HashUtils.GetFNV("Or");
    static readonly uint Hash_Until               = HashUtils.GetFNV("Until");
    static readonly uint Hash_Posture             = HashUtils.GetFNV("Posture");
    static readonly uint Hash_Button              = HashUtils.GetFNV("Button");
    static readonly uint Hash_Thrust              = HashUtils.GetFNV("Thrust");
    static readonly uint Hash_ThrustAngle         = HashUtils.GetFNV("ThrustAngle");
    static readonly uint Hash_TimeStart           = HashUtils.GetFNV("TimeStart");
    static readonly uint Hash_TimeEnd             = HashUtils.GetFNV("TimeEnd");
    static readonly uint Hash_Break               = HashUtils.GetFNV("Break");
    static readonly uint Hash_EnergyCost          = HashUtils.GetFNV("EnergyCost");
}