using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;



// For each weapon, will shall generate one animation set
// Also, a Set should include a basepose (skeleton setup)
public struct PhxAnimHumanSet
{
    public CraState StandIdle;
    public CraState StandIdleCheckweapon;
    public CraState StandIdleLookaround;
    public CraState StandWalkForward;
    //public CraState StandWalkBackward; // Seems like this doesn't exist?
    public CraState StandRunForward;
    public CraState StandRunBackward;
    public CraState StandReload;
    public CraState StandShootPrimary;
    public CraState StandShootSecondary;
    public CraState StandAlertIdle;
    public CraState StandAlertWalkForward;
    //public CraState StandAlertWalkBackward;
    public CraState StandAlertRunForward;
    public CraState StandAlertRunBackward;
    public CraState StandTurnLeft;
    public CraState StandTurnRight;
    public CraState StandHitFront;
    public CraState StandHitBack;
    public CraState StandHitLeft;
    public CraState StandHitRight;
    public CraState StandGetupFront;
    public CraState StandGetupBack;
    public CraState StandDeathForward;
    public CraState StandDeathBackward;
    public CraState StandDeathLeft;
    public CraState StandDeathRight;
    public CraState StandDeadhero;

    public CraState CrouchIdle;
    public CraState CrouchIdleTakeknee;
    public CraState CrouchHitFront;
    public CraState CrouchHitLeft;
    public CraState CrouchHitRight;
    public CraState CrouchReload;
    public CraState CrouchShoot;
    public CraState CrouchTurnLeft;
    public CraState CrouchTurnRight;
    public CraState CrouchWalkForward;
    public CraState CrouchWalkBackward;
    public CraState CrouchAlertIdle;
    public CraState CrouchAlertWalkForward;
    public CraState CrouchAlertWalkBackward;

    public CraState ThrownBounceFrontSoft;
    public CraState ThrownBounceBackSoft;
    public CraState ThrownFlail;
    public CraState ThrownFlyingFront;
    public CraState ThrownFlyingBack;
    public CraState ThrownFlyingLeft;
    public CraState ThrownFlyingRight;
    public CraState ThrownLandFrontSoft;
    public CraState ThrownLandBackSoft;
    public CraState ThrownTumbleFront;
    public CraState ThrownTumbleBack;

    public CraState Sprint;
    public CraState JetpackHover;
    public CraState Jump;
    public CraState Fall;
    public CraState LandSoft;
    public CraState LandHard;
    public CraState RollLeft;
    public CraState RollRight;
    public CraState Jet;
    public CraState Choking;
}

public class PhxAnimHuman
{
    public CraStateMachine Machine { get; private set; }
    public CraLayer LayerLower { get; private set; }
    public CraLayer LayerUpper { get; private set; }

    public CraInput InputMovementX { get; private set; }
    public CraInput InputMovementY { get; private set; }
    public CraInput InputCrouch { get; private set; }
    public CraInput InputSprint { get; private set; }
    public CraInput InputShootPrimary { get; private set; }
    public CraInput InputShootSecondary { get; private set; }
    public CraInput InputEnergy { get; private set; }


    CraState StateNone;
    PhxAnimHumanSet[] Sets;
    byte ActiveSet;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponNameToSetIdx;

    const float Deadzone = 0.05f;


