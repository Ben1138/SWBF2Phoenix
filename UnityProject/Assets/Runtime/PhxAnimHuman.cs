using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;



// For each weapon, will shall generate one animation set
// Also, a Set should include a basepose (skeleton setup)
public struct PhxAnimSet
{
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

    public CraState StandIdle;
    public CraState StandIdleCheckweapon;
    public CraState StandIdleLookaround;
    public CraState StandWalkForward;
    public CraState StandWalkBackward;
    public CraState StandRunForward;
    public CraState StandRunBackward;
    public CraState StandReload;
    public CraState StandShootPrimary;
    public CraState StandShootSecondary;
    public CraState StandAlertIdle;
    public CraState StandAlertWalkForward;
    public CraState StandAlertWalkBackward;
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

    // Melee
    public CraState StandAttack;
    public CraState StandBlockForward;
    public CraState StandBlockLeft;
    public CraState StandBlockRight;
    public CraState StandDeadhero;

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
    public CraState Choking;
}

public struct PhxAnimHuman
{
    public CraStateMachine Anim { get; private set; }
    public CraInput InputMovementX { get; private set; }
    public CraInput InputMovementY { get; private set; }
    CraLayer LayerLower;
    CraLayer LayerUpper;

    CraState StateNone;
    PhxAnimSet[] Sets;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponNameToSetIdx;


