using System;
using System.Reflection;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;

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
            LocalizedName = ENV.GetLocalized(locPath);
        }
        else
        {
            string locPath = "entity.";
            int splitIdx = Name.IndexOf('_');
            locPath += Name.Substring(0, splitIdx) + '.' + Name.Substring(splitIdx + 1);
            LocalizedName = ENV.GetLocalized(locPath);
        }

        Type type = GetType();
        MemberInfo[] members = type.GetMembers();
        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(PhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                PhxPropRef refValue = type.GetField(member.Name).GetValue(this) as PhxPropRef;
                P.Register(member.Name, refValue);
                PhxPropertyDB.AssignProp(ec, member.Name, refValue);
            }
        }

        EntityClass = ec;
    }
}