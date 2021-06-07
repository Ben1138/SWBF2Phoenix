using System;
using System.Reflection;
using UnityEngine;
using LibSWBF2.Wrappers;

public abstract class PhxInstance : MonoBehaviour
{
    public PhxPropertyDB P { get; private set; } = new PhxPropertyDB();

    public abstract void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties);
}

public abstract class PhxInstance<T> : PhxInstance where T : PhxClass
{
    public bool IsInit => C != null;
    public T C { get; private set; } = null;


    // Use this as constructor (MonoBehaviour constructors don't get called, and
    // Awake() won't be called until next frame)
    public abstract void Init();

    // Override this method in inheriting instance classes.
    // In this method, bind your custom property events and implement
    // the intended behaviour (e.g. Team.OnValueChanged)
    public abstract void BindEvents();

    public override void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties)
    {
        C = (T)classProperties;

        Init();

        Type type = GetType();
        MemberInfo[] members = type.GetMembers();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(PhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                PhxPropRef refValue = (PhxPropRef)type.GetField(member.Name).GetValue(this);
                P.Register(member.Name, refValue);
            }
        }

        // make sure the instance is listening on property change events
        // before assigning the actual instance property value
        BindEvents();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(PhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                PhxPropRef refValue = (PhxPropRef)type.GetField(member.Name).GetValue(this);
                PhxPropertyDB.AssignProp(instOrClass, member.Name, refValue);
            }
        }
    }
}

public interface IPhxControlableInstance
{
    public PhxInstance GetInstance();
    public Vector2 GetViewConstraint();
    public Vector2 GetMaxTurnSpeed();
    public Vector3 GetTargetPosition();
}

public abstract class PhxControlableInstance<T> : PhxInstance<T>, IPhxControlableInstance where T : PhxClass
{
    public PhxPawnController Controller;
    protected Vector3 TargetPos;
    protected Vector2 ViewConstraint = Vector2.positiveInfinity;

    // degrees per second
    protected Vector2 MaxTurnSpeed = Vector2.positiveInfinity;


    public PhxInstance GetInstance()
    {
        return this;
    }

    public Vector2 GetViewConstraint()
    {
        return ViewConstraint;
    }

    public Vector2 GetMaxTurnSpeed()
    {
        return MaxTurnSpeed;
    }

    public Vector3 GetTargetPosition()
    {
        return TargetPos;
    }
}