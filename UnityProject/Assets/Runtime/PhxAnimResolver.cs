using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PhxAnimDesc
{                              // E.g.:
    public string Character;   //  human
    public string Weapon;      //  rifle
    public string Posture;     //  stand
    public string Animation;   //  shoot (optional, e.g. sprint doesn't have these)
    public string Scope;       //  full  (optional)

    public string ParentCharacter;
    public string ParentWeapon;

    // Otherwise, it's a movement animation
    public bool IsWeaponAnimation()
    {
        if (string.IsNullOrEmpty(Animation))
        {
            return false;
        }

        PhxUtils.IntFromStringEnd(Animation, out string anim);
        return
            anim.ToLower() == "shoot" ||
            anim.ToLower() == "shoot_secondary" ||
            anim.ToLower() == "charge" ||
            anim.ToLower() == "reload";
    }

    public override string ToString()
    {
        return $"{Character}_{Weapon}_{Posture}{(!string.IsNullOrEmpty(Animation) ? $"_{Animation}" : "")}{(!string.IsNullOrEmpty(Scope) ? $"_{Scope}" : "")}";
    }

    public static bool operator ==(in PhxAnimDesc lhs, in PhxAnimDesc rhs)
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
    None, Lower, Upper, Full
}

public class PhxAnimationResolver
{
    readonly Dictionary<string, string> WeaponInheritance = new Dictionary<string, string>()
    {
        { "melee",   /* --> */ "tool"  },
        { "grenade", /* --> */ "tool"  },
        { "pistol",  /* --> */ "tool"  },
        { "tool",    /* --> */ "rifle" },
        { "bazooka", /* --> */ "rifle" },
    };

    // Add weapons with no alert states here.
    // See CustomAnimationBank odf property
    HashSet<string> NoAlertSupport = new HashSet<string>()
    {
        "bazooka", "melee"
    };

    struct PhxCached
    {
        public CraClip Clip;
        public PhxAnimScope Scope;
    }
    struct PhxClipSearch
    {
        public PhxAnimDesc Desc;
        public PhxAnimScope FindScope;
    }
    Dictionary<PhxClipSearch, PhxCached> ResolvedAnimations = new Dictionary<PhxClipSearch, PhxCached>();


    public void AddNoAlertSupportWeapon(string weapon)
    {
        NoAlertSupport.Add(weapon);
    }

    public bool ResolveAnim(ref PhxAnimDesc animDesc, out CraClip clip, out PhxAnimScope clipScope, PhxAnimScope findScope = PhxAnimScope.None)
    {
        if (ResolvedAnimations.TryGetValue(new PhxClipSearch { Desc = animDesc, FindScope = findScope }, out PhxCached result))
        {
            //Debug.Log($"Found already resolved clip: {animDesc}");
            clip = result.Clip;
            clipScope = result.Scope;
            return true;
        }

        bool found = ResolveAnim(animDesc, out PhxAnimDesc resolved, out clip, out clipScope, findScope);
        if (found)
        {
            ResolvedAnimations.Add(new PhxClipSearch { Desc = animDesc, FindScope = findScope }, new PhxCached { Clip = clip, Scope = clipScope });
            //if (resolved != animDesc)
            //{
            //    Debug.Log($"{animDesc} --> {resolved}");
            //}
        }
        animDesc = resolved;
        return found;
    }

    // Idk what's the better approach here. Either build up a tree to
    // resolve animation inheritance (which requires to know about all
    // animations beforehand), or just apply the rules recursively until
    // a matching animation is found. I'm going with the latter for now.
    bool ResolveAnim(PhxAnimDesc animDesc, out PhxAnimDesc found, out CraClip clip, out PhxAnimScope clipScope, PhxAnimScope findScope = PhxAnimScope.None)
    {
        clip = CraClip.None;
        if (FindScope(animDesc, out found, out clip, out clipScope, findScope))
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
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }

