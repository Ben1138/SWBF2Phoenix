using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using UnityEditor.ShaderGraph.Internal;

public class GC_powerupstation : ISWBFInstance<GC_powerupstation.ClassProperties>
{
    public class ClassProperties : ISWBFClass
    {
        public Prop<Texture2D> MapTexture          = new Prop<Texture2D>(null);
        public Prop<float>     MapScale            = new Prop<float>(1.0f);
        public Prop<float>     MaxHealth           = new Prop<float>(1.0f);
        public Prop<float>     PowerupDelay        = new Prop<float>(1.0f);
        public Prop<float>     SoldierAmmo         = new Prop<float>(0.0f);
        public Prop<float>     SoldierHealth       = new Prop<float>(0.0f);
        public Prop<float>     IdleRotateSpeed     = new Prop<float>(0.0f);
        public Prop<float>     IdleWaitTime        = new Prop<float>(0.0f);
        public MultiProp       ActiveRotateNode    = new MultiProp(typeof(string));
        public Prop<string>    ActiveSpinNode      = new Prop<string>(null);
        public Prop<string>    IdleWobbleNode      = new Prop<string>(null);
        public Prop<float>     IdleWobbleFactor    = new Prop<float>(0.0f);
        public Prop<string>    IdleWobbleLeftFoot  = new Prop<string>(null);
        public Prop<string>    IdleWobbleRightFoot = new Prop<string>(null);
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

    public Prop<Region> EffectRegion = new Prop<Region>(null);
    public Prop<float>  Radius = new Prop<float>(1.0f);

    float IdleWaitTimer = -1.0f;
    RotateNode RootRotation;
    RotateNode ActiveSpinNode;
    RotateNode[] ActiveRotateNodes;
    Transform IdleWobbleNode;
    Transform IdleWobbleLeftFoot;
    Transform IdleWobbleRightFoot;
    Region PowerupRegion;
    float IdleWobble = 0.0f;        // 0f => Idle, 1f => Wobble
    HashSet<GC_soldier> Soldiers = new HashSet<GC_soldier>();
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

    public override void BindEvents()
    {
        C.IdleRotateSpeed.OnValueChanged += (float _) => RootRotation.AnglesPerSec = C.IdleRotateSpeed;

        EffectRegion.OnValueChanged += (Region _) => UpdateEffectRegion();
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
            PowerupRegion = gameObject.AddComponent<Region>();
        }

        PowerupRegion.OnEnter += OnEffectRegionEnter;
        PowerupRegion.OnLeave += OnEffectRegionLeave;
    }

    void OnEffectRegionEnter(ISWBFInstance other)
    {
        GC_soldier soldier = other as GC_soldier;
        if (soldier != null)
        {
            Soldiers.Add(soldier);
        }
    }

    void OnEffectRegionLeave(ISWBFInstance other)
    {
        GC_soldier soldier = other as GC_soldier;
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

    void Update()
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
                IdleWobble = Mathf.Min(IdleWobble + Time.deltaTime, 1f);
                if (RootRotation.Step(Time.deltaTime))
                {
                    IdleWaitTimer = 0.0f;
                }
            }
            else
            {
                IdleWobble = Mathf.Max(IdleWobble - Time.deltaTime, 0f);
                IdleWaitTimer += Time.deltaTime;
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
            if (ActiveSpinNode.Step(Time.deltaTime))
            {
                ActiveSpinNode.SetRandomTarget(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 360f));
            }
        }

        for (int i = 0; i < ActiveRotateNodes.Length; ++i)
        {
            if (ActiveRotateNodes[i] == null) continue;

            if (ActiveRotateNodes[i].Step(Time.deltaTime))
            {
                ActiveRotateNodes[i].SetRandomTarget(new Vector3(140f, 0f, 0f), new Vector3(220f, 0f, 0f));
            }
        }

        // -----------------------------------------------------------------------------------
        // Ammo / Healing
        // -----------------------------------------------------------------------------------

        PowerupTimer += Time.deltaTime;
        if (PowerupTimer >= 1.0f)
        {
            foreach (GC_soldier soldier in Soldiers)
            {
                soldier.AddHealth(C.SoldierHealth);
                soldier.AddAmmo(C.SoldierAmmo);
            }
            PowerupTimer = 0f;
        }
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
