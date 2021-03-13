using System;
using System.Collections.Generic;
using UnityEngine;

public static class OdfRegister
{
    struct OdfClass
    {
        public Type ClassType;
        public Type InstanceType;

        public OdfClass(Type ct, Type it)
        {
            ClassType = ct;
            InstanceType = it;
        }
    }

    static Dictionary<string, OdfClass> TypeDB = new Dictionary<string, OdfClass>()
    {
        { "commandpost", new OdfClass(typeof(GC_commandpost.ClassProperties), typeof(GC_commandpost)) }
    };

    public static Type GetInstanceType(string name)
    {
        if (TypeDB.TryGetValue(name, out OdfClass odf))
        {
            return odf.InstanceType;
        }
        return null;
    }

    public static Type GetClassType(string name)
    {
        if (TypeDB.TryGetValue(name, out OdfClass odf))
        {
            return odf.ClassType;
        }
        return null;
    }
}
