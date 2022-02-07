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

public struct PhxAnimDesc
{                       // E.g.:
    public string Character;   //  human
    public string Weapon;      //  rifle
    public string Posture;     //  stand
    public string Animation;   //  shoot
    public string Scope;       //  full

    public string ParentCharacter;
    public string ParentWeapon;

    // Otherwise, it's a movement animation
    public bool IsWeaponAnimation()
    {
        PhxUtils.IntFromStringEnd(Animation, out string anim);
        return
            anim.ToLower() == "shoot" ||
            anim.ToLower() == "shoot_secondary" ||
            anim.ToLower() == "charge" ||
            anim.ToLower() == "reload";
    }

    public override string ToString()
    {
        return $"{Character}_{Weapon}_{Posture}_{Animation}{(!string.IsNullOrEmpty(Scope) ? $"_{Scope}" : "")}";
    }

    public static bool operator==(in PhxAnimDesc lhs, in PhxAnimDesc rhs)
    {
        return
            lhs.Character == rhs.Character &&
            lhs.Weapon == rhs.Weapon &&
            lhs.Posture == rhs.Posture &&
            lhs.Animation == rhs.Animation &&
            lhs.Scope == rhs.Scope;
    }

    public static bool operator !=(in PhxAnimDesc lhs, in PhxAnimDesc rhs)
    {
        return !(lhs == rhs);
    }
}

public enum PhxAnimScope
{
    Lower, Upper, Full
}

public class PhxAnimationResolver
{
    readonly Dictionary<string, string> WeaponInheritance = new Dictionary<string, string>()
    {
        { "melee",   /* --> */ "tool"   },
        { "grenade", /* --> */ "tool"   },
        { "tool",    /* --> */ "pistol" },
        { "pistol",  /* --> */ "rifle"  },
        { "bazooka", /* --> */ "rifle"  },
    };

    // Add weapons with no alert states here.
    // See CustomAnimationBank odf property
    HashSet<string> NoAlertSupport = new HashSet<string>()
    {
        "bazooka", "melee"
    };


    public void AddNoAlertSupportWeapon(string weapon)
    {
        NoAlertSupport.Add(weapon);
    }

    public bool ResolveAnim(PhxAnimDesc animDesc, out CraClip clip, out PhxAnimScope scope, out bool loop)
    {
        bool found = ResolveAnim(animDesc, out PhxAnimDesc resolved, out clip, out scope);
        if (found && resolved != animDesc)
        {
            Debug.Log($"{animDesc} --> {resolved}");
        }
        // Don't loop weapon and turn animations, but everything else
        loop = !resolved.IsWeaponAnimation() && !resolved.Animation.ToLower().StartsWith("turn");
        return found;
    }

