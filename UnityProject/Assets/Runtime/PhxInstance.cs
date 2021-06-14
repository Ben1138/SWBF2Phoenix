using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

using LibSWBF2.Wrappers;

public abstract class PhxInstance : MonoBehaviour
{
    public PhxPropertyDB P { get; private set; } = new PhxPropertyDB();

    // Every SWBF2 object has a Team
    public PhxProp<int> Team = new PhxProp<int>(0);


    // Use this as constructor (MonoBehaviour constructors don't get called, and
    // Awake() won't be called until next frame)
    public abstract void Init();

    // Override this method in inheriting instance classes.
    // In this method, bind your custom property events and implement
    // the intended behaviour (e.g. Team.OnValueChanged)
    public abstract void BindEvents();


    public virtual void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties)
    {
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

    public abstract void Tick(float deltaTime);
    public abstract void TickPhysics(float deltaTime);
}

public abstract class PhxInstance<T> : PhxInstance where T : PhxClass
{
    public bool IsInit => C != null;
    public T C { get; private set; } = null;


    public override void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties)
    {
        Debug.Assert(classProperties is T);
        C = (T)classProperties;

        base.InitInstance(instOrClass, null);
    }
}

public interface IPhxControlableInstance
{
    public PhxInstance GetInstance();
    public Vector2 GetViewConstraint();
    public Vector2 GetMaxTurnSpeed();

    PhxPawnController GetController();
    void Assign(PhxPawnController controller);
    void UnAssign();

    // Makes this instance immovable
    // Useful for testing and character selection
    public void Fixate();

    public void PlayIntroAnim();

    IPhxWeapon GetPrimaryWeapon();
}

public abstract class PhxControlableInstance<T> : PhxInstance<T>, IPhxControlableInstance where T : PhxClass
{
    protected PhxPawnController Controller;
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

    public PhxPawnController GetController()
    {
        return Controller;
    }

    public void Assign(PhxPawnController controller)
    {
        Controller = controller;
        controller.SetPawn(this);
    }

    public void UnAssign()
    {
        if (Controller != null)
        {
            Controller.RemovePawn();
            Controller = null;
        }
    }

    public abstract void Fixate();
    public abstract void PlayIntroAnim();
    public abstract IPhxWeapon GetPrimaryWeapon();
}

public interface IPhxWeapon
{
    public PhxInstance GetInstance();
    public bool Fire(PhxPawnController owner, Vector3 targetPos);

    public void Reload();
    public void OnShot(Action callback);
    public void OnReload(Action callback);
    public string GetAnimBankName();

    public void SetFirePoint(Transform FirePoint);

    public Transform GetFirePoint();
    public void GetFirePoint(out Vector3 Pos, out Quaternion Rot);


    public void SetIgnoredColliders(List<Collider> Colliders);
    public List<Collider> GetIgnoredColliders();

    public PhxPawnController GetOwnerController();
    public bool IsFiring();




    public int GetMagazineSize();
    public int GetTotalAmmo();
    public int GetMagazineAmmo();
    public int GetAvailableAmmo();
    public float GetReloadTime();
    public float GetReloadProgress();
}