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
    public CraState Full;
}

//public struct PhxAnimSetHumanLower
//{
//    public CraState StandIdle;
//    public CraState StandWalkForward;
//    public CraState StandRunForward;
//    public CraState StandRunBackward;
//    public CraState StandTurnLeft;
//    public CraState StandTurnRight;
//    public CraState StandAlertIdle;
//    public CraState StandAlertWalkForward;
//    public CraState StandAlertRunForward;
//    public CraState StandAlertRunBackward;

//    public CraState CrouchIdle;
//    public CraState CrouchIdleTakeknee;
//    public CraState CrouchTurnLeft;
//    public CraState CrouchTurnRight;
//    public CraState CrouchWalkForward;
//    public CraState CrouchWalkBackward;
//    public CraState CrouchAlertIdle;
//    public CraState CrouchAlertWalkForward;
//    public CraState CrouchAlertWalkBackward;

//    public CraState JetpackHover;
//    public CraState Sprint;
//}

//public struct PhxAnimSetHumanUpper
//{
//    // Upper
//    public CraState StandIdle;
//    public CraState StandWalkForward;
//    public CraState StandRunForward;
//    public CraState StandRunBackward;
//    public CraState StandHitFront;
//    public CraState StandHitBack;
//    public CraState StandHitLeft;
//    public CraState StandHitRight;
//    public CraState StandReload;
//    public CraState StandShootPrimary;
//    public CraState StandShootSecondary;
//    public CraState StandAlertIdle;
//    public CraState StandAlertWalkForward;
//    public CraState StandAlertRunForward;
//    public CraState StandAlertRunBackward;

//    public CraState CrouchIdle;
//    public CraState CrouchWalkForward;
//    public CraState CrouchWalkBackward;
//    public CraState CrouchHitFront;
//    public CraState CrouchHitLeft;
//    public CraState CrouchHitRight;
//    public CraState CrouchReload;
//    public CraState CrouchShoot;
//    public CraState CrouchAlertIdle;
//    public CraState CrouchAlertWalkForward;
//    public CraState CrouchAlertWalkBackward;

//    public CraState Sprint;
//}

//public struct PhxAnimSetHumanFull
//{
//    public CraState StandIdleCheckweapon;
//    public CraState StandIdleLookaround;
//    public CraState StandGetupFront;
//    public CraState StandGetupBack;
//    public CraState StandDeathForward;
//    public CraState StandDeathBackward;
//    public CraState StandDeathLeft;
//    public CraState StandDeathRight;
//    public CraState StandDeadhero;

//    public CraState ThrownBounceFrontSoft;
//    public CraState ThrownBounceBackSoft;
//    public CraState ThrownFlail;
//    public CraState ThrownFlyingFront;
//    public CraState ThrownFlyingBack;
//    public CraState ThrownFlyingLeft;
//    public CraState ThrownFlyingRight;
//    public CraState ThrownLandFrontSoft;
//    public CraState ThrownLandBackSoft;
//    public CraState ThrownTumbleFront;
//    public CraState ThrownTumbleBack;

//    public CraState Jump;
//    public CraState Fall;
//    public CraState LandSoft;
//    public CraState LandHard;
//    public CraState RollLeft;
//    public CraState RollRight;
//    public CraState Choking;
//}





// For each weapon, will shall generate one animation set
// Also, a Set should include a basepose (skeleton setup)
public struct PhxAnimHumanSet
{
    //public PhxAnimSetHumanLower Lower;
    //public PhxAnimSetHumanUpper Upper;
    //public PhxAnimSetHumanFull  Full;

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
    public CraLayer LayerFull { get; private set; }

    public CraInput InputMovementX { get; private set; }
    public CraInput InputMovementY { get; private set; }
    public CraInput InputCrouch { get; private set; }
    public CraInput InputSprint { get; private set; }
    public CraInput InputShootPrimary { get; private set; }
    public CraInput InputShootSecondary { get; private set; }
    public CraInput InputEnergy { get; private set; }