    // Idk what's the better approach here. Either build up a tree to
    // resolve animation inheritance (which requires to know about all
    // animations beforehand), or just apply the rules recursively until
    // a matching animation is found. I'm going with the latter for now.
    public bool ResolveAnim(PhxAnimDesc animDesc, out PhxAnimDesc found, out CraClip clip, out PhxAnimScope scope)
    {
        clip = CraClip.None;
        while (!clip.IsValid())
        {
            if (FindScope(animDesc, out found, out clip, out scope))
            {
                return true;
            }

            // Ensure parents
            if (string.IsNullOrEmpty(animDesc.ParentCharacter))
            {
                animDesc.ParentCharacter = "human";
            }
            if (string.IsNullOrEmpty(animDesc.ParentWeapon))
            {
                if (!WeaponInheritance.TryGetValue(animDesc.Weapon, out animDesc.ParentWeapon))
                {
                    animDesc.ParentWeapon = "rifle";
                }
            }

            PhxAnimDesc next;
            if (animDesc.IsWeaponAnimation())
            {
                // Apply weapon inheritance rules in strict
                // order until a matching animation is found.

                // 1. From character parent
                next = animDesc;
                if (next.Character.ToLower() != animDesc.ParentCharacter.ToLower())
                {
                    next.Character = animDesc.ParentCharacter;
                    next.ParentCharacter = null;
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 2. From neighbouring animations (e.g. shoot1, shoot2, shoot3)
                next = animDesc;
                int idx = PhxUtils.IntFromStringEnd(animDesc.Animation, out string anim);
                for (int i = 1; i < 10; ++i)
                {
                    if (i != idx)
                    {
                        next.Animation = $"{anim}{i}";
                        if (ResolveAnim(next, out found, out clip, out scope))
                        {
                            return true;
                        }
                    }
                }
                if (idx >= 0)
                {
                    next.Animation = anim;
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 3. Posture fallback to Stand
                next = animDesc;
                if (next.Posture.ToLower() != "stand")
                {
                    next.Posture = "stand";
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 4. From weapon parent
                next = animDesc;
                if (next.Weapon.ToLower() != animDesc.ParentWeapon.ToLower())
                {
                    next.Weapon = animDesc.ParentWeapon;
                    next.ParentWeapon = null;
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // Apply movement inheritance rules in strict
                // order until a matching animation is found.

                // 1. When weapon doesn't support alert state, try fall back to non-alert postures first!
                next = animDesc;
                if (NoAlertSupport.Contains(next.Weapon))
                {
                    if (next.Posture.ToLower() == "standalert")
                    {
                        next.Posture = "stand";
                        if (ResolveAnim(next, out found, out clip, out scope))
                        {
                            return true;
                        }
                    }
                    else if (next.Posture.ToLower() == "crouchalert")
                    {
                        next.Posture = "crouch";
                        if (ResolveAnim(next, out found, out clip, out scope))
                        {
                            return true;
                        }
                    }
                }

                // 2. From weapon parent
                next = animDesc;
                if (next.Weapon.ToLower() != animDesc.ParentWeapon.ToLower())
                {
                    next.Weapon = animDesc.ParentWeapon;
                    next.ParentWeapon = null;
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 3. From character parent
                next = animDesc;
                if (next.Character.ToLower() != animDesc.ParentCharacter.ToLower())
                {
                    next.Character = animDesc.ParentCharacter;
                    next.ParentCharacter = null;
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 4. Now also fall back to non-alert postures
                //    for weapons that do support alert states
                next = animDesc;
                if (next.Posture.ToLower() == "standalert")
                {
                    next.Posture = "stand";
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }
                else if (next.Posture.ToLower() == "crouchalert")
                {
                    next.Posture = "crouch";
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 5. Idle fallback
                next = animDesc;
                if (next.Animation.ToLower().StartsWith("idle") || next.Animation.ToLower().StartsWith("turn"))
                {
                    next.Animation = "idle_emote";
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }

                // 6. Stand Idle posture fallback
                if (next.Posture.ToLower() != "stand")
                {
                    next.Posture = "stand";
                    if (ResolveAnim(next, out found, out clip, out scope))
                    {
                        return true;
                    }
                }
            }
        }

        found = animDesc;
        scope = PhxAnimScope.Full;
        return false;
    }

    bool FindScope(PhxAnimDesc animDesc, out PhxAnimDesc found, out CraClip clip, out PhxAnimScope scope)
    {
        clip = PhxAnimationLoader.Import(animDesc.Character, animDesc.ToString());
        if (clip.IsValid())
        {
            found = animDesc;
            if (string.IsNullOrEmpty(found.Scope))
            {
                if (found.IsWeaponAnimation())
                {
                    scope = PhxAnimScope.Upper;
                }
                else if (found.Animation.ToLower().StartsWith("turn"))
                {
                    scope = PhxAnimScope.Lower;
                }
                else
                {
                    scope = PhxAnimScope.Full;
                }
            }
            else
            {
                switch (found.Scope.ToLower())
                {
                    case "lower":
                        scope = PhxAnimScope.Lower;
                        break;
                    case "upper":
                        scope = PhxAnimScope.Upper;
                        break;
                    case "full":
                        scope = PhxAnimScope.Full;
                        break;
                    default:
                        Debug.LogError($"Unknown animation scope '{found.Scope}'!");
                        scope = PhxAnimScope.Full;
                        break;
                }
            }
            return true;
        }

        if (!string.IsNullOrEmpty(animDesc.Scope))
        {
            found = animDesc;
            scope = PhxAnimScope.Full;
            return false;
        }

        animDesc.Scope = "lower";
        if (FindScope(animDesc, out found, out clip, out scope))
        {
            Debug.Assert(scope == PhxAnimScope.Lower);
            return true;
        }

        animDesc.Scope = "upper";
        if (FindScope(animDesc, out found, out clip, out scope))
        {
            Debug.Assert(scope == PhxAnimScope.Upper);
            return true;
        }

        animDesc.Scope = "full";
        if (FindScope(animDesc, out found, out clip, out scope))
        {
            Debug.Assert(scope == PhxAnimScope.Full);
            return true;
        }

        return false;
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
    PhxAnimSet[] Sets;

    PhxAnimationResolver Resolver;
    Dictionary<string, int> WeaponNameToSetIdx;


    public PhxHumanAnimator(Transform root, string characterAnimBank, string[] weaponAnimBanks)
    {
        Anim = CraStateMachine.CreateNew();
        WeaponNameToSetIdx = new Dictionary<string, int>();

        Resolver = new PhxAnimationResolver();

        LayerLower = Anim.NewLayer();
        LayerUpper = Anim.NewLayer();

        var bank = PhxAnimationBanks.Banks["human"]["rifle"];

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


            Sets[i].StandWalkForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandIdle,
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
            Sets[i].StandWalkForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandRunForward,
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

            Sets[i].StandRunForward.NewTransition(new CraTransitionData
            {
                Target = Sets[i].StandWalkForward,
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
        }
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

    CraState CreateState(Transform root, string character, string weapon, string posture, string anim, string stateName =null)
    {
        PhxAnimDesc animDesc = new PhxAnimDesc { Character = character, Weapon = weapon, Posture = posture, Animation = anim };
        if (!Resolver.ResolveAnim(animDesc, out CraClip clip, out PhxAnimScope scope, out bool loop))
        {
            return CraState.None;
        }
        Debug.Assert(clip.IsValid());
        CraState state = CraState.None;
        switch(scope)
        {
            case PhxAnimScope.Lower:
                state = LayerLower.NewState(PhxAnimationLoader.CreatePlayer(root, loop, "bone_pelvis"), stateName);
                break;
            case PhxAnimScope.Upper:
                state = LayerUpper.NewState(PhxAnimationLoader.CreatePlayer(root, loop, "root_a_spine"), stateName);
                break;
            case PhxAnimScope.Full:
                state = LayerUpper.NewState(PhxAnimationLoader.CreatePlayer(root, loop), stateName);
                break;
        }
        Debug.Assert(state.IsValid());
        state.GetPlayer().SetClip(clip);
        return state;
    }

    PhxAnimSet GenerateSet(Transform root, string character, string weapon)
    {
        return new PhxAnimSet
        {
            CrouchIdle = CreateState(root, character, weapon, "crouch", "idle_emote", "CrouchIdle"),
            CrouchIdleTakeknee = CreateState(root, character, weapon, "crouch", "idle_takeknee", "CrouchIdle"),
            CrouchHitFront = CreateState(root, character, weapon, "crouch", "hitfront", "CrouchHitFront"),
            CrouchHitLeft = CreateState(root, character, weapon, "crouch", "hitleft", "CrouchHitLeft"),
            CrouchHitRight = CreateState(root, character, weapon, "crouch", "hitright", "CrouchHitRight"),
            CrouchReload = CreateState(root, character, weapon, "crouch", "reload", "CrouchReload"),
            CrouchShoot = CreateState(root, character, weapon, "crouch", "shoot", "CrouchShoot"),
            CrouchTurnLeft = CreateState(root, character, weapon, "crouch", "turnleft", "CrouchTurnLeft"),
            CrouchTurnRight = CreateState(root, character, weapon, "crouch", "turnright", "CrouchTurnRight"),
            CrouchWalkForward = CreateState(root, character, weapon, "crouch", "walkforward", "CrouchWalkForward"),
            CrouchWalkBackward = CreateState(root, character, weapon, "crouch", "walkbackward", "CrouchWalkBackward"),
            CrouchAlertIdle = CreateState(root, character, weapon, "crouchalert", "idle_emote", "CrouchAlertIdle"),
            CrouchAlertWalkForward = CreateState(root, character, weapon, "crouchalert", "walkforward", "CrouchAlertWalkForward"),
            CrouchAlertWalkBackward = CreateState(root, character, weapon, "crouchalert", "walkbackward", "CrouchAlertWalkBackward"),

            StandIdle = CreateState(root, character, weapon, "stand", "idle_emote", "StandIdle"),
            StandIdleCheckweapon = CreateState(root, character, weapon, "stand", "idle_checkweapon", "StandIdleCheckweapon"),
            StandIdleLookaround = CreateState(root, character, weapon, "stand", "idle_lookaround", "StandIdleLookaround"),
            StandWalkForward = CreateState(root, character, weapon, "stand", "walkforward", "StandWalkForward"),
            StandRunForward = CreateState(root, character, weapon, "stand", "runforward", "StandRunForward"),
            StandRunBackward = CreateState(root, character, weapon, "stand", "runbackward", "StandRunBackward"),
            StandReload = CreateState(root, character, weapon, "stand", "reload", "StandReload"),
            StandShootPrimary = CreateState(root, character, weapon, "stand", "shoot", "StandShootPrimary"),
            StandShootSecondary = CreateState(root, character, weapon, "stand", "shoot_secondary", "StandShootSecondary"),
            StandAlertIdle = CreateState(root, character, weapon, "standalert", "idle_emote", "StandAlertIdle"),
            StandAlertWalkForward = CreateState(root, character, weapon, "standalert", "walkforward", "StandAlertWalkForward"),
            StandAlertRunForward = CreateState(root, character, weapon, "standalert", "runforward", "StandAlertRunForward"),
            StandAlertRunBackward = CreateState(root, character, weapon, "standalert", "runbackward", "StandAlertRunBackward"),
            StandTurnLeft = CreateState(root, character, weapon, "stand", "turnleft", "StandTurnLeft"),
            StandTurnRight = CreateState(root, character, weapon, "stand", "turnright", "StandTurnRight"),
            StandHitFront = CreateState(root, character, weapon, "stand", "gitfront", "StandHitFront"),
            StandHitBack = CreateState(root, character, weapon, "stand", "hitback", "StandHitBack"),
            StandHitLeft = CreateState(root, character, weapon, "stand", "hitleft", "StandHitLeft"),
            StandHitRight = CreateState(root, character, weapon, "stand", "hitright", "StandHitRight"),
            StandGetupFront = CreateState(root, character, weapon, "stand", "getupfront", "StandGetupFront"),
            StandGetupBack = CreateState(root, character, weapon, "stand", "getupback", "StandGetupBack"),
            StandDeathForward = CreateState(root, character, weapon, "stand", "death_forward", "StandDeathForward"),
            StandDeathBackward = CreateState(root, character, weapon, "stand", "death_backward", "StandDeathBackward"),
            StandDeathLeft = CreateState(root, character, weapon, "stand", "death_left", "StandDeathLeft"),
            StandDeathRight = CreateState(root, character, weapon, "stand", "death_right", "StandDeathRight"),

            //StandAttack = CreateState(root, character, weapon, "stand", "idle_emote", "StandAttack"),
            //StandBlockForward = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockForward"),
            //StandBlockLeft = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockLeft"),
            //StandBlockRight = CreateState(root, character, weapon, "stand", "idle_emote", "StandBlockRight"),
            //StandDeadhero = CreateState(root, character, weapon, "stand", "idle_emote", "StandDeadhero"),

            ThrownBounceFrontSoft = CreateState(root, character, weapon, "thrown", "bouncefrontsoft", "ThrownBounceFrontSoft"),
            ThrownBounceBackSoft = CreateState(root, character, weapon, "thrown", "bouncebacksoft", "ThrownBounceBackSoft"),
            ThrownFlail = CreateState(root, character, weapon, "thrown", "flail", "ThrownFlail"),
            ThrownFlyingFront = CreateState(root, character, weapon, "thrown", "flyingfront", "ThrownFlyingFront"),
            ThrownFlyingBack = CreateState(root, character, weapon, "thrown", "flyingback", "ThrownFlyingBack"),
            ThrownFlyingLeft = CreateState(root, character, weapon, "thrown", "flyingleft", "ThrownFlyingLeft"),
            ThrownFlyingRight = CreateState(root, character, weapon, "thrown", "flyingright", "ThrownFlyingRight"),
            ThrownLandFrontSoft = CreateState(root, character, weapon, "thrown", "landfrontsoft", "ThrownLandFrontSoft"),
            ThrownLandBackSoft = CreateState(root, character, weapon, "thrown", "landbacksoft", "ThrownLandBackSoft"),
            ThrownTumbleFront = CreateState(root, character, weapon, "thrown", "tumblefront", "ThrownTumbleFront"),
            ThrownTumbleBack = CreateState(root, character, weapon, "thrown", "tumbleback", "ThrownTumbleBack"),

            Sprint = CreateState(root, character, weapon, "crouch", "sprint", "Sprint"),
            JetpackHover = CreateState(root, character, weapon, "crouch", "jetpack_hover", "JetpackHover"),
            Jump = CreateState(root, character, weapon, "crouch", "jump", "Jump"),
            Fall = CreateState(root, character, weapon, "crouch", "fall", "Fall"),
            LandSoft = CreateState(root, character, weapon, "crouch", "landsoft", "LandSoft"),
            LandHard = CreateState(root, character, weapon, "crouch", "landhard", "LandHard"),
        };
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