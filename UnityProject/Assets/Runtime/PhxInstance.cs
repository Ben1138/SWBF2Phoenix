using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

using LibSWBF2.Wrappers;

// Bare minimum requirements
public interface IPhxInstantiable
{
    // Use this as constructor
    // (MonoBehaviour constructors don't get called, and Awake() won't be called until next frame)
    //
    // Use this to add Components like Rigidbody, etc.
    // and bind property change events.
    // Will be called BEFORE instance properties assignments!
    public void Init();

    // Use this as destructor
    public void Destroy();
}

public interface IPhxTickable
{
    public void Tick(float deltaTime);
}

public interface IPhxTickablePhysics
{
    public void TickPhysics(float deltaTime);
}

// Use this whenever we're dealing with a Unity Component attached to a GameObject in general.
// From here on, instances of all inheriting classes are poolable.
public abstract class PhxComponent : MonoBehaviour, IPhxInstantiable
{
    public PhxPool ParentPool;

    public abstract void Init();
    public abstract void Destroy();
}

// Use this when we're dealing with instances that contain reflected properties that are exposed to Lua
public abstract class PhxInstance : PhxComponent
{
    public PhxPropertyDB P { get; private set; } = new PhxPropertyDB();

    // Every SWBF2 object has a Team
    public PhxProp<int> Team = new PhxProp<int>(0);


    public virtual void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties)
    {
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

        // Call before property assignments, such that we can react to their initialization
        Init();

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
    public PhxAnimWeapon GetAnimInfo();

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