    public PhxAnimHuman(Transform root, string characterAnimBank, string[] weaponAnimBanks)
    {
        Anim = CraStateMachine.CreateNew();
        WeaponNameToSetIdx = new Dictionary<string, int>();

        Resolver = new PhxAnimationResolver();

        LayerLower = Anim.NewLayer();
        LayerUpper = Anim.NewLayer();

        InputMovementX = Anim.NewInput(CraValueType.Float, "Input X");
        InputMovementY = Anim.NewInput(CraValueType.Float, "Input Y");

        StateNone = LayerUpper.NewState(CraPlayer.None, "None");

        Sets = new PhxAnimSet[weaponAnimBanks.Length];
        for (int i = 0; i < Sets.Length; ++i)
        {
            WeaponNameToSetIdx.Add(weaponAnimBanks[i], i);
            Sets[i] = GenerateSet(root, characterAnimBank, weaponAnimBanks[i]);

            Sets[i].StandIdle.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandWalkForward,
                TransitionTime = 0.15f,
                Or0 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.Greater,
                        Input = InputMovementX,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                        ValueAsAbsolute = true
                    }
                },
                Or1 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.Greater,
                        Input = InputMovementY,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                        ValueAsAbsolute = true
                    }
                },
            });


            Sets[i].StandWalkForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandIdle,
                TransitionTime = 0.15f,
                Or0 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.LessOrEqual,
                        Input = InputMovementX,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                        ValueAsAbsolute = true
                    },
                    And1 = new CraCondition
                    {
                        Type = CraConditionType.LessOrEqual,
                        Input = InputMovementY,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                        ValueAsAbsolute = true
                    }
                }
            });
            Sets[i].StandWalkForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandRunForward,
                TransitionTime = 0.15f,
                Or0 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.Greater,
                        Input = InputMovementX,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                        ValueAsAbsolute = true
                    },
                },
                Or1 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.Greater,
                        Input = InputMovementY,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                        ValueAsAbsolute = true
                    }
                }
            });

            Sets[i].StandRunForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandWalkForward,
                TransitionTime = 0.15f,
                Or0 = new CraConditionOr
                {
                    And0 = new CraCondition
                    {
                        Type = CraConditionType.LessOrEqual,
                        Input = InputMovementX,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                        ValueAsAbsolute = true
                    },
                    And1 = new CraCondition
                    {
                        Type = CraConditionType.LessOrEqual,
                        Input = InputMovementY,
                        Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                        ValueAsAbsolute = true
                    }
                },
            });
        }

        LayerLower.SetActiveState(Sets[0].StandIdle);
        LayerUpper.SetActiveState(StateNone);
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



    CraPlayer CreatePlayer(bool loop)
    {
        CraPlayer player = CraPlayer.CreateNew();
        player.SetLooping(loop);
        return player;
    }

    PhxAnimSet GenerateSet(Transform root, string character, string weapon)
    {
        PhxAnimSet set = new PhxAnimSet();

        //set.CrouchIdle = CreateState(root, character, weapon, "crouch", "idle_emote", "CrouchIdle");
        //set.CrouchIdleTakeknee = CreateState(root, character, weapon, "crouch", "idle_takeknee", "CrouchIdle");
        //set.CrouchHitFront = CreateState(root, character, weapon, "crouch", "hitfront", "CrouchHitFront");
        //set.CrouchHitLeft = CreateState(root, character, weapon, "crouch", "hitleft", "CrouchHitLeft");
        //set.CrouchHitRight = CreateState(root, character, weapon, "crouch", "hitright", "CrouchHitRight");
        //set.CrouchReload = CreateState(root, character, weapon, "crouch", "reload", "CrouchReload");
        //set.CrouchShoot = CreateState(root, character, weapon, "crouch", "shoot", "CrouchShoot");
        //set.CrouchTurnLeft = CreateState(root, character, weapon, "crouch", "turnleft", "CrouchTurnLeft");
        //set.CrouchTurnRight = CreateState(root, character, weapon, "crouch", "turnright", "CrouchTurnRight");
        //set.CrouchWalkForward = CreateState(root, character, weapon, "crouch", "walkforward", "CrouchWalkForward");
        //set.CrouchWalkBackward = CreateState(root, character, weapon, "crouch", "walkbackward", "CrouchWalkBackward");
        //set.CrouchAlertIdle = CreateState(root, character, weapon, "crouchalert", "idle_emote", "CrouchAlertIdle");
        //set.CrouchAlertWalkForward = CreateState(root, character, weapon, "crouchalert", "walkforward", "CrouchAlertWalkForward");
        //set.CrouchAlertWalkBackward = CreateState(root, character, weapon, "crouchalert", "walkbackward", "CrouchAlertWalkBackward");

        //set.StandIdle = CreateState(root, character, weapon, "stand", "idle_emote", "StandIdle");
        //set.StandIdleCheckweapon = CreateState(root, character, weapon, "stand", "idle_checkweapon", "StandIdleCheckweapon");
        //set.StandIdleLookaround = CreateState(root, character, weapon, "stand", "idle_lookaround", "StandIdleLookaround");
        //set.StandWalkForward = CreateState(root, character, weapon, "stand", "walkforward", "StandWalkForward");
        //set.StandRunForward = CreateState(root, character, weapon, "stand", "runforward", "StandRunForward");
        //set.StandRunBackward = CreateState(root, character, weapon, "stand", "runbackward", "StandRunBackward");
        //set.StandReload = CreateState(root, character, weapon, "stand", "reload", "StandReload");
        //set.StandShootPrimary = CreateState(root, character, weapon, "stand", "shoot", "StandShootPrimary");
        //set.StandShootSecondary = CreateState(root, character, weapon, "stand", "shoot_secondary", "StandShootSecondary");
        //set.StandAlertIdle = CreateState(root, character, weapon, "standalert", "idle_emote", "StandAlertIdle");
        //set.StandAlertWalkForward = CreateState(root, character, weapon, "standalert", "walkforward", "StandAlertWalkForward");
        //set.StandAlertRunForward = CreateState(root, character, weapon, "standalert", "runforward", "StandAlertRunForward");
        //set.StandAlertRunBackward = CreateState(root, character, weapon, "standalert", "runbackward", "StandAlertRunBackward");
        //set.StandTurnLeft = CreateState(root, character, weapon, "stand", "turnleft", "StandTurnLeft");
        //set.StandTurnRight = CreateState(root, character, weapon, "stand", "turnright", "StandTurnRight");
        //set.StandHitFront = CreateState(root, character, weapon, "stand", "gitfront", "StandHitFront");
        //set.StandHitBack = CreateState(root, character, weapon, "stand", "hitback", "StandHitBack");
        //set.StandHitLeft = CreateState(root, character, weapon, "stand", "hitleft", "StandHitLeft");
        //set.StandHitRight = CreateState(root, character, weapon, "stand", "hitright", "StandHitRight");
        //set.StandGetupFront = CreateState(root, character, weapon, "stand", "getupfront", "StandGetupFront");
        //set.StandGetupBack = CreateState(root, character, weapon, "stand", "getupback", "StandGetupBack");
        //set.StandDeathForward = CreateState(root, character, weapon, "stand", "death_forward", "StandDeathForward");
        //set.StandDeathBackward = CreateState(root, character, weapon, "stand", "death_backward", "StandDeathBackward");
        //set.StandDeathLeft = CreateState(root, character, weapon, "stand", "death_left", "StandDeathLeft");
        //set.StandDeathRight = CreateState(root, character, weapon, "stand", "death_right", "StandDeathRight");

        ////StandAttack = CreateState(root, character, weapon, "stand", "idle_emote", "StandAttack");
        ////StandBlockForward = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockForward");
        ////StandBlockLeft = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockLeft");
        ////StandBlockRight = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockRight");
        ////StandDeadhero = CreateState(root, character, weapon, "stand", "idle_emote", "StandDeadhero");

        //set.ThrownBounceFrontSoft = CreateState(root, character, weapon, "thrown", "bouncefrontsoft", "ThrownBounceFrontSoft");
        //set.ThrownBounceBackSoft = CreateState(root, character, weapon, "thrown", "bouncebacksoft", "ThrownBounceBackSoft");
        //set.ThrownFlail = CreateState(root, character, weapon, "thrown", "flail", "ThrownFlail");
        //set.ThrownFlyingFront = CreateState(root, character, weapon, "thrown", "flyingfront", "ThrownFlyingFront");
        //set.ThrownFlyingBack = CreateState(root, character, weapon, "thrown", "flyingback", "ThrownFlyingBack");
        //set.ThrownFlyingLeft = CreateState(root, character, weapon, "thrown", "flyingleft", "ThrownFlyingLeft");
        //set.ThrownFlyingRight = CreateState(root, character, weapon, "thrown", "flyingright", "ThrownFlyingRight");
        //set.ThrownLandFrontSoft = CreateState(root, character, weapon, "thrown", "landfrontsoft", "ThrownLandFrontSoft");
        //set.ThrownLandBackSoft = CreateState(root, character, weapon, "thrown", "landbacksoft", "ThrownLandBackSoft");
        //set.ThrownTumbleFront = CreateState(root, character, weapon, "thrown", "tumblefront", "ThrownTumbleFront");
        //set.ThrownTumbleBack = CreateState(root, character, weapon, "thrown", "tumbleback", "ThrownTumbleBack");

        //set.Sprint = CreateState(root, character, weapon, "crouch", "sprint", "Sprint");
        //set.JetpackHover = CreateState(root, character, weapon, "crouch", "jetpack_hover", "JetpackHover");
        //set.Jump = CreateState(root, character, weapon, "crouch", "jump", "Jump");
        //set.Fall = CreateState(root, character, weapon, "crouch", "fall", "Fall");
        //set.LandSoft = CreateState(root, character, weapon, "crouch", "landsoft", "LandSoft");
        //set.LandHard = CreateState(root, character, weapon, "crouch", "landhard", "LandHard");

        return set;
    }

    public void PlayIntroAnim()
    {
        LayerLower.SetActiveState(Sets[0].StandReload);
        LayerUpper.SetActiveState(Sets[0].StandReload);
    }

    public void SetAnimBank(string bankName)
    {
        if (string.IsNullOrEmpty(bankName))
        {
            return;
        }
        // TODO
    }

    public void SetActive(bool status = true)
    {
        Anim.SetActive(status);
    }
}