using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;



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
    public CraInput InputShootPrimary { get; private set; }
    public CraInput InputShootSecondary { get; private set; }
    public CraInput InputEnergy { get; private set; }

    PhxAnimHumanSet[] Sets;
    byte ActiveSet;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponNameToSetIdx;

    const float Deadzone = 0.05f;


    public PhxAnimHuman(PhxAnimationResolver resolver, Transform root, string characterAnimBank, string[] weaponAnimBanks)
    {
        Debug.Assert(weaponAnimBanks != null);
        Debug.Assert(weaponAnimBanks.Length > 0);

        Machine = CraStateMachine.CreateNew();
        WeaponNameToSetIdx = new Dictionary<string, int>();

        Resolver = resolver;

        LayerLower = Machine.NewLayer();
        LayerUpper = Machine.NewLayer();

        InputMovementX = Machine.NewInput(CraValueType.Float, "Movement X");
        InputMovementY = Machine.NewInput(CraValueType.Float, "Movement Y");
        InputMagnitude = Machine.NewInput(CraValueType.Float, "Magnitude");
        InputTurnLeft = Machine.NewInput(CraValueType.Bool, "Turn Left");
        InputTurnRight = Machine.NewInput(CraValueType.Bool, "Turn Right");
        InputCrouch = Machine.NewInput(CraValueType.Bool, "Crouch");
        InputSprint = Machine.NewInput(CraValueType.Bool, "Sprint");
        InputEnergy = Machine.NewInput(CraValueType.Float, "Energy");
        InputShootPrimary = Machine.NewInput(CraValueType.Bool, "Shoot Primary");
        InputShootSecondary = Machine.NewInput(CraValueType.Bool, "Shoot Secondary");

        Sets = new PhxAnimHumanSet[weaponAnimBanks.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            bool weaponSupportsAlert = true;

            WeaponNameToSetIdx.Add(weaponAnimBanks[i], i);
            Sets[i] = GenerateSet(root, characterAnimBank, weaponAnimBanks[i]);

            Transitions_Stand(ref Sets[i]);
            Transitions_StandTurn(ref Sets[i]);
            Transitions_Crouch(ref Sets[i]);
            Transitions_CrouchTurn(ref Sets[i]);
            Transitions_StandToCrouch(ref Sets[i]);
            Transitions_CrouchToStand(ref Sets[i]);
            Transitions_Sprint(ref Sets[i]);
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
        Debug.Assert(args.Length <= 4);

        CraTransitionData transition = new CraTransitionData
        {
            Target = to,
            TransitionTime = transitionTime
        };

        // No fixed() needed, since 'transition' is allocated on the stack
        CraConditionOr* or = &transition.Or0;
        for (int i = 0; i < Mathf.Min(args.Length, 4); ++i)
        {
            or[i] = args[i];
        }

        from.NewTransition(transition);
    }

    void Transitions_Stand(ref PhxAnimHumanSet set)
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

    void Transitions_StandTurn(ref PhxAnimHumanSet set)
    {
        Transition(set.StandIdle, set.StandTurnLeft, 0.15f,
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

        Transition(set.StandIdle, set.StandTurnRight, 0.15f,
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

        Transition(set.StandTurnLeft, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.StandTurnRight, set.StandIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
            }
        );
    }

    void Transitions_Crouch(ref PhxAnimHumanSet set)
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

    void Transitions_CrouchTurn(ref PhxAnimHumanSet set)
    {
        Transition(set.CrouchIdle, set.CrouchTurnLeft, 0.15f,
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

        Transition(set.CrouchIdle, set.CrouchTurnRight, 0.15f,
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

        Transition(set.CrouchTurnLeft, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
            }
        );

        Transition(set.CrouchTurnRight, set.CrouchIdle, 0.15f,
            new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.IsFinished
                },
            }
        );
    }

    void Transitions_StandToCrouch(ref PhxAnimHumanSet set)
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

    void Transitions_CrouchToStand(ref PhxAnimHumanSet set)
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

    void Transitions_Sprint(ref PhxAnimHumanSet set)
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

    //public PhxAnimPosture GetCurrentPosture()
    //{
    //    // Maybe introduce CraOutput's such that a state can
    //    // write to an output when it becomes active and we
    //    // can simply read that output here instead of doing
    //    // a whole bunch of compares.

    //    CraState state = LayerLower.GetActiveState();
    //    PhxAnimHumanSet set = Sets[ActiveSet];

    //    if (state == set.Sprint)
    //    {
    //        return PhxAnimPosture.Sprint;
    //    }
    //    if (state == set.Jump)
    //    {
    //        return PhxAnimPosture.Jump;
    //    }
    //    if (state == set.RollLeft)
    //    {
    //        return PhxAnimPosture.RollLeft;
    //    }
    //    if (state == set.RollRight)
    //    {
    //        return PhxAnimPosture.RollRight;
    //    }
    //    if (state == set.Jet)
    //    {
    //        return PhxAnimPosture.Jet;
    //    }

    //    if (state == set.StandIdle ||
    //        state == set.StandIdleCheckweapon ||
    //        state == set.StandIdleLookaround ||
    //        state == set.StandTurnLeft ||
    //        state == set.StandTurnRight ||
    //        state == set.StandShootPrimary ||
    //        state == set.StandShootSecondary ||
    //        state == set.StandReload ||
    //        state == set.StandRunBackward ||
    //        state == set.StandRunForward ||
    //        state == set.StandWalkForward ||
    //        //state == set.StandWalkBackward ||
    //        state == set.StandHitFront ||
    //        state == set.StandHitBack ||
    //        state == set.StandHitLeft ||
    //        state == set.StandHitRight ||
    //        state == set.StandAlertIdle ||
    //        state == set.StandAlertRunBackward ||
    //        state == set.StandAlertRunForward ||
    //        //state == set.StandAlertWalkBackward ||
    //        state == set.StandAlertWalkForward)
    //    {
    //        return PhxAnimPosture.Stand;
    //    }

    //    if (state == set.CrouchIdle ||
    //        state == set.CrouchIdleTakeknee ||
    //        state == set.CrouchTurnLeft ||
    //        state == set.CrouchTurnRight ||
    //        state == set.CrouchHitFront ||
    //        state == set.CrouchHitLeft ||
    //        state == set.CrouchHitRight ||
    //        state == set.CrouchShoot ||
    //        state == set.CrouchReload ||
    //        state == set.CrouchWalkForward ||
    //        state == set.CrouchWalkBackward ||
    //        state == set.CrouchAlertIdle ||
    //        state == set.CrouchAlertWalkBackward ||
    //        state == set.CrouchAlertWalkForward)
    //    {
    //        return PhxAnimPosture.Crouch;
    //    }

    //    if (state == set.ThrownBounceFrontSoft ||
    //        state == set.ThrownBounceBackSoft ||
    //        state == set.ThrownFlail ||
    //        state == set.ThrownFlyingFront ||
    //        state == set.ThrownFlyingBack ||
    //        state == set.ThrownFlyingLeft ||
    //        state == set.ThrownFlyingRight ||
    //        state == set.ThrownLandFrontSoft ||
    //        state == set.ThrownLandBackSoft ||
    //        state == set.ThrownTumbleFront ||
    //        state == set.ThrownTumbleBack)
    //    {
    //        return PhxAnimPosture.Thrown;
    //    }

    //    return PhxAnimPosture.None;
    //}

    public void SetActiveWeaponBank(string weaponAnimBank)
    {
        if (!WeaponNameToSetIdx.TryGetValue(weaponAnimBank, out int idx))
        {
            Debug.LogError($"Unknown weapon animation bank '{weaponAnimBank}'!");
            return;
        }

        // TODO: How to keep current states?
        LayerLower.SetActiveState(Sets[idx].StandIdle.Lower, 0.15f);
        LayerUpper.SetActiveState(Sets[idx].StandIdle.Upper, 0.15f);
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

    PhxScopedState CreateScopedState(Transform root, string character, string weapon, string posture, string anim, bool overrideLowerRifle)
    {
        PhxAnimDesc animDesc = new PhxAnimDesc { Character = character, Weapon = weapon, Posture = posture, Animation = anim };

        PhxScopedState res;
        res.Lower = CraState.None;
        res.Upper = CraState.None;

        // lower override only for non-rifle weapons
        overrideLowerRifle = overrideLowerRifle && weapon != "rifle";

        if (!Resolver.ResolveAnim(animDesc, out CraClip clip, out PhxAnimScope animScope))
        {
            Debug.LogError($"Couldn't resolve {animDesc}!");
            return res;
        }
        Debug.Assert(clip.IsValid());
        Debug.Assert(animScope != PhxAnimScope.None);

        bool loop = !animDesc.IsWeaponAnimation() && (string.IsNullOrEmpty(animDesc.Animation) || !animDesc.Animation.ToLower().StartsWith("turn"));
        if (overrideLowerRifle || animScope == PhxAnimScope.Upper)
        {
            animDesc.Weapon = "rifle";
            if (!Resolver.ResolveAnim(animDesc, out CraClip clipRifle, out PhxAnimScope animScopeRifle))
            {
                Debug.LogError($"Couldn't resolve {animDesc}!");
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

    PhxAnimHumanSet GenerateSet(Transform root, string character, string weapon)
    {
        PhxAnimHumanSet set = new PhxAnimHumanSet();

        set.CrouchIdle = CreateScopedState(root, character, weapon, "crouch", "idle_emote", true);
        set.CrouchIdleTakeknee = CreateScopedState(root, character, weapon, "crouch", "idle_takeknee", false);
        set.CrouchHitFront = CreateScopedState(root, character, weapon, "crouch", "hitfront", true);
        set.CrouchHitLeft = CreateScopedState(root, character, weapon, "crouch", "hitleft", true);
        set.CrouchHitRight = CreateScopedState(root, character, weapon, "crouch", "hitright", true);
        set.CrouchReload = CreateScopedState(root, character, weapon, "crouch", "reload", true);
        set.CrouchShoot = CreateScopedState(root, character, weapon, "crouch", "shoot", true);
        set.CrouchTurnLeft = CreateScopedState(root, character, weapon, "crouch", "turnleft", false);
        set.CrouchTurnRight = CreateScopedState(root, character, weapon, "crouch", "turnright", false);
        set.CrouchWalkForward = CreateScopedState(root, character, weapon, "crouch", "walkforward", true);
        set.CrouchWalkBackward = CreateScopedState(root, character, weapon, "crouch", "walkbackward", true);
        set.CrouchAlertIdle = CreateScopedState(root, character, weapon, "crouchalert", "idle_emote", true);
        set.CrouchAlertWalkForward = CreateScopedState(root, character, weapon, "crouchalert", "walkforward", true);
        set.CrouchAlertWalkBackward = CreateScopedState(root, character, weapon, "crouchalert", "walkbackward", true);

        // human_sabre_stand_idle_emote_full in rep.lvl doesn't contain the lower body animation, although the .msh does...
        // Idk why, maybe the lower body got compiled out at munge?
        set.StandIdle = CreateScopedState(root, character, weapon, "stand", "idle_emote", true);
        //set.StandIdle = CreateScopedState(root, character, weapon, "sprint", null, true);
        set.StandIdleCheckweapon = CreateScopedState(root, character, weapon, "stand", "idle_checkweapon", false);
        set.StandIdleLookaround = CreateScopedState(root, character, weapon, "stand", "idle_lookaround", false);
        set.StandWalkForward = CreateScopedState(root, character, weapon, "stand", "walkforward", true);
        set.StandRunForward = CreateScopedState(root, character, weapon, "stand", "runforward", true);
        set.StandRunBackward = CreateScopedState(root, character, weapon, "stand", "runbackward", true);
        set.StandReload = CreateScopedState(root, character, weapon, "stand", "reload", true);
        set.StandShootPrimary = CreateScopedState(root, character, weapon, "stand", "shoot", true);
        set.StandShootSecondary = CreateScopedState(root, character, weapon, "stand", "shoot_secondary", true);
        set.StandAlertIdle = CreateScopedState(root, character, weapon, "standalert", "idle_emote", true);
        set.StandAlertWalkForward = CreateScopedState(root, character, weapon, "standalert", "walkforward", true);
        set.StandAlertRunForward = CreateScopedState(root, character, weapon, "standalert", "runforward", true);
        set.StandAlertRunBackward = CreateScopedState(root, character, weapon, "standalert", "runbackward", true);
        set.StandTurnLeft = CreateScopedState(root, character, weapon, "stand", "turnleft", false);
        set.StandTurnRight = CreateScopedState(root, character, weapon, "stand", "turnright", false);
        set.StandHitFront = CreateScopedState(root, character, weapon, "stand", "hitfront", true);
        set.StandHitBack = CreateScopedState(root, character, weapon, "stand", "hitback", true);
        set.StandHitLeft = CreateScopedState(root, character, weapon, "stand", "hitleft", true);
        set.StandHitRight = CreateScopedState(root, character, weapon, "stand", "hitright", true);
        set.StandGetupFront = CreateScopedState(root, character, weapon, "stand", "getupfront", false);
        set.StandGetupBack = CreateScopedState(root, character, weapon, "stand", "getupback", false);
        set.StandDeathForward = CreateScopedState(root, character, weapon, "stand", "death_forward", false);
        set.StandDeathBackward = CreateScopedState(root, character, weapon, "stand", "death_backward", false);
        set.StandDeathLeft = CreateScopedState(root, character, weapon, "stand", "death_left", false);
        set.StandDeathRight = CreateScopedState(root, character, weapon, "stand", "death_right", false);
        set.StandDeadhero = CreateScopedState(root, character, weapon, "stand", "idle_emote", false);

        set.ThrownBounceFrontSoft = CreateScopedState(root, character, weapon, "thrown", "bouncefrontsoft", false);
        set.ThrownBounceBackSoft = CreateScopedState(root, character, weapon, "thrown", "bouncebacksoft", false);
        set.ThrownFlail = CreateScopedState(root, character, weapon, "thrown", "flail", false);
        set.ThrownFlyingFront = CreateScopedState(root, character, weapon, "thrown", "flyingfront", false);
        set.ThrownFlyingBack = CreateScopedState(root, character, weapon, "thrown", "flyingback", false);
        set.ThrownFlyingLeft = CreateScopedState(root, character, weapon, "thrown", "flyingleft", false);
        set.ThrownFlyingRight = CreateScopedState(root, character, weapon, "thrown", "flyingright", false);
        set.ThrownLandFrontSoft = CreateScopedState(root, character, weapon, "thrown", "landfrontsoft", false);
        set.ThrownLandBackSoft = CreateScopedState(root, character, weapon, "thrown", "landbacksoft", false);
        set.ThrownTumbleFront = CreateScopedState(root, character, weapon, "thrown", "tumblefront", false);
        set.ThrownTumbleBack = CreateScopedState(root, character, weapon, "thrown", "tumbleback", false);

        set.Sprint = CreateScopedState(root, character, weapon, "sprint", null, false);
        set.JetpackHover = CreateScopedState(root, character, weapon, "jetpack_hover", null, false);
        set.Jump = CreateScopedState(root, character, weapon, "jump", null, false);
        set.Fall = CreateScopedState(root, character, weapon, "fall", null, false);
        set.LandSoft = CreateScopedState(root, character, weapon, "landsoft", null, false);
        set.LandHard = CreateScopedState(root, character, weapon, "landhard", null, false);

#if UNITY_EDITOR
        foreach (var field in set.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (field.FieldType == typeof(PhxScopedState))
            {
                PhxScopedState state = (PhxScopedState)field.GetValue(set);
                if (state.Lower.IsValid()) state.Lower.SetName($"Lower {weapon} {field.Name}");
                if (state.Upper.IsValid()) state.Upper.SetName($"Upper {weapon} {field.Name}");
            }
        }
#endif

        return set;
    }

    public void PlayIntroAnim()
    {
        //LayerLower.SetActiveState(Sets[ActiveSet].StandReload);
        LayerUpper.SetActiveState(Sets[ActiveSet].StandReload.Upper);
    }

    public void SetActive(bool status = true)
    {
        Machine.SetActive(status);
    }
}