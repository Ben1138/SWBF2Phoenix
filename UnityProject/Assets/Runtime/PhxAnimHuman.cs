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

public class PhxAnimHuman
{
    PhxEnvironment Env => PhxGame.GetEnvironment();

    public CraStateMachine Machine { get; private set; }
    public CraLayer LayerLower { get; private set; }
    public CraLayer LayerUpper { get; private set; }

    public CraInput InputMovementX { get; private set; }
    public CraInput InputMovementY { get; private set; }
    public CraInput InputMagnitude { get; private set; }
    public CraInput InputTurnLeft { get; private set; }
    public CraInput InputTurnRight { get; private set; }
    public CraInput InputCrouch { get; private set; }
    public CraInput InputSprint { get; private set; }
    public CraInput InputJump { get; private set; }
    public CraInput InputShootPrimary { get; private set; }
    public CraInput InputShootSecondary { get; private set; }
    public CraInput InputReload { get; private set; }
    public CraInput InputEnergy { get; private set; }
    public CraInput InputGrounded { get; private set; }
    public CraInput InputMultiJump { get; private set; }
    public CraInput InputLandHardness { get; private set; }

    public CraOutput OutputPosture { get; private set; }
    public CraOutput OutputStrafeBackwards { get; private set; }
    public CraOutput OutputIsReloading { get; private set; }


    PhxAnimHumanSet[] Sets;
    byte ActiveSet;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponAnimToSetIdx;

    const float Deadzone = 0.05f;
    Dictionary<string, CraInput> ComboButtons = new Dictionary<string, CraInput>();


