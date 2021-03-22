using System;
using System.Collections.Generic;
using UnityEngine;

public static class ClassRegister
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
        { "commandpost", new GameBaseClass(typeof(GC_commandpost.ClassProperties), typeof(GC_commandpost)) },
        { "soldier",     new GameBaseClass(typeof(GC_soldier.ClassProperties),     typeof(GC_soldier))     },
        { "cannon",      new GameBaseClass(typeof(GC_cannon.ClassProperties),      typeof(GC_cannon))      },
        { "grenade",     new GameBaseClass(typeof(GC_grenade.ClassProperties),     typeof(GC_grenade))     },
    };

    public static Type GetInstanceType(string name)
    {
        if (TypeDB.TryGetValue(name, out GameBaseClass cl))
        {
            return cl.InstanceType;
        }
        return null;
    }

    public static Type GetClassType(string name)
    {
        if (TypeDB.TryGetValue(name, out GameBaseClass cl))
        {
            return cl.ClassType;
        }
        return null;
    }
}
