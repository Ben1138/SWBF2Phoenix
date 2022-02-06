using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

public static class PhxAnimationBanks
{
    public struct PhxAnimBank
    {
        public string StandSprint;
        public string StandRun;
        public string StandWalk;
        public string StandIdle;
        public string StandBackward;
        public string StandReload;
        public string StandShootPrimary;
        public string StandShootSecondary;
        public string StandAlertIdle;
        public string StandAlertWalk;
        public string StandAlertRun;
        public string StandAlertBackward;
        public string Jump;
        public string Fall;
        public string LandSoft;
        public string LandHard;
        public string TurnLeft;
        public string TurnRight;
    }

    public static readonly Dictionary<string, Dictionary<string, PhxAnimBank>> Banks = new Dictionary<string, Dictionary<string, PhxAnimBank>>()
    {
        { 
            "human", new Dictionary<string, PhxAnimBank>() 
            { 
                { 
                    "rifle", 
                    new PhxAnimBank
                    {
                        StandIdle = "human_rifle_stand_idle_emote_full", 
                        StandRun = "human_rifle_stand_runforward",
                        StandWalk = "human_rifle_stand_walkforward",
                        StandSprint = "human_rifle_sprint_full",
                        StandBackward = "human_rifle_stand_runbackward",
                        StandReload = "human_rifle_stand_reload_full",
                        StandShootPrimary = "human_rifle_stand_shoot_full",
                        StandShootSecondary = "human_rifle_stand_shoot_secondary_full",
                        StandAlertIdle = "human_rifle_standalert_idle_emote_full",
                        StandAlertWalk = "human_rifle_standalert_walkforward",
                        StandAlertRun = "human_rifle_standalert_runforward",
                        StandAlertBackward = "human_rifle_standalert_runbackward",
                        Jump = "human_rifle_jump",
                        Fall = "human_rifle_fall",
                        LandSoft = "human_rifle_landsoft",
                        LandHard = "human_rifle_landhard",
                        TurnLeft = "human_rifle_stand_turnleft",
                        TurnRight = "human_rifle_stand_turnright"
                    }
                },
                {
                    "pistol",
                    new PhxAnimBank
                    {
                        StandIdle = "human_tool_stand_idle_emote",                          // tool
                        StandRun = "human_pistol_stand_runforward",
                        StandWalk = "human_pistol_stand_walkforward",
                        StandSprint = "human_pistol_sprint",
                        StandBackward = "human_tool_stand_runbackward",                     // tool
                        StandReload = "human_pistol_stand_reload",
                        StandShootPrimary = "human_pistol_stand_shoot",
                        StandShootSecondary = "human_rifle_stand_shoot_secondary_full",     // rifle
                        StandAlertIdle = "human_pistol_standalert_idle_emote",
                        StandAlertWalk = "human_pistol_standalert_walkforward_full",
                        StandAlertRun = "human_pistol_standalert_runforward_full",
                        StandAlertBackward = "human_pistol_standalert_runbackward",
                        Jump = "human_tool_jump",
                        Fall = "human_tool_fall",                                           // tool
                        LandSoft = "human_tool_landsoft",                                   // tool
                        LandHard = "human_tool_landhard",                                   // tool
                        TurnLeft = "human_rifle_stand_turnleft",                            // rifle
                        TurnRight = "human_rifle_stand_turnright"                           // rifle
                    }
                },
                { 
                    "bazooka", 
                    new PhxAnimBank
                    {
                        StandIdle = "human_bazooka_stand_idle_emote", 
                        StandRun = "human_bazooka_stand_runforward",
                        StandWalk = "human_bazooka_stand_walkforward",
                        StandSprint = "human_bazooka_sprint",
                        StandBackward = "human_bazooka_stand_runbackward",
                        StandReload = "human_bazooka_stand_reload_full",
                        StandShootPrimary = "human_bazooka_stand_shoot_full",
                        StandShootSecondary = "human_bazooka_stand_shoot_secondary",
                        StandAlertIdle = "human_bazooka_standalert_idle_emote",
                        StandAlertWalk = "human_bazooka_standalert_walkforward",
                        StandAlertRun = "human_bazooka_standalert_runforward",
                        StandAlertBackward = "human_bazooka_standalert_runbackward",
                        Jump = "human_bazooka_jump",
                        Fall = "human_bazooka_fall",
                        LandSoft = "human_bazooka_landsoft",
                        LandHard = "human_bazooka_landhard",
                        TurnLeft = "human_rifle_stand_turnleft",
                        TurnRight = "human_rifle_stand_turnright"
                    }
                },
            } 
        }
    };
}

