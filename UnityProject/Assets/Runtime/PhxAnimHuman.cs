using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using LibSWBF2.Enums;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;



// From some testing with Rifle / Bazooka / Pistol / Sabre
// +------------------+-------+-------+
// |    Animation     | Lower | Upper |
// +------------------+-------+-------+
// | Idle             | Rifle | Anim  |
// | Idle Full        | Anim  | Anim  |
// | Walk             | Rifle | Anim  |
// | Walk Full        | Rifle | Anim  |
// | Idle Alert       | Rifle | Anim  |
// | Idle Alert Full  | Anim  | Anim  |
// | Walk Alert       | Rifle | Anim  |
// | Walk Alert Full  | Rifle | Anim  |
// | Idle Shoot       | Rifle | Anim  |
// | Idle Shoot Full  | Rifle | Anim  |
// | Walk Shoot       | Rifle | Anim  |
// | Walk Shoot Full  | Rifle | Anim  |
// | Idle Attack      | Rifle | Anim  |
// | Idle Attack Full | Anim  | Anim  |
// | Walk Attack      | Rifle | Anim  |
// | Walk Attack Full | Rifle | Anim  |
// +------------------+-------+-------+



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

    // Full
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

    // Full
    public PhxScopedState ThrownBounceFrontSoft;
    public PhxScopedState ThrownBounceBackSoft;
    public PhxScopedState ThrownFlail;
    public PhxScopedState ThrownFlyingFront;
    public PhxScopedState ThrownFlyingBack;
    public PhxScopedState ThrownFlyingLeft;
    public PhxScopedState ThrownFlyingRight;
    public PhxScopedState ThrownLandFrontSoft;
    public PhxScopedState ThrownLandBackSoft;
    public PhxScopedState ThrownTumbleFront;
    public PhxScopedState ThrownTumbleBack;

    // Full
    public PhxScopedState Jump;
    public PhxScopedState Fall;
    public PhxScopedState LandSoft;
    public PhxScopedState LandHard;
    public PhxScopedState RollLeft;
    public PhxScopedState RollRight;
    public PhxScopedState Choking;

    // Lower
    public PhxScopedState JetpackHover;

    // Lower + Upper
    public PhxScopedState Sprint;
}

public enum PhxAnimTimeMode
{
    Seconds, Frames, FromAnim
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
    public CraLayer LayerLower { get; private set; }
    public CraLayer LayerUpper { get; private set; }

    public CraMachineValue InMovementX { get; private set; }
    public CraMachineValue InMovementY { get; private set; }
    public CraMachineValue InMagnitude { get; private set; }
    public CraMachineValue InTurnLeft { get; private set; }
    public CraMachineValue InTurnRight { get; private set; }
    public CraMachineValue InCrouch { get; private set; }
    public CraMachineValue InSprint { get; private set; }
    public CraMachineValue InJump { get; private set; }
    public CraMachineValue InShootPrimary { get; private set; }
    public CraMachineValue InShootSecondary { get; private set; }
    public CraMachineValue InReload { get; private set; }
    public CraMachineValue InEnergy { get; private set; }
    public CraMachineValue InGrounded { get; private set; }
    public CraMachineValue InMultiJump { get; private set; }
    public CraMachineValue InLandHardness { get; private set; }

    public CraMachineValue OutPosture { get; private set; }
    public CraMachineValue OutStrafeBackwards { get; private set; }
    public CraMachineValue OutIsReloading { get; private set; }
    public PhxAnimAttackOutput[] OutAttacks { get; private set; }


    PhxAnimHumanSet[] Sets;
    byte ActiveSet;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponAnimToSetIdx;

    const float Deadzone = 0.05f;
    Dictionary<string, CraMachineValue> ComboButtons = new Dictionary<string, CraMachineValue>();

    static Dictionary<string, PhxAnimPosture> StrToPosture = new Dictionary<string, PhxAnimPosture>()
    {
        { "All", PhxAnimPosture.All },
        { "Any", PhxAnimPosture.All },

        { "Stand",     PhxAnimPosture.Stand     },
        { "Crouch",    PhxAnimPosture.Crouch    },
        { "Prone",     PhxAnimPosture.Prone     },
        { "Sprint",    PhxAnimPosture.Sprint    },
        { "Jump",      PhxAnimPosture.Jump      },
        { "RollLeft",  PhxAnimPosture.RollLeft  },
        { "RollRight", PhxAnimPosture.RollRight },
        { "Roll",      PhxAnimPosture.Roll      },
        { "Jet",       PhxAnimPosture.Jet       },
    };


