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
    // attempt to resolve the class string to a class type, or create an instance of it, respectively.
    static Dictionary<string, GameBaseClass> TypeDB = new Dictionary<string, GameBaseClass>()
    {
        { "commandpost",    new GameBaseClass(typeof(PhxCommandpost.ClassProperties),    typeof(PhxCommandpost))    },
        { "soldier",        new GameBaseClass(typeof(PhxSoldier.ClassProperties),        typeof(PhxSoldier))        },
        { "powerupstation", new GameBaseClass(typeof(PhxPowerupstation.ClassProperties), typeof(PhxPowerupstation)) },
        { "hover",          new GameBaseClass(typeof(PhxHover.ClassProperties),          typeof(PhxHover))          },
        { "commandhover",   new GameBaseClass(typeof(PhxHover.ClassProperties),          typeof(PhxHover))          },
        { "armedbuilding",  new GameBaseClass(typeof(PhxArmedBuilding.ClassProperties),  typeof(PhxArmedBuilding))  },
        { "vehiclespawn",   new GameBaseClass(null,                                      typeof(PhxVehicleSpawn))   },

        { "leafpatch",      new GameBaseClass(typeof(PhxLeafPatchClass),                 typeof(PhxLeafPatch))      },

        { "prop",           new GameBaseClass(typeof(PhxProp.ClassProperties),           typeof(PhxProp))           },
        
        { "weapon",         new GameBaseClass(typeof(PhxGenericWeapon.ClassProperties),  typeof(PhxGenericWeapon))  },
        { "grenade",        new GameBaseClass(typeof(PhxGrenade.ClassProperties),        typeof(PhxGrenade))        },
        { "launcher",       new GameBaseClass(typeof(PhxGenericWeapon.ClassProperties),  typeof(PhxGenericWeapon))  },
        { "cannon",         new GameBaseClass(typeof(PhxCannon.ClassProperties),         typeof(PhxCannon))         },

        { "door",           new GameBaseClass(typeof(PhxDoor.ClassProperties),           typeof(PhxDoor))           },
        
        // Right now, there's custom object pooling just for projectiles, meaning, PhxBolt's are not instantiated via
        // PhxScene.CreateInstance(), but with PhxProjectiles.FireProjectile()
        // Maybe this will be obsolete once we've got a generic object pooling for everything. Idk yet.
        { "missile",        new GameBaseClass(typeof(PhxMissileClass),                   null)                      },
        { "sticky",         new GameBaseClass(typeof(PhxMissileClass),                   null)                      },
        { "shell",          new GameBaseClass(typeof(PhxMissileClass),                   null)                      },
        { "beam",           new GameBaseClass(typeof(PhxBeamClass),                      null)                      },
        { "bolt",           new GameBaseClass(typeof(PhxBoltClass),                      null)                      },
        { "bullet",         new GameBaseClass(typeof(PhxBoltClass),                      null)                      },

        { "explosion",      new GameBaseClass(typeof(PhxExplosionClass),                 null)                      },
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