// For each weapon, will shall generate one animation set
// Also, a Set should include a basepose (skeleton setup)
public struct PhxAnimSet
{
    public CraState CrouchIdle;
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
    public CraState StandWalk;
    public CraState StandRun;
    public CraState StandBackward;
    public CraState StandReload;
    public CraState StandShootPrimary;
    public CraState StandShootSecondary;
    public CraState StandAlertIdle;
    public CraState StandAlertWalk;
    public CraState StandAlertRun;
    public CraState StandAlertBackward;
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
    public CraState ThrownBounceFlail;
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

public struct PhxAnimDesc
{                       // E.g.:
    public string Character;   //  human
    public string Weapon;      //  rifle
    public string Posture;     //  stand
    public string Animation;   //  shoot
    public string Scope;       //  full

    public string ParentCharacter;
    public string ParentWeapon;

    public override string ToString()
    {
        return $"{Character}_{Weapon}_{Posture}_{Animation}{(!string.IsNullOrEmpty(Scope) ? $"_{Scope}" : "")}";
    }
}

public struct PhxAnimator
{
    LibSWBF2.Wrappers.Container Con;

    static readonly Dictionary<string, string> WeaponInheritance = new Dictionary<string, string>()
    {
        { "melee",   /* --> */ "tool" },
        { "grenade", /* --> */ "tool" },
        { "tool",    /* --> */ "pistol" },
        { "pistol",  /* --> */ "rifle" },
        { "bazooka", /* --> */ "rifle" },
    };

    public void AddAnimPath(PhxAnimDesc animDesc)
    {


        // TODO: insert into tree
    }

    public PhxAnimSet BakeAnimSet(PhxAnimDesc animDesc)
    {
        
    }

    // Idk what's the better approach here. Either build up a tree
    // (which requires to know about all animations beforehand), to
    // resolve animation inheritance, or just apply the rules recursively
    // until a matching animation is found. I'm going with the latter for now.
    CraClip ResolveAnim(in PhxAnimDesc animDesc)
    {
        CraClip clip = CraClip.None;
        while (!clip.IsValid())
        {
            PhxAnimDesc next = animDesc;

            if (string.IsNullOrEmpty(animDesc.ParentCharacter))
            {
                next.ParentCharacter = "human";
            }
            if (string.IsNullOrEmpty(animDesc.ParentWeapon))
            {
                if (!WeaponInheritance.TryGetValue(animDesc.Weapon, out next.ParentWeapon))
                {
                    next.ParentWeapon = "rifle";
                }
            }

            clip = PhxAnimationLoader.Import(animDesc.Character, animDesc.ToString());
            if (!clip.IsValid())
            {
                if (animDesc.Character == "human" && animDesc.Weapon == "rifle")
                {
                    // Reached root
                    break;
                }

                // Apply weapon inheritance rules

                // 1. From character parent              
                next.Character = animDesc.ParentCharacter;
                next.ParentCharacter = null;
                clip = ResolveAnim(next);
                if (clip.IsValid())
                {
                    return clip;
                }

                // 2. From neighbouring animations
                int idx = -1;
                int.TryParse(animDesc.Animation.ToCharArray()[animDesc.Animation.Length - 1].ToString(), out idx);
                for (int i = 1; i < 10; ++i)
                {
                    
                }
            }
        }
        return clip;
    }

    string GetParent(string charSet)
    {
        return "human";
    }

    string GetWeaponParent(string weapon)
    {
        if (WeaponInheritance.TryGetValue(weapon, out string parent))
        {
            return parent;
        }
        return "rifle";
    }
}

public struct PhxHumanAnimator
{
    public CraStateMachine Anim { get; private set; }
    public CraInput InputMovementX { get; private set; }
    public CraInput InputMovementY { get; private set; }
    CraLayer LayerLower;
    CraLayer LayerUpper;

    CraState StateNone;
    PhxAnimSet Bank;

    Dictionary<string, int> NameToBankIdx;