    public PhxAnimHuman(PhxAnimationResolver resolver, Transform root, string characterAnimBank, PhxAnimWeapon[] weapons)
    {
        Debug.Assert(weapons != null);
        Debug.Assert(weapons.Length > 0);

        Machine = CraStateMachine.CreateNew();
        WeaponAnimToSetIdx = new Dictionary<string, int>();

        Resolver = resolver;

        LayerLower = Machine.NewLayer();
        LayerUpper = Machine.NewLayer();

        InputMovementX = Machine.NewInput(CraValueType.Float, "Movement X");
        InputMovementY = Machine.NewInput(CraValueType.Float, "Movement Y");
        InputMagnitude = Machine.NewInput(CraValueType.Float, "Magnitude");
        InputTurnLeft = Machine.NewInput(CraValueType.Trigger, "Turn Left");
        InputTurnRight = Machine.NewInput(CraValueType.Trigger, "Turn Right");
        InputCrouch = Machine.NewInput(CraValueType.Bool, "Crouch");
        InputSprint = Machine.NewInput(CraValueType.Bool, "Sprint");
        InputJump = Machine.NewInput(CraValueType.Trigger, "Jump");
        InputEnergy = Machine.NewInput(CraValueType.Float, "Energy");
        InputShootPrimary = Machine.NewInput(CraValueType.Trigger, "Shoot Primary");
        InputShootSecondary = Machine.NewInput(CraValueType.Trigger, "Shoot Secondary");
        InputReload = Machine.NewInput(CraValueType.Trigger, "Reload");
        InputGrounded = Machine.NewInput(CraValueType.Bool, "Grounded");
        InputMultiJump = Machine.NewInput(CraValueType.Bool, "Multi Jump");
        InputLandHardness = Machine.NewInput(CraValueType.Int, "Land Hardness");

        ComboButtons = new Dictionary<string, CraInput>
        {
            { "Fire", InputShootPrimary },
            { "FireSecondary", InputShootSecondary },
        };

        OutputPosture = Machine.NewOutput(CraValueType.Int, "Posture");
        OutputStrafeBackwards = Machine.NewOutput(CraValueType.Bool, "Strafe Backwards");
        OutputIsReloading = Machine.NewOutput(CraValueType.Bool, "Is Reloading");

        Sets = new PhxAnimHumanSet[weapons.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            bool weaponSupportsAlert = true;

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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputMovementY,
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
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputReload
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputMovementY,
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
                    Input = InputReload
                }
            }
        );

        Transition(set.StandRunForward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputReload
                }
            }
        );

        Transition(set.StandRunBackward.Upper, set.StandReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputReload
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputTurnLeft
                }
            }
        );

        Transition(set.StandIdle.Lower, set.StandTurnRight.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputTurnRight
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputReload
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputMovementY,
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
                    Input = InputReload
                }
            }
        );

        Transition(set.CrouchWalkBackward.Upper, set.CrouchReload.Upper, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputReload
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
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputTurnLeft
                }
            }
        );

        Transition(set.CrouchIdle.Lower, set.CrouchTurnRight.Lower, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputTurnRight
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
                    Input = InputCrouch,
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
                    Input = InputCrouch,
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
                    Input = InputCrouch,
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
                    Input = InputCrouch,
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
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementX,
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
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
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
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
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
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputSprint,
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
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            },
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputSprint,
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
                    Input = InputJump,
                }
            }
        );

        Transition(set.StandWalkForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputJump,
                }
            }
        );

        Transition(set.StandRunForward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputJump,
                }
            }
        );

        Transition(set.StandRunBackward, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputJump,
                }
            }
        );

        Transition(set.Sprint, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputJump,
                }
            }
        );

        Transition(set.Jump, set.Jump, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputMultiJump,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Trigger,
                    Input = InputJump,
                }
            }
        );

        Transition(set.Jump, set.Fall, 1.0f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
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
                    Input = InputGrounded,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputLandHardness,
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
                    Input = InputGrounded,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true }
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputLandHardness,
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

        WriteInt(set.CrouchIdle, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchIdleTakeknee, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchHitFront, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchHitLeft, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchHitRight, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchReload, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchShoot, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchTurnLeft, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchTurnRight, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchWalkForward, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchWalkBackward, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchAlertIdle, OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchAlertWalkForward , OutputPosture, (int)PhxAnimPosture.Crouch);
        WriteInt(set.CrouchAlertWalkBackward, OutputPosture, (int)PhxAnimPosture.Crouch);

        WriteInt(set.StandIdle, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandIdleCheckweapon, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandIdleLookaround, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandWalkForward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandRunForward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandRunBackward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandReload, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandShootPrimary, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandShootSecondary, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandAlertIdle, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandAlertWalkForward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandAlertRunForward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandAlertRunBackward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandTurnLeft, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandTurnRight, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandHitFront, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandHitBack, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandHitLeft, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandHitRight, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandGetupFront, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandGetupBack, OutputPosture, (int)PhxAnimPosture.Stand);        
        WriteInt(set.StandDeathForward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandDeathBackward, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandDeathLeft, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandDeathRight, OutputPosture, (int)PhxAnimPosture.Stand);
        WriteInt(set.StandDeadhero, OutputPosture, (int)PhxAnimPosture.Stand);

        WriteInt(set.ThrownBounceFrontSoft, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownBounceBackSoft, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownFlail, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownFlyingFront, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownFlyingBack, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownFlyingLeft, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownFlyingRight, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownLandFrontSoft, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownLandBackSoft, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownTumbleFront, OutputPosture, (int)PhxAnimPosture.Thrown);
        WriteInt(set.ThrownTumbleBack, OutputPosture, (int)PhxAnimPosture.Thrown);

        WriteInt(set.Sprint, OutputPosture, (int)PhxAnimPosture.Sprint);
        WriteInt(set.JetpackHover, OutputPosture, (int)PhxAnimPosture.Jet);
        WriteInt(set.Jump, OutputPosture, (int)PhxAnimPosture.Jump);
        WriteInt(set.Fall, OutputPosture, (int)PhxAnimPosture.Fall);
        WriteInt(set.LandSoft, OutputPosture, (int)PhxAnimPosture.Land);
        WriteInt(set.LandHard, OutputPosture, (int)PhxAnimPosture.Land);


        WriteBool(set.StandRunBackward, OutputStrafeBackwards, true);
        WriteBool(set.StandAlertRunBackward, OutputStrafeBackwards, true);
        WriteBool(set.CrouchAlertWalkBackward, OutputStrafeBackwards, true);

        WriteBool(set.StandWalkForward, OutputStrafeBackwards, false);
        WriteBool(set.StandRunForward, OutputStrafeBackwards, false);
        WriteBool(set.StandAlertWalkForward, OutputStrafeBackwards, false);
        WriteBool(set.StandAlertRunForward, OutputStrafeBackwards, false);
        WriteBool(set.CrouchAlertWalkForward, OutputStrafeBackwards, false);

        WriteBool(set.StandReload, OutputIsReloading, true);
        WriteBool(set.StandIdle.Upper, OutputIsReloading, false);
        WriteBool(set.StandWalkForward.Upper, OutputIsReloading, false);
        WriteBool(set.StandRunForward.Upper, OutputIsReloading, false);
        WriteBool(set.StandRunBackward.Upper, OutputIsReloading, false);
        WriteBool(set.CrouchReload, OutputIsReloading, true);
        WriteBool(set.CrouchIdle.Upper, OutputIsReloading, false);
        WriteBool(set.CrouchWalkForward.Upper, OutputIsReloading, false);
        WriteBool(set.CrouchWalkBackward.Upper, OutputIsReloading, false);

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

    void WriteInt(PhxScopedState state, CraOutput output, int value)
    {
        WriteInt(state.Lower, output, value);
        WriteInt(state.Upper, output, value);
    }

    void WriteInt(CraState state, CraOutput output, int value)
    {
        state.WriteOutput(new CraWriteOutput { Output = output, Value = new CraValueUnion { Type = CraValueType.Int, ValueInt = value } });
    }

    void WriteBool(PhxScopedState state, CraOutput output, bool value)
    {
        WriteBool(state.Lower, output, value);
        WriteBool(state.Upper, output, value);
    }

    void WriteBool(CraState state, CraOutput output, bool value)
    {
        state.WriteOutput(new CraWriteOutput { Output = output, Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = value } });
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
            uint Hash_State = HashUtils.GetFNV("State");
            uint Hash_Duration = HashUtils.GetFNV("Duration");
            uint Hash_Animation = HashUtils.GetFNV("Animation");
            uint Hash_EnergyRestoreRate = HashUtils.GetFNV("EnergyRestoreRate");
            uint Hash_InputLock = HashUtils.GetFNV("InputLock");
            uint Hash_Transition = HashUtils.GetFNV("Transition");
            uint Hash_If = HashUtils.GetFNV("If");
            uint Hash_Or = HashUtils.GetFNV("Or");
            uint Hash_Button = HashUtils.GetFNV("Button");

            // Since transitions may refer to yet undefined states (or in circles), we need
            // to store all the transitions and apply them AFTER the full combo has been parsed.
            List<PhxComboTransitionCache> Transitions = new List<PhxComboTransitionCache>();
            Dictionary<string, PhxScopedState> ComboStates = new Dictionary<string, PhxScopedState>();

            Field[] fields = combo.GetFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                Field field = fields[i];
                if (field.GetNameHash() == Hash_State)
                {
                    string stateName = field.GetString();
                    if (ComboStates.ContainsKey(stateName))
                    {
                        Debug.LogError($"State '{stateName}' is defined more than once in combo '{comboName}'!");
                    }
                    else if (field.Scope != null)
                    {
                        List<PhxComboTransitionCache> localTransitions = new List<PhxComboTransitionCache>();
                        PhxScopedState state = new PhxScopedState();
                        bool hasAnimation = false;

                        Field[] stateFields = field.Scope.GetFields();
                        for (int j = 0; j < stateFields.Length; ++j)
                        {
                            Field stateField = stateFields[j];
                            if (stateField.GetNameHash() == Hash_Animation)
                            {
                                if (hasAnimation)
                                {
                                    Debug.LogError($"Combo '{comboName}' tried to assign more than once Animation to state '{stateName}'!");
                                    continue;
                                }

                                string animname = stateField.GetString();
                                string[] split = animname.Split(new char[] { '_' }, 2);
                                state = CreateScopedState(root, character, weapon, split[0], split[1], true);
                                if (state.Lower.IsValid() && state.Upper.IsValid())
                                {
                                    state.Lower.GetPlayer().SetLooping(false);
                                    state.Upper.GetPlayer().SetLooping(false);
    #if UNITY_EDITOR
                                    if (state.Lower.IsValid()) state.Lower.SetName($"COMBO Lower {weapon} {stateName}");
                                    if (state.Upper.IsValid()) state.Upper.SetName($"COMBO Upper {weapon} {stateName}");
    #endif
                                    hasAnimation = true;
                                }
                                else
                                {
                                    Debug.LogError($"Failed to create state '{stateName}' in Combo '{comboName}'!");
                                }
                            }
                            else if (stateField.GetNameHash() == Hash_Duration)
                            {
                                float time = stateField.GetFloat(0);
                                if (time <= 0f)
                                {
                                    state.Lower.GetPlayer().SetLooping(true);
                                    state.Upper.GetPlayer().SetLooping(true);
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
                                    state.Lower.GetPlayer().SetPlayRange(range);
                                    state.Upper.GetPlayer().SetPlayRange(range);
                                }
                            }
                            else if (stateField.GetNameHash() == Hash_Transition)
                            {
                                localTransitions.Add(new PhxComboTransitionCache { Transition = stateField });
                            }
                        }

                        if (stateName.ToLower() == "idle")
                        {
                            hasAnimation = true;
                            state = set.StandIdle;
                        }

                        if (hasAnimation)
                        {
                            for (int j = 0; j < localTransitions.Count; ++j)
                            {
                                var lt = localTransitions[j];
                                lt.SourceState = state;
                                localTransitions[j] = lt;
                            }
                            ComboStates.Add(stateName, state);
                            Transitions.AddRange(localTransitions);
                        }
                    }
                }
            }

            for (int ti = 0; ti < Transitions.Count; ++ti)
            {
                if (!Transitions[ti].SourceState.Lower.IsValid() || !Transitions[ti].SourceState.Upper.IsValid())
                {
                    // TODO: This should never happen later on
                    continue;
                }

                PhxScopedState targetState = set.StandIdle;
                string targetStateName = Transitions[ti].Transition.GetString();
                if (targetStateName.ToLower() != "idle")
                {
                    if (!ComboStates.TryGetValue(targetStateName, out targetState))
                    {
                        Debug.LogError($"Tried to transition to unknown state '{targetStateName}' in combo '{comboName}'!");
                        continue;
                    }
                }

                if (!targetState.Lower.IsValid() || !targetState.Upper.IsValid())
                {
                    // TODO: This should never happen later on
                    continue;
                }

                List<CraConditionOr> Ors = new List<CraConditionOr>();

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
                CheckSameAnimation(Transitions[ti].SourceState.Lower, targetState.Lower);
                CheckSameAnimation(Transitions[ti].SourceState.Upper, targetState.Upper);


                Field[] orFields = Transitions[ti].Transition.Scope.GetFields();
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
                                    if (!ComboButtons.TryGetValue(comboButton, out CraInput input))
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

                //if (Ors.Count == 0)
                //{
                //    Ors.Add(new CraConditionOr
                //    {
                //        And0 = new CraCondition
                //        {
                //            Type = CraConditionType.IsFinished
                //        }
                //    });
                //}

                Transition(Transitions[ti].SourceState, targetState, 0.15f, Ors.ToArray());
            }
        }
    }

    struct PhxComboTransitionCache
    {
        public PhxScopedState SourceState;
        public Field Transition;
    }
}