    CraState StateNoneUpper;
    CraState StateNoneFull;
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
        LayerFull  = Machine.NewLayer();

        InputMovementX = Machine.NewInput(CraValueType.Float, "Movement X");
        InputMovementY = Machine.NewInput(CraValueType.Float, "Movement Y");
        InputCrouch = Machine.NewInput(CraValueType.Bool, "Crouch");
        InputSprint = Machine.NewInput(CraValueType.Bool, "Sprint");
        InputEnergy = Machine.NewInput(CraValueType.Float, "Energy");
        InputShootPrimary = Machine.NewInput(CraValueType.Bool, "Shoot Primary");
        InputShootSecondary = Machine.NewInput(CraValueType.Bool, "Shoot Secondary");

        StateNoneUpper = LayerUpper.NewState(CraPlayer.None);
        StateNoneFull = LayerFull.NewState(CraPlayer.None);
#if UNITY_EDITOR
        StateNoneUpper.SetName("None");
        StateNoneFull.SetName("None");
#endif

        Sets = new PhxAnimHumanSet[weaponAnimBanks.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            bool weaponSupportsAlert = true;

            WeaponNameToSetIdx.Add(weaponAnimBanks[i], i);
            Sets[i] = GenerateSet(root, characterAnimBank, weaponAnimBanks[i]);

            Transitions_Stand(ref Sets[i]);
            //Transitions_Crouch(ref Sets[i]);
            //Transitions_StandToCrouch(ref Sets[i]);
            //Transitions_CrouchToStand(ref Sets[i]);
        }