    public PhxHumanAnimator(Transform root, string[] weaponAnimBanks)
    {
        Anim = CraStateMachine.CreateNew();
        NameToBankIdx = new Dictionary<string, int>();


        LayerLower = Anim.NewLayer();
        LayerUpper = Anim.NewLayer();

        var bank = PhxAnimationBanks.Banks["human"]["rifle"];

        InputMovementX = Anim.NewInput(CraValueType.Float, "Input X");
        InputMovementY = Anim.NewInput(CraValueType.Float, "Input Y");

        StateNone = LayerUpper.NewState(CraPlayer.None, "None");

        Bank = new PhxAnimSet();
        Bank = new PhxAnimSet
        {
            StandIdle = CreateState(root, HUMANM_BANKS, bank.StandIdle, true, null, "StandIdle"),
            StandWalk = CreateState(root, HUMANM_BANKS, bank.StandWalk, true, null, "StandWalk"),
            StandRun = CreateState(root, HUMANM_BANKS, bank.StandRun, true, null, "StandRun"),
            StandSprint = CreateState(root, HUMANM_BANKS, bank.StandSprint, true, null, "StandSprint"),
            StandBackward = CreateState(root, HUMANM_BANKS, bank.StandBackward, true, null, "StandBackward"),
            StandReload = CreateState(root, HUMANM_BANKS, bank.StandReload, false, "bone_a_spine", "StandReload"),
            StandShootPrimary = CreateState(root, HUMANM_BANKS, bank.StandShootPrimary, false, "bone_a_spine", "StandShootPrimary"),
            StandShootSecondary = CreateState(root, HUMANM_BANKS, bank.StandShootSecondary, false, "bone_a_spine", "StandShootSecondary"),
            StandAlertIdle = CreateState(root, HUMANM_BANKS, bank.StandAlertIdle, true, null, "StandAlertIdle"),
            StandAlertWalk = CreateState(root, HUMANM_BANKS, bank.StandAlertWalk, true, null, "StandAlertWalk"),
            StandAlertRun = CreateState(root, HUMANM_BANKS, bank.StandAlertRun, true, null, "StandAlertRun"),
            StandAlertBackward = CreateState(root, HUMANM_BANKS, bank.StandAlertBackward, true, null, "StandAlertBackward"),
            Jump = CreateState(root, HUMANM_BANKS, bank.Jump, false, null, "Jump"),
            Fall = CreateState(root, HUMANM_BANKS, bank.Fall, true, null, "Fall"),
            LandSoft = CreateState(root, HUMANM_BANKS, bank.LandSoft, true, null, "LandSoft"),
            LandHard = CreateState(root, HUMANM_BANKS, bank.LandHard, true, null, "LandHard"),
            TurnLeft = CreateState(root, HUMANM_BANKS, bank.TurnLeft, true, null, "TurnLeft"),
            TurnRight = CreateState(root, HUMANM_BANKS, bank.TurnRight, true, null, "TurnRight")
        };

        Bank.StandIdle.NewTransition(new CraTransitionData
        {
            Target = Bank.StandWalk,
            TransitionTime = 0.15f,
            Or1 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    ValueAsAbsolute = true
                }
            },
            Or2 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    ValueAsAbsolute = true
                }
            },
        });


        Bank.StandWalk.NewTransition(new CraTransitionData
        {
            Target = Bank.StandIdle,
            TransitionTime = 0.15f,
            Or1 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    ValueAsAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.2f },
                    ValueAsAbsolute = true
                }
            }
        });
        Bank.StandWalk.NewTransition(new CraTransitionData
        {
            Target = Bank.StandRun,
            TransitionTime = 0.15f,
            Or1 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    ValueAsAbsolute = true
                },
            },
            Or2 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.Greater,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    ValueAsAbsolute = true
                }
            }
        });

        Bank.StandRun.NewTransition(new CraTransitionData
        {
            Target = Bank.StandWalk,
            TransitionTime = 0.15f,
            Or1 = new CraConditionOr
            {
                And1 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementX,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    ValueAsAbsolute = true
                },
                And2 = new CraCondition
                {
                    Type = CraConditionType.LessOrEqual,
                    Input = InputMovementY,
                    Value = new CraValueUnion { Type = CraValueType.Float, ValueFloat = 0.75f },
                    ValueAsAbsolute = true
                }
            },
        });

        LayerLower.SetActiveState(Bank.StandIdle);
    }

    CraState CreateState(Transform root, string[] animBankNames, string animName, bool looping, string boneMaskName=null, string stateName =null)
    {
        CraState state = LayerLower.NewState(PhxAnimationLoader.CreatePlayer(root, looping, boneMaskName), stateName);
        state.GetPlayer().SetClip(PhxAnimationLoader.Import(animBankNames, animName));
        return state;
    }

    public void PlayIntroAnim()
    {
        LayerUpper.SetActiveState(Bank.StandReload);
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