            // 2. From neighbouring animations (e.g. shoot1, shoot2, shoot3)
            //    Use FindScope here, since for this case we don't want 
            //    to traverse the inheritance tree any further.
            next = animDesc;
            int idx = PhxUtils.IntFromStringEnd(animDesc.Animation, out string anim);
            for (int i = 1; i < 10; ++i)
            {
                if (i != idx)
                {
                    next.Animation = $"{anim}{i}";
                    if (FindScope(next, out found, out clip, out clipScope, findScope))
                    {
                        return true;
                    }
                }
            }
            if (idx >= 0)
            {
                next.Animation = anim;
                if (FindScope(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }

            // 3. Posture fallback to Stand
            next = animDesc;
            if (next.Posture.ToLower() != "stand")
            {
                next.Posture = "stand";
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
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
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
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
                    if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                    {
                        return true;
                    }
                }
                else if (next.Posture.ToLower() == "crouchalert")
                {
                    next.Posture = "crouch";
                    if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                    {
                        return true;
                    }
                }
            }

            // 2. From character parent
            next = animDesc;
            if (next.Character.ToLower() != animDesc.ParentCharacter.ToLower())
            {
                next.Character = animDesc.ParentCharacter;
                next.ParentCharacter = null;
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }

            // 3. From weapon parent
            next = animDesc;
            if (next.Weapon.ToLower() != animDesc.ParentWeapon.ToLower())
            {
                next.Weapon = animDesc.ParentWeapon;
                next.ParentWeapon = null;
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
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
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }
            else if (next.Posture.ToLower() == "crouchalert")
            {
                next.Posture = "crouch";
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }

            // 5. Idle fallback
            next = animDesc;
            if (!string.IsNullOrEmpty(next.Animation) && ((next.Animation.ToLower() != "idle_emote" && next.Animation.ToLower().StartsWith("idle")) || next.Animation.ToLower().StartsWith("turn")))
            {
                next.Animation = "idle_emote";
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }

            // 6. Stand Idle posture fallback
            if (next.Posture.ToLower() != "stand")
            {
                next.Posture = "stand";
                if (ResolveAnim(next, out found, out clip, out clipScope, findScope))
                {
                    return true;
                }
            }
        }

        found = animDesc;
        clipScope = PhxAnimScope.Full;
        return false;
    }

    bool FindScope(PhxAnimDesc animDesc, out PhxAnimDesc found, out CraClip clip, out PhxAnimScope clipScope, PhxAnimScope findScope)
    {
        if (animDesc.ToString().Contains("sabre_stand_attack1a"))
        {
            Debug.Log("qwer");
        }

        if (PhxAnimLoader.Exists(animDesc.Character, animDesc.ToString()))
        {
            clip = PhxAnimLoader.SearchImport(animDesc.Character, animDesc.ToString());
            Debug.Assert(clip.IsValid());

            found = animDesc;
            if (string.IsNullOrEmpty(found.Scope))
            {
                if (!string.IsNullOrEmpty(found.Animation) && found.Animation.ToLower().StartsWith("turn"))
                {
                    clipScope = PhxAnimScope.Lower;
                }
                else if (animDesc.Character.ToLower() == "human" && animDesc.Weapon.ToLower() == "rifle")
                {
                    clipScope = PhxAnimScope.Full;
                }
                else
                {
                    clipScope = PhxAnimScope.Full;
                }
            }
            else
            {
                switch (found.Scope.ToLower())
                {
                    case "lower":
                        clipScope = PhxAnimScope.Lower;
                        break;
                    case "upper":
                        clipScope = PhxAnimScope.Upper;
                        break;
                    case "full":
                        clipScope = PhxAnimScope.Full;
                        break;
                    default:
                        Debug.LogError($"Unknown animation scope '{found.Scope}'!");
                        clipScope = PhxAnimScope.None;
                        break;
                }
            }

            if (findScope != PhxAnimScope.None && clipScope != PhxAnimScope.Full && clipScope != findScope)
            {
                return false;
            }

            // TODO: Finally load clip here
            return true;
        }

        if (!string.IsNullOrEmpty(animDesc.Scope))
        {
            found = animDesc;
            clipScope = PhxAnimScope.None;
            clip = CraClip.None;
            return false;
        }

        if (findScope == PhxAnimScope.None || findScope == PhxAnimScope.Lower)
        {
            animDesc.Scope = "lower";
            if (FindScope(animDesc, out found, out clip, out clipScope, findScope))
            {
                Debug.Assert(clipScope == PhxAnimScope.Lower);
                return true;
            }
        }

        if (findScope == PhxAnimScope.None || findScope == PhxAnimScope.Upper)
        {
            animDesc.Scope = "upper";
            if (FindScope(animDesc, out found, out clip, out clipScope, findScope))
            {
                Debug.Assert(clipScope == PhxAnimScope.Upper);
                return true;
            }
        }

        animDesc.Scope = "full";
        if (FindScope(animDesc, out found, out clip, out clipScope, findScope))
        {
            Debug.Assert(clipScope == PhxAnimScope.Full);
            return true;
        }

        found = animDesc;
        clipScope = PhxAnimScope.None;
        return false;
    }
}