    public unsafe PhxAnimHuman(PhxAnimationResolver resolver, Transform root, string characterAnimBank, PhxAnimWeapon[] weapons)
    {
        Debug.Assert(weapons != null);
        Debug.Assert(weapons.Length > 0);

        Machine = CraStateMachine.CreateNew();
        WeaponAnimToSetIdx = new Dictionary<string, int>();

        Resolver = resolver;

        LayerLower = Machine.NewLayer();
        LayerUpper = Machine.NewLayer();

        InMovementX = Machine.NewMachineValue(CraValueType.Float, "Movement X");
        InMovementY = Machine.NewMachineValue(CraValueType.Float, "Movement Y");
        InMagnitude = Machine.NewMachineValue(CraValueType.Float, "Magnitude");
        InTurnLeft = Machine.NewMachineValue(CraValueType.Trigger, "Turn Left");
        InTurnRight = Machine.NewMachineValue(CraValueType.Trigger, "Turn Right");
        InCrouch = Machine.NewMachineValue(CraValueType.Bool, "Crouch");
        InSprint = Machine.NewMachineValue(CraValueType.Bool, "Sprint");
        InJump = Machine.NewMachineValue(CraValueType.Trigger, "Jump");
        InEnergy = Machine.NewMachineValue(CraValueType.Float, "Energy");
        InShootPrimary = Machine.NewMachineValue(CraValueType.Trigger, "Shoot Primary");
        InShootSecondary = Machine.NewMachineValue(CraValueType.Trigger, "Shoot Secondary");
        InReload = Machine.NewMachineValue(CraValueType.Trigger, "Reload");
        InGrounded = Machine.NewMachineValue(CraValueType.Bool, "Grounded");
        InMultiJump = Machine.NewMachineValue(CraValueType.Bool, "Multi Jump");
        InLandHardness = Machine.NewMachineValue(CraValueType.Int, "Land Hardness");

        ComboButtons = new Dictionary<string, CraMachineValue>
        {
            { "Fire", InShootPrimary },
            { "FireSecondary", InShootSecondary },
        };

        OutPosture = Machine.NewMachineValue(CraValueType.Int, "Posture");
        OutStrafeBackwards = Machine.NewMachineValue(CraValueType.Bool, "Strafe Backwards");
        OutIsReloading = Machine.NewMachineValue(CraValueType.Bool, "Is Reloading");

        OutAttacks = new PhxAnimAttackOutput[4];
        for (int i = 0; i < OutAttacks.Length; ++i)
        {
            OutAttacks[i].AttackID = Machine.NewMachineValue(CraValueType.Int, $"[{i}] Attack ID");
            OutAttacks[i].AttackEdge = Machine.NewMachineValue(CraValueType.Int, $"[{i}] Attack Edge");
            OutAttacks[i].AttackDamageTimeStart = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Time Start");
            OutAttacks[i].AttackDamageTimeEnd = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Time End");
            OutAttacks[i].AttackDamageTimeMode = Machine.NewMachineValue(CraValueType.Int, $"[{i}] Attack Damage Time Mode");
            OutAttacks[i].AttackDamageLength = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Length");
            OutAttacks[i].AttackDamageLengthFromEdge = Machine.NewMachineValue(CraValueType.Bool, $"[{i}] Attack Damage Length From Edge");
            OutAttacks[i].AttackDamageWidth = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage Width");
            OutAttacks[i].AttackDamageWidthFromEdge = Machine.NewMachineValue(CraValueType.Bool, $"[{i}] Attack Damage Width From Edge");
            OutAttacks[i].AttackDamage = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack Damage");
            OutAttacks[i].AttackPush = Machine.NewMachineValue(CraValueType.Float, $"[{i}] Attack IPush");

            OutAttacks[i].AttackID.SetInt(-1);
        }

        Sets = new PhxAnimHumanSet[weapons.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            WeaponAnimToSetIdx.Add(weapons[i].AnimationBank, i);
            Sets[i] = GenerateSet(root, characterAnimBank, weapons[i]);

            // TODO: Do not generate reload states when current weapon doesn't have/support reload (same for Alert)
            Transitions_Stand(in Sets[i]);
            Transitions_StandReload(in Sets[i]);
            Transitions_StandTurn(in Sets[i]);
            Transitions_StandToFall(in Sets[i]);
            Transitions_StandToCrouch(in Sets[i]);
            Transitions_Crouch(in Sets[i]);
            Transitions_CrouchReload(in Sets[i]);
            Transitions_CrouchTurn(in Sets[i]);
            Transitions_CrouchToStand(in Sets[i]);
            Transitions_CrouchToFall(in Sets[i]);
            Transitions_Sprint(in Sets[i]);
            Transitions_Jump(in Sets[i]);
            Transitions_Land(in Sets[i]);
        }

        LayerLower.SetActiveState(Sets[ActiveSet].StandIdle.Lower);
        LayerUpper.SetActiveState(Sets[ActiveSet].StandIdle.Upper);
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
        Transition(set.StandIdle, set.StandWalkForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                }
            }
        );

        Transition(set.StandWalkForward, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.StandWalkForward, set.StandRunForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.StandRunForward, set.StandWalkForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.StandIdle, set.StandRunBackward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            }
        );

        Transition(set.StandWalkForward, set.StandRunBackward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            }
        );

        Transition(set.StandRunBackward, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.StandRunBackward, set.StandWalkForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
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
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.StandReload, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.StandReload.Lower, set.StandWalkForward.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                }
            }
        );

        Transition(set.StandReload.Lower, set.StandRunBackward.Lower, 0.15f,
            new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            }
        );

        Transition(set.StandWalkForward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.StandRunForward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.StandRunBackward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandWalkForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandRunForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.StandReload.Upper, set.StandRunBackward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
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
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
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
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
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
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.StandTurnRight.Lower, set.StandIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
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
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                }
            }
        );

        Transition(set.CrouchWalkForward, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.CrouchIdle, set.CrouchWalkBackward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            }
        );

        Transition(set.CrouchWalkBackward, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
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
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.CrouchReload, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.CrouchReload.Lower, set.CrouchWalkForward.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                }
            }
        );

        Transition(set.CrouchReload.Lower, set.CrouchWalkBackward.Lower, 0.15f,
            new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            }
        );

        Transition(set.CrouchWalkForward.Upper, set.CrouchReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.CrouchWalkBackward.Upper, set.CrouchReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InReload
                }
            }
        );

        Transition(set.CrouchReload.Upper, set.CrouchWalkForward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                }
            }
        );

        Transition(set.CrouchReload.Upper, set.CrouchWalkBackward.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
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
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
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
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
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
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.CrouchTurnRight.Lower, set.CrouchIdle.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
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
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.StandWalkForward, set.CrouchWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.StandRunForward, set.CrouchWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
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
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                }
            }
        );

        Transition(set.CrouchWalkForward, set.StandWalkForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.CrouchWalkForward, set.StandRunForward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            }
        );

        Transition(set.CrouchWalkBackward, set.StandRunBackward, 0.25f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
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
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InSprint,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                }
            }
        );

        Transition(set.Sprint, set.StandRunForward, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InSprint,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
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
                    Type = CraConditionType.Trigger,
                    Input = InJump,
                }
            }
        );

        Transition(set.StandWalkForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InJump,
                }
            }
        );

        Transition(set.StandRunForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InJump,
                }
            }
        );

        Transition(set.StandRunBackward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InJump,
                }
            }
        );

        Transition(set.Sprint, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InJump,
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InJump,
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
                },
                And1 = new CraCondition
                {
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InLandHardness,
                    Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = 1 }
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
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InLandHardness,
                    Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = 2 }
                }
            }
        );

        Transition(set.LandSoft, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished,
                }
            }
        );

        Transition(set.LandHard, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished,
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
            Debug.LogError($"Couldn't resolve {inputDesc}!");
            return res;
        }
        Debug.Assert(clip.IsValid());
        Debug.Assert(animScope != PhxAnimScope.None);

        if (animScope == PhxAnimScope.Upper)
        {
            animDesc.Scope = null;
            if (!Resolver.ResolveAnim(ref animDesc, out CraClip clipRifle, out _, PhxAnimScope.Lower))
            {
                Debug.LogError($"Couldn't resolve {inputDesc} on scope Lower!");
                return res;
            }
            res.Lower = CreateState(root, clipRifle, loop, PhxAnimScope.Lower);
        }
        else
        {
            res.Lower = CreateState(root, clip, loop, PhxAnimScope.Lower);
        }
        res.Upper = CreateState(root, clip, loop, PhxAnimScope.Upper);

        Debug.Assert(res.Lower.IsValid());
        Debug.Assert(res.Upper.IsValid());

        return res;
    }

    PhxAnimHumanSet GenerateSet(Transform root, string character, PhxAnimWeapon weapon)
    {
        // TODO: Do not generate Reload/Alert states if not supported!
        string weaponName = weapon.AnimationBank;
        PhxAnimHumanSet set = new PhxAnimHumanSet();

        set.CrouchIdle = CreateScopedState(root, character, weaponName, "crouch", "idle_emote", true);
        set.CrouchIdleTakeknee = CreateScopedState(root, character, weaponName, "crouch", "idle_takeknee", false);
        set.CrouchHitFront = CreateScopedState(root, character, weaponName, "crouch", "hitfront", false);
        set.CrouchHitLeft = CreateScopedState(root, character, weaponName, "crouch", "hitleft", false);
        set.CrouchHitRight = CreateScopedState(root, character, weaponName, "crouch", "hitright", false);
        set.CrouchReload = CreateScopedState(root, character, weaponName, "crouch", "reload", false);
        set.CrouchShoot = CreateScopedState(root, character, weaponName, "crouch", "shoot", false);
        set.CrouchTurnLeft = CreateScopedState(root, character, weaponName, "crouch", "turnleft", false);
        set.CrouchTurnRight = CreateScopedState(root, character, weaponName, "crouch", "turnright", false);
        set.CrouchWalkForward = CreateScopedState(root, character, weaponName, "crouch", "walkforward", true);
        set.CrouchWalkBackward = CreateScopedState(root, character, weaponName, "crouch", "walkbackward", true);
        set.CrouchAlertIdle = CreateScopedState(root, character, weaponName, "crouchalert", "idle_emote", true);
        set.CrouchAlertWalkForward = CreateScopedState(root, character, weaponName, "crouchalert", "walkforward", true);
        set.CrouchAlertWalkBackward = CreateScopedState(root, character, weaponName, "crouchalert", "walkbackward", true);

        set.StandIdle = CreateScopedState(root, character, weaponName, "stand", "idle_emote", true);
        set.StandIdleCheckweapon = CreateScopedState(root, character, weaponName, "stand", "idle_checkweapon", false);
        set.StandIdleLookaround = CreateScopedState(root, character, weaponName, "stand", "idle_lookaround", false);
        set.StandWalkForward = CreateScopedState(root, character, weaponName, "stand", "walkforward", true);
        set.StandRunForward = CreateScopedState(root, character, weaponName, "stand", "runforward", true);
        set.StandRunBackward = CreateScopedState(root, character, weaponName, "stand", "runbackward", true);
        set.StandReload = CreateScopedState(root, character, weaponName, "stand", "reload", false);
        set.StandShootPrimary = CreateScopedState(root, character, weaponName, "stand", "shoot", false);
        set.StandShootSecondary = CreateScopedState(root, character, weaponName, "stand", "shoot_secondary", false);
        set.StandAlertIdle = CreateScopedState(root, character, weaponName, "standalert", "idle_emote", true);
        set.StandAlertWalkForward = CreateScopedState(root, character, weaponName, "standalert", "walkforward", true);
        set.StandAlertRunForward = CreateScopedState(root, character, weaponName, "standalert", "runforward", true);
        set.StandAlertRunBackward = CreateScopedState(root, character, weaponName, "standalert", "runbackward", true);
        set.StandTurnLeft = CreateScopedState(root, character, weaponName, "stand", "turnleft", false);
        set.StandTurnRight = CreateScopedState(root, character, weaponName, "stand", "turnright", false);
        set.StandHitFront = CreateScopedState(root, character, weaponName, "stand", "hitfront", false);
        set.StandHitBack = CreateScopedState(root, character, weaponName, "stand", "hitback", false);
        set.StandHitLeft = CreateScopedState(root, character, weaponName, "stand", "hitleft", false);
        set.StandHitRight = CreateScopedState(root, character, weaponName, "stand", "hitright", false);
        set.StandGetupFront = CreateScopedState(root, character, weaponName, "stand", "getupfront", false);
        set.StandGetupBack = CreateScopedState(root, character, weaponName, "stand", "getupback", false);
        set.StandDeathForward = CreateScopedState(root, character, weaponName, "stand", "death_forward", false);
        set.StandDeathBackward = CreateScopedState(root, character, weaponName, "stand", "death_backward", false);
        set.StandDeathLeft = CreateScopedState(root, character, weaponName, "stand", "death_left", false);
        set.StandDeathRight = CreateScopedState(root, character, weaponName, "stand", "death_right", false);
        set.StandDeadhero = CreateScopedState(root, character, weaponName, "stand", "idle_emote", false);

        set.ThrownBounceFrontSoft = CreateScopedState(root, character, weaponName, "thrown", "bouncefrontsoft", false);
        set.ThrownBounceBackSoft = CreateScopedState(root, character, weaponName, "thrown", "bouncebacksoft", false);
        set.ThrownFlail = CreateScopedState(root, character, weaponName, "thrown", "flail", false);
        set.ThrownFlyingFront = CreateScopedState(root, character, weaponName, "thrown", "flyingfront", false);
        set.ThrownFlyingBack = CreateScopedState(root, character, weaponName, "thrown", "flyingback", false);
        set.ThrownFlyingLeft = CreateScopedState(root, character, weaponName, "thrown", "flyingleft", false);
        set.ThrownFlyingRight = CreateScopedState(root, character, weaponName, "thrown", "flyingright", false);
        set.ThrownLandFrontSoft = CreateScopedState(root, character, weaponName, "thrown", "landfrontsoft", false);
        set.ThrownLandBackSoft = CreateScopedState(root, character, weaponName, "thrown", "landbacksoft", false);
        set.ThrownTumbleFront = CreateScopedState(root, character, weaponName, "thrown", "tumblefront", false);
        set.ThrownTumbleBack = CreateScopedState(root, character, weaponName, "thrown", "tumbleback", false);

        set.Sprint = CreateScopedState(root, character, weaponName, "sprint", null, true);
        set.JetpackHover = CreateScopedState(root, character, weaponName, "jetpack_hover", null, true);
        set.Jump = CreateScopedState(root, character, weaponName, "jump", null, false);
        set.Fall = CreateScopedState(root, character, weaponName, "fall", null, true);
        set.LandSoft = CreateScopedState(root, character, weaponName, "landsoft", null, false);
        set.LandHard = CreateScopedState(root, character, weaponName, "landhard", null, false);

        WriteIntOnEnter(set.CrouchIdle, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchIdleTakeknee, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchHitFront, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchHitLeft, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchHitRight, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchReload, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchShoot, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchTurnLeft, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchTurnRight, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchWalkForward, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchWalkBackward, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchAlertIdle, OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchAlertWalkForward , OutPosture, (int)PhxAnimPosture.Crouch);
        WriteIntOnEnter(set.CrouchAlertWalkBackward, OutPosture, (int)PhxAnimPosture.Crouch);

        WriteIntOnEnter(set.StandIdle, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandIdleCheckweapon, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandIdleLookaround, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandWalkForward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandRunForward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandRunBackward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandReload, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandShootPrimary, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandShootSecondary, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandAlertIdle, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandAlertWalkForward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandAlertRunForward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandAlertRunBackward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandTurnLeft, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandTurnRight, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandHitFront, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandHitBack, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandHitLeft, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandHitRight, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandGetupFront, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandGetupBack, OutPosture, (int)PhxAnimPosture.Stand);        
        WriteIntOnEnter(set.StandDeathForward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandDeathBackward, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandDeathLeft, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandDeathRight, OutPosture, (int)PhxAnimPosture.Stand);
        WriteIntOnEnter(set.StandDeadhero, OutPosture, (int)PhxAnimPosture.Stand);

        WriteIntOnEnter(set.ThrownBounceFrontSoft, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownBounceBackSoft, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownFlail, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownFlyingFront, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownFlyingBack, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownFlyingLeft, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownFlyingRight, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownLandFrontSoft, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownLandBackSoft, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownTumbleFront, OutPosture, (int)PhxAnimPosture.Thrown);
        WriteIntOnEnter(set.ThrownTumbleBack, OutPosture, (int)PhxAnimPosture.Thrown);

        WriteIntOnEnter(set.Sprint, OutPosture, (int)PhxAnimPosture.Sprint);
        WriteIntOnEnter(set.JetpackHover, OutPosture, (int)PhxAnimPosture.Jet);
        WriteIntOnEnter(set.Jump, OutPosture, (int)PhxAnimPosture.Jump);
        WriteIntOnEnter(set.Fall, OutPosture, (int)PhxAnimPosture.Fall);
        WriteIntOnEnter(set.LandSoft, OutPosture, (int)PhxAnimPosture.Land);
        WriteIntOnEnter(set.LandHard, OutPosture, (int)PhxAnimPosture.Land);


        WriteBoolOnEnter(set.StandRunBackward, OutStrafeBackwards, true);
        WriteBoolOnEnter(set.StandAlertRunBackward, OutStrafeBackwards, true);
        WriteBoolOnEnter(set.CrouchAlertWalkBackward, OutStrafeBackwards, true);
        WriteBoolOnLeave(set.StandRunBackward, OutStrafeBackwards, false);
        WriteBoolOnLeave(set.StandAlertRunBackward, OutStrafeBackwards, false);
        WriteBoolOnLeave(set.CrouchAlertWalkBackward, OutStrafeBackwards, false);

        WriteBoolOnEnter(set.StandReload, OutIsReloading, true);
        WriteBoolOnEnter(set.CrouchReload, OutIsReloading, true);
        WriteBoolOnLeave(set.StandReload, OutIsReloading, false);
        WriteBoolOnLeave(set.CrouchReload, OutIsReloading, false);


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

    void WriteIntOnEnter(PhxScopedState state, CraMachineValue machineValue, int value)
    {
        WriteIntOnEnter(state.Lower, machineValue, value);
        WriteIntOnEnter(state.Upper, machineValue, value);
    }

    void WriteIntOnEnter(CraState state, CraMachineValue machineValue, int value)
    {
        state.WriteOnEnter(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = value } });
    }

    void WriteBoolOnEnter(PhxScopedState state, CraMachineValue machineValue, bool value)
    {
        WriteBoolOnEnter(state.Lower, machineValue, value);
        WriteBoolOnEnter(state.Upper, machineValue, value);
    }

    void WriteBoolOnLeave(CraState state, CraMachineValue machineValue, bool value)
    {
        state.WriteOnLeave(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = value } });
    }

    void WriteBoolOnLeave(PhxScopedState state, CraMachineValue machineValue, bool value)
    {
        WriteBoolOnLeave(state.Lower, machineValue, value);
        WriteBoolOnLeave(state.Upper, machineValue, value);
    }

    void WriteBoolOnEnter(CraState state, CraMachineValue machineValue, bool value)
    {
        state.WriteOnEnter(new CraWriteOutput { MachineValue = machineValue, Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = value } });
    }

    unsafe void CreateCombo(in PhxAnimHumanSet set, Transform root, string character, string weapon, string comboName)
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
            // to store all the transitions and apply them AFTER the full combo has been parsed.
            List<PhxComboTransitionCache> Transitions = new List<PhxComboTransitionCache>();
            Dictionary<string, PhxComboState> ComboStates = new Dictionary<string, PhxComboState>();

            Field[] fields = combo.GetFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                Field field = fields[i];
                if (field.GetNameHash() == Hash_State)
                {
                    string stateName = field.GetString();
                    bool isIdle = stateName.ToLower() == "idle";

                    if (ComboStates.ContainsKey(stateName))
                    {
                        Debug.LogError($"State '{stateName}' is defined more than once in combo '{comboName}'!");
                    }
                    if (!CreateComboState(root, character, weapon, comboName, field, out PhxComboState comboState) && !isIdle)
                    {
                        Debug.LogError($"Failed to create combo state '{stateName}' in Combo '{comboName}'!");
                        continue;
                    }

                    if (isIdle)
                    {
                        comboState.State = set.StandIdle;
                    }

                    List<PhxComboTransitionCache> localTransitions = new List<PhxComboTransitionCache>();

                    int attackCount = 0;
                    Field[] stateFields = field.Scope.GetFields();
                    for (int j = 0; j < stateFields.Length; ++j)
                    {
                        Field stateField = stateFields[j];
                        if (stateField.GetNameHash() == Hash_Duration)
                        {
                            float time = stateField.GetFloat(0);
                            if (time <= 0f)
                            {
                                comboState.State.Lower.GetPlayer().SetLooping(true);
                                comboState.State.Upper.GetPlayer().SetLooping(true);
                            }
                            else
                            {
                                bool asFrames = stateField.GetNumValues() > 1 && stateField.GetString(1).ToLower() == "frames";
                                if (asFrames)
                                {
                                    // convert from battlefront frames (30 fps) to time
                                    time /= 30f;
                                }
                                CraPlayRange range = new CraPlayRange { MinTime = 0f, MaxTime = time };
                                comboState.State.Lower.GetPlayer().SetPlayRange(range);
                                comboState.State.Upper.GetPlayer().SetPlayRange(range);
                            }
                        }
                        else if (stateField.GetNameHash() == Hash_Transition)
                        {
                            localTransitions.Add(new PhxComboTransitionCache 
                            { 
                                TargetStateField = stateField, 
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
                        else if (stateField.GetNameHash() == Hash_Attack)
                        {
                            if (attackCount >= 4)
                            {
                                Debug.LogError($"PhxAnimHuman does currently not support more thasn 4 Attacks!");
                                continue;
                            }

                            int   id = 0;
                            int   edge = 0;
                            float timeStart = 0.2f;
                            float timeEnd = 0.3f;
                            int   timeMode = 2; // default: FromAnim
                            float length = 1f;
                            bool  lengthFromEdge = true;
                            float width = 1f;
                            bool  widthFromEdge = true;
                            float damage = 0f;
                            float push = 0f;

                            Field[] attackFields = stateField.Scope.GetFields();
                            for (int ai = 0; ai < attackFields.Length; ++ai)
                            {
                                Field attackField = attackFields[ai];
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
                                            default: Debug.LogError($"Unknown time mode '{mode}'!");   break;
                                        }
                                    }
                                }
                                else if (attackField.GetNameHash() == Hash_DamageLength)
                                {
                                    length = attackField.GetFloat(0);
                                    lengthFromEdge = attackField.GetNumValues() > 1 && attackField.GetString(1) == "FromEdge";
                                }
                                else if (attackField.GetNameHash() == Hash_DamageWidth)
                                {
                                    width = attackField.GetFloat(0);
                                    widthFromEdge = attackField.GetNumValues() > 1 && attackField.GetString(1) == "FromEdge";
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
                        lt.SourceState = comboState.State;
                        localTransitions[j] = lt;
                    }
                    ComboStates.Add(stateName, comboState);
                    Transitions.AddRange(localTransitions);
                }
            }

            // Posture transitions State --> Walk
            foreach (var comboState in ComboStates)
            {
                PhxComboState state = comboState.Value;

                if ((state.Posture & PhxAnimPosture.Stand) != 0)
                {
                    // TODO: Add transitions to all lower walk states and back
                    // Make sure lower walk states can't transition further (e.g. to sprint or crouch)

                    Transition(state.State.Lower, set.StandWalkForward.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Greater,
                                Input = InMovementX,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                                CompareToAbsolute = true
                            }
                        },
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Greater,
                                Input = InMovementY,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                            }
                        }
                    );

                    Transition(state.State.Lower, set.StandRunBackward.Lower, 0.15f,
                        new CraConditionOr
                        {
                            And0 = new CraCondition
                            {
                                Type = CraConditionType.Less,
                                Input = InMovementY,
                                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                            }
                        }
                    );
                }
            }

            for (int ti = 0; ti < Transitions.Count; ++ti)
            {
                if (!Transitions[ti].SourceState.Lower.IsValid() || !Transitions[ti].SourceState.Upper.IsValid())
                {
                    // TODO: This should never happen later on
                    continue;
                }

                PhxComboState targetState = new PhxComboState();
                targetState.State = set.StandIdle;

                string targetStateName = Transitions[ti].TargetStateField.GetString();
                if (targetStateName.ToLower() != "idle")
                {
                    if (!ComboStates.TryGetValue(targetStateName, out targetState))
                    {
                        Debug.LogError($"Tried to transition to unknown state '{targetStateName}' in combo '{comboName}'!");
                        continue;
                    }
                }

                if (!targetState.State.Lower.IsValid() || !targetState.State.Upper.IsValid())
                {
                    // TODO: This should never happen later on
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
                CheckSameAnimation(Transitions[ti].SourceState.Lower, targetState.State.Lower);
                CheckSameAnimation(Transitions[ti].SourceState.Upper, targetState.State.Upper);

                CraConditionOr[] conditions = GetComboTransitionConditions(Transitions[ti].TargetStateField.Scope);
                Transition(Transitions[ti].SourceState, targetState.State, 0.15f, conditions);

                // IDLE is a special state that corresponds to the entire postures the Combo state is allowed in
                if (Transitions[ti].SourceStateIsIdle)
                {
                    // Walk --> State
                    if ((targetState.Posture & PhxAnimPosture.Stand) != 0)
                    {
                        Transition(set.StandWalkForward.Upper, targetState.State.Upper, 0.15f,
                            conditions
                        );

                        Transition(set.StandRunForward.Upper, targetState.State.Upper, 0.15f,
                            conditions
                        );

                        Transition(set.StandRunBackward.Upper, targetState.State.Upper, 0.15f,
                            conditions
                        );
                    }
                }
            }
        }
    }

    bool CreateComboState(Transform root, string character, string weapon, string comboName, Field state, out PhxComboState comboState)
    {
        comboState = new PhxComboState();

        string stateName = state.GetString();
        Field[] stateFields = state.Scope.GetFields();
        for (int i = 0; i < stateFields.Length; ++i)
        {
            Field stateField = stateFields[i];
            if (stateField.GetNameHash() == Hash_Animation)
            {
                string animname = stateField.GetString();
                string[] split = animname.Split(new char[] { '_' }, 2);
                comboState.State = CreateScopedState(root, character, weapon, split[0], split[1], true);
                if (comboState.State.Lower.IsValid() && comboState.State.Upper.IsValid())
                {
                    comboState.State.Lower.GetPlayer().SetLooping(false);
                    comboState.State.Upper.GetPlayer().SetLooping(false);
#if UNITY_EDITOR
                    if (comboState.State.Lower.IsValid()) comboState.State.Lower.SetName($"COMBO Lower {weapon} {stateName}");
                    if (comboState.State.Upper.IsValid()) comboState.State.Upper.SetName($"COMBO Upper {weapon} {stateName}");
#endif
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

    unsafe CraConditionOr[] GetComboTransitionConditions(Scope transitionScope)
    {
        List<CraConditionOr> Ors = new List<CraConditionOr>();
        Field[] orFields = transitionScope.GetFields();
        if (orFields.Length == 0)
        {
            CraConditionOr newOr = new CraConditionOr();
            newOr.And0 = new CraCondition
            {
                Type = CraConditionType.IsFinished
            };
            Ors.Add(newOr);
        }
        else
        {
            for (int oi = 0; oi < orFields.Length; ++oi)
            {
                Field orField = orFields[oi];
                if (orField.GetNameHash() == Hash_If || orField.GetNameHash() == Hash_Or)
                {
                    CraConditionOr newOr = new CraConditionOr();
                    CraCondition* and = &newOr.And0;

                    int andIdx = 0;
                    Field[] andFields = orField.Scope.GetFields();
                    for (int ai = 0; ai < andFields.Length; ++ai)
                    {
                        if (andIdx >= 10)
                        {
                            Debug.LogError("Cra currently doesn't support more than 10 'And' conditions!");
                            break;
                        }

                        Field andField = andFields[ai];
                        if (andField.GetNameHash() == Hash_Button)
                        {
                            string comboButton = andField.GetString();
                            if (!ComboButtons.TryGetValue(comboButton, out CraMachineValue input))
                            {
                                Debug.LogError($"Cannot resolve unknown Combo Button '{comboButton}' to a CraInput!");
                                continue;
                            }

                            and[andIdx].Input = input;
                            and[andIdx].Type = CraConditionType.Trigger;
                            andIdx++;
                        }
                    }

                    if (andIdx > 0)
                    {
                        Ors.Add(newOr);
                    }
                }
            }
        }

        return Ors.ToArray();
    }

    struct PhxComboTransitionCache
    {
        public PhxScopedState SourceState;
        public Field TargetStateField;
        public bool SourceStateIsIdle;
    }

    struct PhxComboState
    {
        public PhxScopedState State;
        public PhxAnimPosture Posture;
    }

    static readonly uint Hash_State = HashUtils.GetFNV("State");
    static readonly uint Hash_Duration = HashUtils.GetFNV("Duration");
    static readonly uint Hash_Animation = HashUtils.GetFNV("Animation");
    static readonly uint Hash_EnergyRestoreRate = HashUtils.GetFNV("EnergyRestoreRate");
    static readonly uint Hash_InputLock = HashUtils.GetFNV("InputLock");
    static readonly uint Hash_Transition = HashUtils.GetFNV("Transition");
    static readonly uint Hash_Attack = HashUtils.GetFNV("Attack");
    static readonly uint Hash_AttackId = HashUtils.GetFNV("AttackId");
    static readonly uint Hash_Edge = HashUtils.GetFNV("Edge");
    static readonly uint Hash_DamageTime = HashUtils.GetFNV("DamageTime");
    static readonly uint Hash_Damage = HashUtils.GetFNV("Damage");
    static readonly uint Hash_Push = HashUtils.GetFNV("Push");
    static readonly uint Hash_DamageLength = HashUtils.GetFNV("DamageLength");
    static readonly uint Hash_DamageWidth = HashUtils.GetFNV("DamageWidth");
    static readonly uint Hash_If = HashUtils.GetFNV("If");
    static readonly uint Hash_Or = HashUtils.GetFNV("Or");
    static readonly uint Hash_Posture = HashUtils.GetFNV("Posture");
    static readonly uint Hash_Button = HashUtils.GetFNV("Button");
}