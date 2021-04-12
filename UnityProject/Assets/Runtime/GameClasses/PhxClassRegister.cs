using System;
using System.Collections.Generic;
using UnityEngine;

public static class PhxClassRegister
{
    struct GameBaseClass
    {
        public Type ClassType;
        public Type InstanceType;

        public GameBaseClass(Type ct, Type it)
        {
            ClassType = ct;
            InstanceType = it;
        }
    }

    static Dictionary<string, GameBaseClass> TypeDB = new Dictionary<string, GameBaseClass>()
    {
        { "commandpost",    new GameBaseClass(typeof(PhxCommandpost.ClassProperties),    typeof(PhxCommandpost))    },
        { "soldier",        new GameBaseClass(typeof(PhxSoldier.ClassProperties),        typeof(PhxSoldier))        },
        { "cannon",         new GameBaseClass(typeof(PhxCannon.ClassProperties),         typeof(PhxCannon))         },
        { "grenade",        new GameBaseClass(typeof(PhxGrenade.ClassProperties),        typeof(PhxGrenade))        },
        { "powerupstation", new GameBaseClass(typeof(PhxPowerupstation.ClassProperties), typeof(PhxPowerupstation)) },
    };

    public static Type GetPhxInstanceType(string name)
    {
        if (TypeDB.TryGetValue(name, out GameBaseClass cl))
        {
            return cl.InstanceType;
        }
        return null;
    }

    public static Type GetPhxClassType(string name)
    {
        if (TypeDB.TryGetValue(name, out GameBaseClass cl))
        {
            return cl.ClassType;
        }
        return null;
    }
}
