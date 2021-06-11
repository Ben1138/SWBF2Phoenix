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
            if (member.MemberType == MemberTypes.Field && typeof(IPhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                IPhxPropRef refValue = (IPhxPropRef)type.GetField(member.Name).GetValue(this);
                P.Register(member.Name, refValue);
            }
        }

        // make sure the instance is listening on property change events
        // before assigning the actual instance property value
        BindEvents();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(IPhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                IPhxPropRef refValue = (IPhxPropRef)type.GetField(member.Name).GetValue(this);
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

    PhxPawnController GetController();
    void Assign(PhxPawnController controller);

    // Makes this instance immovable
    // Useful for testing and character selection
    public void Fixate();

    public void PlayIntroAnim();
}

public abstract class PhxControlableInstance<T> : PhxInstance<T>, IPhxControlableInstance where T : PhxClass
{
    protected PhxPawnController Controller;
    
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

    public PhxPawnController GetController()
    {
        return Controller;
    }

    public void Assign(PhxPawnController controller)
    {
        Controller = controller;
        controller.SetPawn(this);
    }

    public abstract void Fixate();
    public abstract void PlayIntroAnim();
}

public interface IPhxWeapon
{
    public PhxInstance GetInstance();
    public void Fire();
    public void OnShot(Action callback);
    public string GetAnimBankName();
}