    public PhxAnimHuman(PhxAnimationResolver resolver, Transform root, string characterAnimBank, string[] weaponAnimBanks)
    {
        Machine = CraStateMachine.CreateNew();
        WeaponNameToSetIdx = new Dictionary<string, int>();

        Resolver = resolver;

        LayerLower = Machine.NewLayer();
        LayerUpper = Machine.NewLayer();

        InputMovementX = Machine.NewInput(CraValueType.Float, "Movement X");
        InputMovementY = Machine.NewInput(CraValueType.Float, "Movement Y");
        InputCrouch = Machine.NewInput(CraValueType.Bool, "Crouch");
        InputSprint = Machine.NewInput(CraValueType.Bool, "Sprint");
        InputEnergy = Machine.NewInput(CraValueType.Float, "Energy");
        InputShootPrimary = Machine.NewInput(CraValueType.Bool, "Shoot Primary");
        InputShootSecondary = Machine.NewInput(CraValueType.Bool, "Shoot Secondary");

        StateNone = LayerUpper.NewState(CraPlayer.None, "None");

        Sets = new PhxAnimHumanSet[weaponAnimBanks.Length];
        ActiveSet = 0;

        for (int i = 0; i < Sets.Length; ++i)
        {
            bool weaponSupportsAlert = true;

            WeaponNameToSetIdx.Add(weaponAnimBanks[i], i);
            Sets[i] = GenerateSet(root, characterAnimBank, weaponAnimBanks[i]);

            Transitions_Stand(ref Sets[i]);
            Transitions_Crouch(ref Sets[i]);
            Transitions_StandToCrouch(ref Sets[i]);
            Transitions_CrouchToStand(ref Sets[i]);
        }

        LayerLower.SetActiveState(Sets[ActiveSet].StandIdle);
        LayerUpper.SetActiveState(StateNone);
    }

