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
            Debug.Assert(ct != null || it != null);

            ClassType = ct;
            InstanceType = it;
        }
    }

    // When setting either the class type or instance type to null, make sure that either noone will
    // attempt to resolve the class string to a class type, or create an instance of it respectively.
    static Dictionary<string, GameBaseClass> TypeDB = new Dictionary<string, GameBaseClass>()
    {
        { "commandpost",    new GameBaseClass(typeof(PhxCommandpost.ClassProperties),    typeof(PhxCommandpost))    },
        { "soldier",        new GameBaseClass(typeof(PhxSoldier.ClassProperties),        typeof(PhxSoldier))        },
        { "cannon",         new GameBaseClass(typeof(PhxCannon.ClassProperties),         typeof(PhxCannon))         },
        { "grenade",        new GameBaseClass(typeof(PhxGrenade.ClassProperties),        typeof(PhxGrenade))        },
        { "powerupstation", new GameBaseClass(typeof(PhxPowerupstation.ClassProperties), typeof(PhxPowerupstation)) },
        // Right now, there's custom object pooling just for projectiles, meaning, PhxBolt's are not instantiated via
        // PhxRuntimeScene.CreateInstance(), but with PhxProjectiles.FireProjectile()
        // Maybe this will be obsolete once we've got a generic object pooling for everything. Idk yet.
        { "bolt",           new GameBaseClass(typeof(PhxBolt),                           null)                      },
        { "hover",          new GameBaseClass(typeof(PhxHover.PhxHoverProperties),       typeof(PhxHover))          },
        { "vehiclespawn",   new GameBaseClass(null,                                      typeof(PhxVehicleSpawn))   },
        { "weapon",         new GameBaseClass(typeof(PhxWeapon.ClassProperties),         typeof(PhxWeapon))         },
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
