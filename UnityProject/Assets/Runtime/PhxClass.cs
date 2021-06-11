using System;
using System.Reflection;
using System.Collections.Generic;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;
using LibSWBF2.Utils;
using UnityEngine;

/// <summary>
/// Represents an ODF class. Needs specific implementation
/// </summary>
public abstract class PhxClass
{
    static PhxRuntimeEnvironment ENV => PhxGameRuntime.GetEnvironment();

    public PhxPropertyDB P { get; private set; } = new PhxPropertyDB();
    public string Name { get; private set; }
    public EEntityClassType ClassType { get; private set; }
    public string LocalizedName { get; private set; }
    public EntityClass EntityClass { get; private set; }

    public void InitClass(EntityClass ec)
    {
        Name = ec.Name;
        ClassType = ec.ClassType;

        if (ClassType == EEntityClassType.WeaponClass)
        {
            string locPath = "weapons.";
            int splitIdx = Name.IndexOf('_');
            locPath += Name.Substring(0, splitIdx) + ".weap." + Name.Substring(splitIdx + 1).Replace("weap_", "");
            LocalizedName = ENV?.GetLocalized(locPath);
        }
        else
        {
            string locPath = "entity.";
            int splitIdx = Name.IndexOf('_');
            locPath += Name.Substring(0, splitIdx) + '.' + Name.Substring(splitIdx + 1);
            LocalizedName = ENV?.GetLocalized(locPath);
        }

        Type type = GetType();
        MemberInfo[] members = type.GetMembers();
        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                if (typeof(IPhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
                {
                    IPhxPropRef refValue = type.GetField(member.Name).GetValue(this) as IPhxPropRef;
                    P.Register(member.Name, refValue);
                    PhxPropertyDB.AssignProp(ec, member.Name, refValue);
                }
                else if (typeof(PhxPropertySection).IsAssignableFrom(type.GetField(member.Name).FieldType))
                {
                    PhxPropertySection section = type.GetField(member.Name).GetValue(this) as PhxPropertySection;

                    // Read properties from top to bottom to fill property sections
                    ec.GetAllProperties(out uint[] propHashes, out string[] propValues);
                    Debug.Assert(propHashes.Length == propValues.Length);

                    var foundSections = new List<Dictionary<string, IPhxPropRef>>();
                    Dictionary<string, IPhxPropRef> currSection = null;

                    for (int i = 0; i < propHashes.Length; ++i)
                    {
                        // Every time we encounter the section header, start a new section
                        if (propHashes[i] == section.NameHash)
                        {
                            foundSections.Add(new Dictionary<string, IPhxPropRef>());
                            currSection = foundSections[foundSections.Count - 1];
                        }

                        if (currSection != null)
                        {
                            for (int j = 0; j < section.Properties.Length; ++j)
                            {
                                string propName = section.Properties[j].Item1;
                                uint propNameHash = HashUtils.GetFNV(propName);

                                // if we encounter a matching property, grab it
                                if (propHashes[i] == propNameHash)
                                {                 
                                    IPhxPropRef prop = section.Properties[j].Item2.ShallowCopy();
                                    prop.SetFromString(propValues[i]);
                                    currSection.Add(propName, prop);
                                }
                            }
                        }
                    }

                    section.SetSections(foundSections.ToArray());
                }
            }
        }

        EntityClass = ec;
    }
}