    void Transitions_Stand(ref PhxAnimHumanSet set)
    {
        // Stand Idle --> Stand Walk Forward
        set.StandIdle.NewTransition(new CraTransitionData
        {
            Target = set.StandWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                    CompareToAbsolute = true
                }
            },
            Or1 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = Deadzone },
                }
            },
        });

        // Stand Walk Forward --> Stand Idle
        set.StandWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.StandIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
        });

        // Stand Walk Forward --> Stand Run Forward
        set.StandWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.StandRunForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
            },
        });

        // Stand Run Forward --> Stand Walk Forward
        set.StandRunForward.NewTransition(new CraTransitionData
        {
            Target = set.StandWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                }
            },
        });

        // Stand Idle --> Stand Run Backward
        set.StandIdle.NewTransition(new CraTransitionData
        {
            Target = set.StandRunBackward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -Deadzone },
                }
            },
        });

        // Stand Walk Forward --> Stand Run Backward
        set.StandWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.StandRunBackward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
        });

        // Stand Run Backward --> Stand Idle
        set.StandRunBackward.NewTransition(new CraTransitionData
        {
            Target = set.StandIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
        });

        // Stand Run Backward --> Stand Walk Forward
        set.StandRunBackward.NewTransition(new CraTransitionData
        {
            Target = set.StandWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
        });
    }

    void Transitions_Crouch(ref PhxAnimHumanSet set)
    {
        // Crouch Idle --> Crouch Walk Forward
        set.CrouchIdle.NewTransition(new CraTransitionData
        {
            Target = set.CrouchWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    CompareToAbsolute = true
                }
            },
            Or1 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                }
            },
        });

        // Crouch Walk Forward --> Crouch Idle
        set.CrouchWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.CrouchIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    CompareToAbsolute = true
                }
            },
        });

        // Crouch Idle --> Crouch Walk Backward
        set.CrouchIdle.NewTransition(new CraTransitionData
        {
            Target = set.CrouchWalkBackward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Less,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = -0.2f },
                }
            },
        });

        // Crouch Walk Backward --> Crouch Idle
        set.CrouchWalkBackward.NewTransition(new CraTransitionData
        {
            Target = set.CrouchIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    CompareToAbsolute = true
                },
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    CompareToAbsolute = true
                }
            },
        });
    }

    void Transitions_StandToCrouch(ref PhxAnimHumanSet set)
    {
        // Stand Idle --> Crouch Idle
        set.StandIdle.NewTransition(new CraTransitionData
        {
            Target = set.CrouchIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
                }
            },
        });

        // Stand Walk Forward --> Crouch Walk Forward
        set.StandWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.CrouchWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
                }
            },
        });

        // Stand Run Forward --> Crouch Walk Forward
        set.StandRunForward.NewTransition(new CraTransitionData
        {
            Target = set.CrouchWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = true },
                    CompareToAbsolute = true
                }
            },
        });
    }

    void Transitions_CrouchToStand(ref PhxAnimHumanSet set)
    {
        // Crouch Idle --> Stand Idle
        set.CrouchIdle.NewTransition(new CraTransitionData
        {
            Target = set.StandIdle,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
            {
                And0 = new CraCondition
                {
                    Type = CraConditionType.Equal,
                    Input = InputCrouch,
                    Value = new CraValueUnion { Type = CraValueType.Bool, ValueBool = false },
                    CompareToAbsolute = true
                }
            },
        });

        // Crouch Walk Forward --> Stand Walk Forward
        set.CrouchWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.StandWalkForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
            },
        });

        // Crouch Walk Forward --> Stand Run Forward
        set.CrouchWalkForward.NewTransition(new CraTransitionData
        {
            Target = set.StandRunForward,
            TransitionTime = 0.15f,
            Or0 = new CraConditionOr
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
            },
        });
    }

    public PhxAnimPosture GetCurrentPosture()
    {
        // Maybe introduce CraOutput's such that a state can
        // write to an output when it becomes active and we
        // can simply read that output here instead of doing
        // a whole bunch of compares.

        CraState state = LayerLower.GetActiveState();
        PhxAnimHumanSet set = Sets[ActiveSet];

        if (state == set.Sprint)
        {
            return PhxAnimPosture.Sprint;
        }
        if (state == set.Jump)
        {
            return PhxAnimPosture.Jump;
        }
        if (state == set.RollLeft)
        {
            return PhxAnimPosture.RollLeft;
        }
        if (state == set.RollRight)
        {
            return PhxAnimPosture.RollRight;
        }
        if (state == set.Jet)
        {
            return PhxAnimPosture.Jet;
        }

        if (state == set.StandIdle ||
            state == set.StandIdleCheckweapon ||
            state == set.StandIdleLookaround ||
            state == set.StandTurnLeft ||
            state == set.StandTurnRight ||
            state == set.StandShootPrimary ||
            state == set.StandShootSecondary ||
            state == set.StandReload ||
            state == set.StandRunBackward ||
            state == set.StandRunForward ||
            state == set.StandWalkForward ||
            //state == set.StandWalkBackward ||
            state == set.StandHitFront ||
            state == set.StandHitBack ||
            state == set.StandHitLeft ||
            state == set.StandHitRight ||
            state == set.StandAlertIdle ||
            state == set.StandAlertRunBackward ||
            state == set.StandAlertRunForward ||
            //state == set.StandAlertWalkBackward ||
            state == set.StandAlertWalkForward)
        {
            return PhxAnimPosture.Stand;
        }

        if (state == set.CrouchIdle ||
            state == set.CrouchIdleTakeknee ||
            state == set.CrouchTurnLeft ||
            state == set.CrouchTurnRight ||
            state == set.CrouchHitFront ||
            state == set.CrouchHitLeft ||
            state == set.CrouchHitRight ||
            state == set.CrouchShoot ||
            state == set.CrouchReload ||
            state == set.CrouchWalkForward ||
            state == set.CrouchWalkBackward ||
            state == set.CrouchAlertIdle ||
            state == set.CrouchAlertWalkBackward ||
            state == set.CrouchAlertWalkForward)
        {
            return PhxAnimPosture.Crouch;
        }

        if (state == set.ThrownBounceFrontSoft ||
            state == set.ThrownBounceBackSoft ||
            state == set.ThrownFlail ||
            state == set.ThrownFlyingFront ||
            state == set.ThrownFlyingBack ||
            state == set.ThrownFlyingLeft ||
            state == set.ThrownFlyingRight ||
            state == set.ThrownLandFrontSoft ||
            state == set.ThrownLandBackSoft ||
            state == set.ThrownTumbleFront ||
            state == set.ThrownTumbleBack)
        {
            return PhxAnimPosture.Thrown;
        }

        return PhxAnimPosture.None;
    }

    public void SetActiveWeaponBank(string weaponAnimBank)
    {
        if (!WeaponNameToSetIdx.TryGetValue(weaponAnimBank, out int idx))
        {
            Debug.LogError($"Unknown weapon animation bank '{weaponAnimBank}'!");
            return;
        }

        // TODO: How to keep current states?
        LayerLower.SetActiveState(Sets[idx].StandIdle);
    }

    CraState CreateState(Transform root, string character, string weapon, string posture, string anim, string stateName = null)
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
        CraState state = CraState.None;
        switch (scope)
        {
            case PhxAnimScope.Lower:
                player.Assign(root, new CraMask(true, "bone_pelvis"));
                state = LayerLower.NewState(player, stateName);
                break;
            case PhxAnimScope.Upper:
                player.Assign(root, new CraMask(true, "root_a_spine"));
                state = LayerUpper.NewState(player, stateName);
                break;
            case PhxAnimScope.Full:
                player.Assign(root);
                state = LayerLower.NewState(player, stateName);
                break;
        }
        Debug.Assert(player.IsValid());
        return state;
    }

    PhxAnimHumanSet GenerateSet(Transform root, string character, string weapon)
    {
        PhxAnimHumanSet set = new PhxAnimHumanSet();

        set.CrouchIdle = CreateState(root, character, weapon, "crouch", "idle_emote", "CrouchIdle");
        set.CrouchIdleTakeknee = CreateState(root, character, weapon, "crouch", "idle_takeknee", "CrouchIdle");
        set.CrouchHitFront = CreateState(root, character, weapon, "crouch", "hitfront", "CrouchHitFront");
        set.CrouchHitLeft = CreateState(root, character, weapon, "crouch", "hitleft", "CrouchHitLeft");
        set.CrouchHitRight = CreateState(root, character, weapon, "crouch", "hitright", "CrouchHitRight");
        set.CrouchReload = CreateState(root, character, weapon, "crouch", "reload", "CrouchReload");
        set.CrouchShoot = CreateState(root, character, weapon, "crouch", "shoot", "CrouchShoot");
        set.CrouchTurnLeft = CreateState(root, character, weapon, "crouch", "turnleft", "CrouchTurnLeft");
        set.CrouchTurnRight = CreateState(root, character, weapon, "crouch", "turnright", "CrouchTurnRight");
        set.CrouchWalkForward = CreateState(root, character, weapon, "crouch", "walkforward", "CrouchWalkForward");
        set.CrouchWalkBackward = CreateState(root, character, weapon, "crouch", "walkbackward", "CrouchWalkBackward");
        set.CrouchAlertIdle = CreateState(root, character, weapon, "crouchalert", "idle_emote", "CrouchAlertIdle");
        set.CrouchAlertWalkForward = CreateState(root, character, weapon, "crouchalert", "walkforward", "CrouchAlertWalkForward");
        set.CrouchAlertWalkBackward = CreateState(root, character, weapon, "crouchalert", "walkbackward", "CrouchAlertWalkBackward");

        set.StandIdle = CreateState(root, character, weapon, "stand", "idle_emote", "StandIdle");
        set.StandIdleCheckweapon = CreateState(root, character, weapon, "stand", "idle_checkweapon", "StandIdleCheckweapon");
        set.StandIdleLookaround = CreateState(root, character, weapon, "stand", "idle_lookaround", "StandIdleLookaround");
        set.StandWalkForward = CreateState(root, character, weapon, "stand", "walkforward", "StandWalkForward");
        set.StandRunForward = CreateState(root, character, weapon, "stand", "runforward", "StandRunForward");
        set.StandRunBackward = CreateState(root, character, weapon, "stand", "runbackward", "StandRunBackward");
        set.StandReload = CreateState(root, character, weapon, "stand", "reload", "StandReload");
        set.StandShootPrimary = CreateState(root, character, weapon, "stand", "shoot", "StandShootPrimary");
        set.StandShootSecondary = CreateState(root, character, weapon, "stand", "shoot_secondary", "StandShootSecondary");
        set.StandAlertIdle = CreateState(root, character, weapon, "standalert", "idle_emote", "StandAlertIdle");
        set.StandAlertWalkForward = CreateState(root, character, weapon, "standalert", "walkforward", "StandAlertWalkForward");
        set.StandAlertRunForward = CreateState(root, character, weapon, "standalert", "runforward", "StandAlertRunForward");
        set.StandAlertRunBackward = CreateState(root, character, weapon, "standalert", "runbackward", "StandAlertRunBackward");
        set.StandTurnLeft = CreateState(root, character, weapon, "stand", "turnleft", "StandTurnLeft");
        set.StandTurnRight = CreateState(root, character, weapon, "stand", "turnright", "StandTurnRight");
        set.StandHitFront = CreateState(root, character, weapon, "stand", "gitfront", "StandHitFront");
        set.StandHitBack = CreateState(root, character, weapon, "stand", "hitback", "StandHitBack");
        set.StandHitLeft = CreateState(root, character, weapon, "stand", "hitleft", "StandHitLeft");
        set.StandHitRight = CreateState(root, character, weapon, "stand", "hitright", "StandHitRight");
        set.StandGetupFront = CreateState(root, character, weapon, "stand", "getupfront", "StandGetupFront");
        set.StandGetupBack = CreateState(root, character, weapon, "stand", "getupback", "StandGetupBack");
        set.StandDeathForward = CreateState(root, character, weapon, "stand", "death_forward", "StandDeathForward");
        set.StandDeathBackward = CreateState(root, character, weapon, "stand", "death_backward", "StandDeathBackward");
        set.StandDeathLeft = CreateState(root, character, weapon, "stand", "death_left", "StandDeathLeft");
        set.StandDeathRight = CreateState(root, character, weapon, "stand", "death_right", "StandDeathRight");
        set.StandDeadhero = CreateState(root, character, weapon, "stand", "idle_emote", "StandDeadhero");

        set.ThrownBounceFrontSoft = CreateState(root, character, weapon, "thrown", "bouncefrontsoft", "ThrownBounceFrontSoft");
        set.ThrownBounceBackSoft = CreateState(root, character, weapon, "thrown", "bouncebacksoft", "ThrownBounceBackSoft");
        set.ThrownFlail = CreateState(root, character, weapon, "thrown", "flail", "ThrownFlail");
        set.ThrownFlyingFront = CreateState(root, character, weapon, "thrown", "flyingfront", "ThrownFlyingFront");
        set.ThrownFlyingBack = CreateState(root, character, weapon, "thrown", "flyingback", "ThrownFlyingBack");
        set.ThrownFlyingLeft = CreateState(root, character, weapon, "thrown", "flyingleft", "ThrownFlyingLeft");
        set.ThrownFlyingRight = CreateState(root, character, weapon, "thrown", "flyingright", "ThrownFlyingRight");
        set.ThrownLandFrontSoft = CreateState(root, character, weapon, "thrown", "landfrontsoft", "ThrownLandFrontSoft");
        set.ThrownLandBackSoft = CreateState(root, character, weapon, "thrown", "landbacksoft", "ThrownLandBackSoft");
        set.ThrownTumbleFront = CreateState(root, character, weapon, "thrown", "tumblefront", "ThrownTumbleFront");
        set.ThrownTumbleBack = CreateState(root, character, weapon, "thrown", "tumbleback", "ThrownTumbleBack");

        set.Sprint = CreateState(root, character, weapon, "crouch", "sprint", "Sprint");
        set.JetpackHover = CreateState(root, character, weapon, "crouch", "jetpack_hover", "JetpackHover");
        set.Jump = CreateState(root, character, weapon, "crouch", "jump", "Jump");
        set.Fall = CreateState(root, character, weapon, "crouch", "fall", "Fall");
        set.LandSoft = CreateState(root, character, weapon, "crouch", "landsoft", "LandSoft");
        set.LandHard = CreateState(root, character, weapon, "crouch", "landhard", "LandHard");

        return set;
    }

    public void PlayIntroAnim()
    {
        //LayerLower.SetActiveState(Sets[ActiveSet].StandReload);
        LayerUpper.SetActiveState(Sets[ActiveSet].StandReload);
    }

    public void SetActive(bool status = true)
    {
        Machine.SetActive(status);
    }
}