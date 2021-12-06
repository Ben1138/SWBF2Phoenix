using System.Collections.Generic;
using UnityEngine;

public class PhxPowerupstation : PhxInstance<PhxPowerupstation.ClassProperties>
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<Texture2D> MapTexture          = new PhxProp<Texture2D>(null);
        public PhxProp<float>     MapScale            = new PhxProp<float>(1f);
        public PhxProp<float>     MaxHealth           = new PhxProp<float>(1f);
        public PhxProp<float>     PowerupDelay        = new PhxProp<float>(1f);
        public PhxProp<float>     SoldierAmmo         = new PhxProp<float>(0f);
        public PhxProp<float>     SoldierHealth       = new PhxProp<float>(0f);
        public PhxProp<float>     IdleRotateSpeed     = new PhxProp<float>(0f);
        public PhxProp<float>     IdleWaitTime        = new PhxProp<float>(0f);
        public PhxMultiProp       ActiveRotateNode    = new PhxMultiProp(typeof(string));
        public PhxProp<string>    ActiveSpinNode      = new PhxProp<string>(null);
        public PhxProp<string>    IdleWobbleNode      = new PhxProp<string>(null);
        public PhxProp<float>     IdleWobbleFactor    = new PhxProp<float>(0f);
        public PhxProp<string>    IdleWobbleLeftFoot  = new PhxProp<string>(null);
        public PhxProp<string>    IdleWobbleRightFoot = new PhxProp<string>(null);
    }

    struct RotTarget
    {
        public bool IsDone { get; private set; }

        Quaternion Origin;
        Quaternion Target;
        float LerpPerSec;
        float CurrentLerp;

        public RotTarget(Quaternion origin, Quaternion target, float anglesPerSec)
        {
            Origin = origin;
            Target = target;
            float angleDiff = Quaternion.Angle(origin, target);
            IsDone = angleDiff == 0.0f;
            LerpPerSec = anglesPerSec / angleDiff;
            CurrentLerp = 0.0f;
        }

        public Quaternion Step(float deltaTime)
        {
            CurrentLerp += LerpPerSec * deltaTime;
            IsDone = CurrentLerp >= 1.0f;
            return Quaternion.Slerp(Origin, Target, CurrentLerp);
        }
    }

    class RotateNode
    {
        public float AnglesPerSec;
        Transform Node;
        RotTarget Target;

        public RotateNode(Transform node, Vector3 minDegr, Vector3 maxDegr, float anglesPerSec)
        {
            Node = node;
            AnglesPerSec = anglesPerSec;
            SetRandomTarget(minDegr, maxDegr);
        }

        public void SetRandomTarget(Vector3 minDegr, Vector3 maxDegr)
        {
            Target = new RotTarget(
                Node.localRotation,
                Quaternion.Euler(
                    minDegr.x == 0f && maxDegr.x == 0f ? Node.localRotation.eulerAngles.x : Random.Range(minDegr.x, maxDegr.x),
                    minDegr.y == 0f && maxDegr.y == 0f ? Node.localRotation.eulerAngles.y : Random.Range(minDegr.y, maxDegr.y),
                    minDegr.z == 0f && maxDegr.z == 0f ? Node.localRotation.eulerAngles.z : Random.Range(minDegr.z, maxDegr.z)
                ),
                AnglesPerSec
            );
        }

        public bool Step(float deltaTime)
        {
            Node.localRotation = Target.Step(deltaTime);
            return Target.IsDone;
        }
    }

    public PhxProp<PhxRegion> EffectRegion = new PhxProp<PhxRegion>(null);
    public PhxProp<float>  Radius = new PhxProp<float>(1.0f);

    float IdleWaitTimer = -1.0f;
    RotateNode RootRotation;
    RotateNode ActiveSpinNode;
    RotateNode[] ActiveRotateNodes;
    Transform IdleWobbleNode;
    Transform IdleWobbleLeftFoot;
    Transform IdleWobbleRightFoot;
    PhxRegion PowerupRegion;
    float IdleWobble = 0.0f;        // 0f => Idle, 1f => Wobble
    HashSet<PhxSoldier> Soldiers = new HashSet<PhxSoldier>();
    float PowerupTimer;

    public override void Init()
    {
        RootRotation   = CreateRotationNode("dummyroot", new Vector3(0f, 0f, 0f), new Vector3(0f, 360f, 0f), 20f * C.IdleRotateSpeed);
        ActiveSpinNode = CreateRotationNode(C.ActiveSpinNode, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 360f), 360f);

        IdleWobbleNode = FindChildRecursive(C.IdleWobbleNode);
        IdleWobbleLeftFoot = FindChildRecursive(C.IdleWobbleLeftFoot);
        IdleWobbleRightFoot = FindChildRecursive(C.IdleWobbleRightFoot);

        ActiveRotateNodes = new RotateNode[C.ActiveRotateNode.Values.Count];
        for (int i = 0; i < ActiveRotateNodes.Length; ++i)
        {
            string nodeName = C.ActiveRotateNode.Values[i][0] as string;
            ActiveRotateNodes[i] = CreateRotationNode(nodeName, new Vector3(140f, 0f, 0f), new Vector3(220f, 0f, 0f), 100f);
        }
    }

    public override void Destroy()
    {
        
    }

    public override void BindEvents()
    {
        C.IdleRotateSpeed.OnValueChanged += (float _) => RootRotation.AnglesPerSec = C.IdleRotateSpeed;

        EffectRegion.OnValueChanged += (PhxRegion _) => UpdateEffectRegion();
        Radius.OnValueChanged       += (float _)  => UpdateEffectRegion();

        // TODO: implement behaviour of remaining value changes
    }

    void UpdateEffectRegion()
    {
        if (PowerupRegion != null)
        {
            PowerupRegion.OnEnter -= OnEffectRegionEnter;
        }
        if (PowerupRegion != null)
        {
            PowerupRegion.OnLeave -= OnEffectRegionLeave;
        }

        // TODO: remove previous sphere + region component

        PowerupRegion = EffectRegion;
        if (PowerupRegion == null)
        {
            SphereCollider coll = gameObject.AddComponent<SphereCollider>();
            coll.radius = Radius;
            coll.isTrigger = true;
            PowerupRegion = gameObject.AddComponent<PhxRegion>();
        }

        PowerupRegion.OnEnter += OnEffectRegionEnter;
        PowerupRegion.OnLeave += OnEffectRegionLeave;
    }

    void OnEffectRegionEnter(IPhxControlableInstance other)
    {
        PhxSoldier soldier = other as PhxSoldier;
        if (soldier != null)
        {
            Soldiers.Add(soldier);
        }
    }

    void OnEffectRegionLeave(IPhxControlableInstance other)
    {
        PhxSoldier soldier = other as PhxSoldier;
        if (soldier != null)
        {
            Soldiers.Remove(soldier);
        }
    }

    RotateNode CreateRotationNode(string nodeName, Vector3 minDegr, Vector3 maxDegr, float anglesPerSec)
    {
        if (string.IsNullOrEmpty(nodeName)) return null;

        nodeName = nodeName.ToLowerInvariant();
        Transform node = FindChildRecursive(nodeName);
        if (node != null)
        {
            return new RotateNode(node, minDegr, maxDegr, anglesPerSec);
        }
        Debug.LogWarning($"Cannot find '{nodeName}'!");
        return null;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsInit) return;

        // -----------------------------------------------------------------------------------
        // Animation
        // -----------------------------------------------------------------------------------
        float wobble = Mathf.Sin(Time.realtimeSinceStartup * 4f);
        if (RootRotation != null)
        {
            if (IdleWaitTimer < 0.0f)
            {
                IdleWobble = Mathf.Min(IdleWobble + deltaTime, 1f);
                if (RootRotation.Step(deltaTime))
                {
                    IdleWaitTimer = 0.0f;
                }
            }
            else
            {
                IdleWobble = Mathf.Max(IdleWobble - deltaTime, 0f);
                IdleWaitTimer += deltaTime;
                if (IdleWaitTimer >= C.IdleWaitTime)
                {
                    RootRotation.SetRandomTarget(new Vector3(0f, 0f, 0f), new Vector3(0f, 360f, 0f));
                    IdleWaitTimer = -1.0f;
                }
            }

            if (IdleWobbleNode != null)
            {
                float range = C.IdleWobbleFactor * 45f;

                IdleWobbleNode.localRotation = Quaternion.Euler(
                    IdleWobbleNode.localRotation.eulerAngles.x,
                    IdleWobbleNode.localRotation.eulerAngles.y,
                    wobble * range * IdleWobble
                );

                if (IdleWobbleLeftFoot != null)
                {
                    IdleWobbleLeftFoot.localPosition = new Vector3(
                        IdleWobbleLeftFoot.localPosition.x,
                        (-wobble * 0.02f + 0.02f) * IdleWobble,
                        IdleWobbleLeftFoot.localPosition.z
                    );
                }

                if (IdleWobbleRightFoot != null)
                {
                    IdleWobbleRightFoot.localPosition = new Vector3(
                        IdleWobbleRightFoot.localPosition.x,
                        (wobble * 0.02f + 0.02f) * IdleWobble,
                        IdleWobbleRightFoot.localPosition.z
                    );
                }
            }
        }

        if (ActiveSpinNode != null)
        {
            if (ActiveSpinNode.Step(deltaTime))
            {
                ActiveSpinNode.SetRandomTarget(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 360f));
            }
        }

        for (int i = 0; i < ActiveRotateNodes.Length; ++i)
        {
            if (ActiveRotateNodes[i] == null)
            {
                continue;
            }

            if (ActiveRotateNodes[i].Step(deltaTime))
            {
                ActiveRotateNodes[i].SetRandomTarget(new Vector3(140f, 0f, 0f), new Vector3(220f, 0f, 0f));
            }
        }

        // -----------------------------------------------------------------------------------
        // Ammo / Healing
        // -----------------------------------------------------------------------------------

        PowerupTimer += deltaTime;
        if (PowerupTimer >= 1.0f)
        {
            foreach (PhxSoldier soldier in Soldiers)
            {
                soldier.AddHealth(C.SoldierHealth);
                soldier.AddAmmo(C.SoldierAmmo);
            }
            PowerupTimer = 0f;
        }
    }

    public override void TickPhysics(float deltaTime)
    {

    }

    Transform FindChildRecursive(string name)
    {
        return FindChildRecursive(transform, name);
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
            child = FindChildRecursive(child, name);
            if (child != null)
            {
                return child;
            }
        }
        return null;
    }
}
