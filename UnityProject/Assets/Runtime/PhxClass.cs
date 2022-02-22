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
    static PhxEnvironment ENV => PhxGame.GetEnvironment();

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
            if (splitIdx >= 0)
            {
                locPath += Name.Substring(0, splitIdx) + '.' + Name.Substring(splitIdx + 1);
                LocalizedName = ENV?.GetLocalized(locPath);
            }
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
                    uint[] propHashes = new uint[0];
                    string[] propValues = new string[0];
                    try {
                        ec.GetAllProperties(out propHashes, out propValues);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Failed to get all props from class: {0} Exception: {1}", ec.Name, e.ToString());
                    }

                    Debug.Assert(propHashes.Length == propValues.Length);

                    var foundSections = new List<Dictionary<string, IPhxPropRef>>();
                    Dictionary<string, IPhxPropRef> currSection = null;

                    HashSet<int> parsedIndices = new HashSet<int>();
                    int currentSectionIdx = -1;

                    for (int i = 0; i < propHashes.Length; ++i)
                    {
                        // Every time we encounter the section header, start a new section
                        if (propHashes[i] == section.NameHash)
                        {
                            foundSections.Add(new Dictionary<string, IPhxPropRef>());
                            currSection = foundSections[foundSections.Count - 1];
                            if (!int.TryParse(propValues[i], out currentSectionIdx))
                            {
                                currentSectionIdx = PhxUtils.IntFromStringEnd(propValues[i], out _);
                            }
                        }
                        else if (section.ContainsProperty(propHashes[i], out _, out int sectionIdx) && sectionIdx > 0 &&  sectionIdx != currentSectionIdx)
                        {
                            foundSections.Add(new Dictionary<string, IPhxPropRef>());
                            currSection = foundSections[foundSections.Count - 1];
                            currentSectionIdx = sectionIdx;
                        }

                        // if we encounter a matching property, grab it
                        if (currSection != null && section.ContainsProperty(propHashes[i], out int propIdx, out _))
                        {
                            string propName = section.Properties[propIdx].Item1;

                            IPhxPropRef prop = section.Properties[propIdx].Item2.ShallowCopy();
                            prop.SetFromString(propValues[i]);

                            if (currSection.ContainsKey(propName))
                            {
                                Debug.LogErrorFormat("Section already contains key: {0} value: {3} (in PhxClass: {1}, Section index: {2})", propName, ec.Name, foundSections.Count - 1, propValues[i]);
                            }
                            else
                            {
                                currSection.Add(propName, prop);
                            }

                            break;
                        }
                    }

                    section.SetSections(foundSections.ToArray());
                }
                else if (typeof(PhxImpliedSection).IsAssignableFrom(type.GetField(member.Name).FieldType))
                {
                    PhxImpliedSection section = type.GetField(member.Name).GetValue(this) as PhxImpliedSection;

                    // Read properties from top to bottom to fill property sections
                    uint[] propHashes = new uint[0];
                    string[] propValues = new string[0];
                    try {
                        ec.GetAllProperties(out propHashes, out propValues);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Failed to get all props from class: {0} Exception: {1}", ec.Name, e.ToString());
                    }

                    Debug.Assert(propHashes.Length == propValues.Length);

                    var foundSections = new List<Dictionary<string, IPhxPropRef>>();
                    Dictionary<string, IPhxPropRef> currSection = null;

                    for (int i = 0; i < propHashes.Length; ++i)
                    {
                        // Every time we encounter the section header, start a new section
                        if (propHashes[i] == section.NameHash)
                        {
                            foundSections.Add(section.GetDefault());
                            currSection = foundSections[foundSections.Count - 1];
                        }

                        //string propNameOut = null;
                        if (currSection != null && section.HasProperty(propHashes[i], out string propNameOut))
                        {
                            currSection[propNameOut].SetFromString(propValues[i]);
                        }
                    }

                    section.SetSections(foundSections.ToArray());
                }
            }
        }

        EntityClass = ec;
    }
}