        LayerLower.SetActiveState(Sets[ActiveSet].StandIdle.Lower);
        LayerUpper.SetActiveState(Sets[ActiveSet].StandIdle.Upper);
        LayerFull.SetActiveState(StateNoneFull);
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
        if (from.Full.IsValid() && to.Full.IsValid())
        {
            Transition(from.Full, to.Full, transitionTime, args);
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

    //void Transitions_Crouch(ref PhxAnimHumanSet set)
    //{
    //    // Crouch Idle --> Crouch Walk Forward
    //    set.CrouchIdle.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchWalkForward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Greater,
    //                Input = InputMovementX,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //                CompareToAbsolute = true
    //            }
    //        },
    //        Or1 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Greater,
    //                Input = InputMovementY,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //            }
    //        },
    //    });

    //    // Crouch Walk Forward --> Crouch Idle
    //    set.CrouchWalkForward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchIdle,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.LessOrEqual,
    //                Input = InputMovementX,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //                CompareToAbsolute = true
    //            },
    //            And1 = new CraCondition
    //            {
    //                Type = CraConditionType.LessOrEqual,
    //                Input = InputMovementY,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });

    //    // Crouch Idle --> Crouch Walk Backward
    //    set.CrouchIdle.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchWalkBackward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Less,
    //                Input = InputMovementY,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -0.2f },
    //            }
    //        },
    //    });

    //    // Crouch Walk Backward --> Crouch Idle
    //    set.CrouchWalkBackward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchIdle,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.LessOrEqual,
    //                Input = InputMovementX,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //                CompareToAbsolute = true
    //            },
    //            And1 = new CraCondition
    //            {
    //                Type = CraConditionType.LessOrEqual,
    //                Input = InputMovementY,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });
    //}

    //void Transitions_StandToCrouch(ref PhxAnimHumanSet set)
    //{
    //    // Stand Idle --> Crouch Idle
    //    set.StandIdle.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchIdle,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });

    //    // Stand Walk Forward --> Crouch Walk Forward
    //    set.StandWalkForward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchWalkForward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });

    //    // Stand Run Forward --> Crouch Walk Forward
    //    set.StandRunForward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.CrouchWalkForward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });
    //}

    //void Transitions_CrouchToStand(ref PhxAnimHumanSet set)
    //{
    //    // Crouch Idle --> Stand Idle
    //    set.CrouchIdle.NewTransition(new CraTransitionData
    //    {
    //        Target = set.StandIdle,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
    //                CompareToAbsolute = true
    //            }
    //        },
    //    });

    //    // Crouch Walk Forward --> Stand Walk Forward
    //    set.CrouchWalkForward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.StandWalkForward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
    //                CompareToAbsolute = true
    //            },
    //            And1 = new CraCondition
    //            {
    //                Type = CraConditionType.LessOrEqual,
    //                Input = InputMovementX,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
    //            }
    //        },
    //    });

    //    // Crouch Walk Forward --> Stand Run Forward
    //    set.CrouchWalkForward.NewTransition(new CraTransitionData
    //    {
    //        Target = set.StandRunForward,
    //        TransitionTime = 0.15f,
    //        Or0 = new CraConditionOr
    //        {
    //            And0 = new CraCondition
    //            {
    //                Type = CraConditionType.Equal,
    //                Input = InputCrouch,
    //                Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
    //                CompareToAbsolute = true
    //            },
    //            And1 = new CraCondition
    //            {
    //                Type = CraConditionType.Greater,
    //                Input = InputMovementY,
    //                Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
    //            }
    //        },
    //    });
    //}

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

    CraState CreateState(Transform root, in PhxAnimDesc animDesc, ref PhxAnimScope animScope)
    {
        if (!Resolver.ResolveAnim(animDesc, out CraClip clip, out PhxAnimScope scope, out bool loop))
        {
            return CraState.None;
        }
        Debug.Assert(clip.IsValid());
        CraPlayer player = CraPlayer.CreateNew();
        player.SetLooping(loop);
        player.SetClip(clip);
        CraState state = CraState.None;
        if (animScope == PhxAnimScope.None)
        {
            animScope = scope;
        }
        string splitBoneName = PhxUtils.FindTransformRecursive(root, "bone_a_spine") != null ? "bone_a_spine" : "bone_b_spine";
        switch (animScope)
        {
            case PhxAnimScope.Lower:
                player.Assign(root, new CraMask(CraMaskOperation.Difference, true, splitBoneName));
                state = LayerLower.NewState(player);
                break;
            case PhxAnimScope.Upper:
                player.Assign(root, new CraMask(CraMaskOperation.Intersection, true, splitBoneName));
                state = LayerUpper.NewState(player);
                break;
            case PhxAnimScope.Full:
                player.Assign(root);
                state = LayerFull.NewState(player);
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
        res.Full  = CraState.None;

        // lower override only for non-rifle weapons
        overrideLowerRifle = overrideLowerRifle && weapon != "rifle";

        PhxAnimScope animScope = PhxAnimScope.None;
        if (overrideLowerRifle)
        {
            animScope = PhxAnimScope.Upper;
        }
        CraState state = CreateState(root, in animDesc, ref animScope);
        if (!state.IsValid())
        {
            Debug.LogError($"Couldnt resolve {animDesc}!");
            return res;
        }

        Debug.Assert(animScope != PhxAnimScope.None);
        //Debug.Assert((animScope == PhxAnimScope.Full) != useLowerRifle);

        switch (animScope)
        {
            case PhxAnimScope.Lower:
                res.Lower = state;
                break;
            case PhxAnimScope.Upper:
                res.Upper = state;
                break;
            case PhxAnimScope.Full:
                res.Full = state;
                break;
        }

        if (overrideLowerRifle)
        {
            Debug.Assert(res.Upper.IsValid());
            Debug.Assert(!res.Full.IsValid());

            animDesc.Weapon = "rifle";
            animScope = PhxAnimScope.Lower;
            state = CreateState(root, in animDesc, ref animScope);
            Debug.Assert(state.IsValid());
            Debug.Assert(animScope == PhxAnimScope.Lower);
            res.Lower = state;
        }

        Debug.Assert(res.Full.IsValid() || res.Lower.IsValid());
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

        set.Sprint = CreateScopedState(root, character, weapon, "sprint", null, true);
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
                if (state.Full.IsValid()) state.Full.SetName($"Full {weapon} {field.Name}");
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