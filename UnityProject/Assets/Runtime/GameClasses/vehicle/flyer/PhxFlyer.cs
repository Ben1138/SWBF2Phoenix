
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

using LibSWBF2.Enums;

public class PhxFlyer : PhxVehicle
{
    public class ClassProperties : PhxVehicleProperties
    {
        public PhxProp<float> Acceleraton = new PhxProp<float>(5.0f);

        public PhxProp<float> MinSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> MidSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> MaxSpeed = new PhxProp<float>(5.0f);

        public PhxProp<float> BoostSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> BoostAcceleraton = new PhxProp<float>(5.0f);

        public PhxProp<float> GravityScale = new PhxProp<float>(.5f);


        public PhxProp<float> BankAngle = new PhxProp<float>(1.7f);

        public PhxProp<float> RollRate = new PhxProp<float>(1.7f);
        public PhxProp<float> PitchRate = new PhxProp<float>(1.7f);
        public PhxProp<float> PitchFilter = new PhxProp<float>(1.7f);
        public PhxProp<float> PitchFilterDecel = new PhxProp<float>(1.7f);
        public PhxProp<float> PitchBuildupMultiplier = new PhxProp<float>(1.7f);
        
        public PhxProp<float> TurnRate = new PhxProp<float>(1.7f);
        public PhxProp<float> TurnFilter = new PhxProp<float>(1.7f);
        public PhxProp<float> TurnFilterDecel = new PhxProp<float>(1.7f);
        public PhxProp<float> TurnBuildupMultiplier = new PhxProp<float>(1.7f);

        /*
        Cases:
            If distance > time * speed, then distance must be reached first...

        */


        // Time flyer starts flying after TakeoffHeight reached?
        public PhxProp<float> TakeoffTime = new PhxProp<float>(1.7f);
        
        // Nothing to do with anim speed, just speed of rise
        public PhxProp<float> TakeoffSpeed = new PhxProp<float>(1.7f);
        
        // Height flyer rises to before playing takeoff anim?
        public PhxProp<float> TakeoffHeight = new PhxProp<float>(0f);

        public PhxProp<float> LandingTime = new PhxProp<float>(1.7f);
        public PhxProp<float> LandingSpeed = new PhxProp<float>(1.7f);
        
        // Spawn height as well ofc, if not set then the model's BBOX is used?
        // Is this added to the BBOX if set?
        public PhxProp<float> LandedHeight = new PhxProp<float>(0f);


        public PhxProp<AudioClip> EngineSound = new PhxProp<AudioClip>(null);


        public PhxImpliedSection ContrailEffects = new PhxImpliedSection(
            ("ContrailEffect", new PhxProp<string>("")),
            ("ContrailAttachPoint", new PhxProp<string>("")),
            ("ContrailAttachOffset", new PhxProp<Vector3>(Vector3.zero)),
            ("ContrailEffectMinSpeed", new PhxProp<float>(1f)),
            ("ContrailEffectMinScale", new PhxProp<float>(1f)),
            ("ContrailEffectMaxScale", new PhxProp<float>(0f))
        );

        public PhxImpliedSection ThrustEffects = new PhxImpliedSection(
            ("ThrustEffect", new PhxProp<string>("")),
            ("ThrustAttachPoint", new PhxProp<string>("")),
            ("ThrustAttachOffset", new PhxProp<Vector3>(Vector3.zero)),
            ("ThrustEffectMinScale", new PhxProp<float>(1f)),
            ("ThrustEffectMaxScale", new PhxProp<float>(1f)),
            ("ThrustEffectScaleStart", new PhxProp<float>(0f))
        );
    }


    Rigidbody Body;

    // Will change to driver section
    PhxFlyerMainSection DriverSeat = null;
    PhxFlyer.ClassProperties F;


    // Poser for movement anims
    PhxPoser Poser;

    CraPlayer TakeOffPlayer;


    AudioSource AudioAmbient;


    List<PhxEffect> ContrailEffects;
    List<PhxEffect> ThrustEffects;


    public enum PhxFlyerState : int 
    {
        Grounded,
        Landing,
        TakingOff,
        Flying,
        Tricking,
        Dying
    }

    PhxFlyerState CurrentState = PhxFlyerState.Grounded;


    public string ClassName;


    float RealLandedHeight;

    public override void Init()
    {
        base.Init();

        ClassName = C.EntityClass.Name;

        CurHealth.Set(C.MaxHealth.Get());

        F = C as PhxFlyer.ClassProperties;
        if (F == null) return;

        RealLandedHeight = F.LandedHeight - (UnityUtils.GetMaxBounds(gameObject).min.y - transform.position.y);
        transform.position += Vector3.up * RealLandedHeight;

        ModelMapping.ConvexifyMeshColliders(false);
        ModelMapping.SetMeshCollider(true);


        /*
        RIGIDBODY
        */

        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = F.GravityScale * 10f;
        Body.useGravity = true;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        //Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Body.isKinematic = true;

        // These get calculated automatically when adding colliders/children IF 
        // they are not set manually beforehand!!
        Body.centerOfMass = Vector3.zero;
        Body.inertiaTensor = new Vector3(1f,1f,1f);
        Body.inertiaTensorRotation = Quaternion.identity;


        // Will expand once sound loading is fixed
        AudioAmbient = gameObject.AddComponent<AudioSource>();
        AudioAmbient.spatialBlend = 1.0f;
        AudioAmbient.clip = F.EngineSound;
        AudioAmbient.pitch = 1.0f;
        AudioAmbient.volume = 0.5f;
        AudioAmbient.rolloffMode = AudioRolloffMode.Linear;
        AudioAmbient.minDistance = 2.0f;
        AudioAmbient.maxDistance = 30.0f;


        /*
        SECTIONS
        */

        Seats = new List<PhxSeat>();

        var EC = F.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        int i = 0;
        int TurretIndex = 1;

        DriverSeat = new PhxFlyerMainSection(this);
        DriverSeat.InitManual(EC, 0, "FLYERSECTION", "BODY");
        Seats.Add(DriverSeat);    

        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("FLYERSECTION"))
            {
                if (!values[i].Equals("BODY", StringComparison.OrdinalIgnoreCase))
                {
                    PhxVehicleTurret Turret = new PhxVehicleTurret(this, TurretIndex++);
                    Turret.InitManual(EC, i, "FLYERSECTION", values[i]);
                    Seats.Add(Turret);
                }
            }

            i++;
        }

        SetIgnoredCollidersOnAllWeapons();


        /*
        CONTRAIL EFFECTS
        */

        ContrailEffects = new List<PhxEffect>();
        foreach (Dictionary<string, IPhxPropRef> Section in F.ContrailEffects)
        {   
            PhxEffect ContrailEffect = SCENE.EffectsManager.LendEffect((Section["ContrailEffect"] as PhxProp<string>).Get());
            Transform ContrailNode = UnityUtils.FindChildTransform(transform, (Section["ContrailAttachPoint"] as PhxProp<string>).Get());
            
            if (ContrailEffect == null || ContrailNode == null) continue;

            ContrailEffect.Stop();

            ContrailEffect.SetParent(ContrailNode);
            ContrailEffect.SetLocalTransform((Section["ContrailAttachOffset"] as PhxProp<Vector3>).Get(), Quaternion.identity);

            ContrailEffects.Add(ContrailEffect);
        }
        
        
        /*
        THRUST EFFECTS
        */

        ThrustEffects = new List<PhxEffect>();
        foreach (Dictionary<string, IPhxPropRef> Section in F.ThrustEffects)
        {   
            PhxEffect ThrustEffect = SCENE.EffectsManager.LendEffect((Section["ThrustEffect"] as PhxProp<string>).Get());
            Transform ThrustNode = UnityUtils.FindChildTransform(transform, (Section["ThrustAttachPoint"] as PhxProp<string>).Get());
            
            if (ThrustEffect == null || ThrustNode == null) continue;

            ThrustEffect.Stop();

            ThrustEffect.SetParent(ThrustNode);
            ThrustEffect.SetLocalTransform((Section["ThrustAttachOffset"] as PhxProp<Vector3>).Get(), Quaternion.identity);

            ThrustEffects.Add(ThrustEffect);
        }



        /*
        POSER
        */

        if (F.AnimationName.Get() != "" && F.FinAnimation.Get() != "")
        {
            Poser = new PhxPoser(F.AnimationName.Get(), F.FinAnimation.Get(), transform);
        }

        /*
        TAKEOFF
        */

        if (F.AnimationName.Get() != "")
        {
            TakeOffPlayer = PhxAnimationLoader.CreatePlayer(transform, F.AnimationName.Get(), "takeoff", false);
            if (TakeOffPlayer.IsValid())
            {
                TakeOffPlayer.SetPlaybackSpeed(1f);
                TakeOffPlayer.SetLooping(false);
            }
        }
    }



    /*
    Update each section, pose if the poser is set, and wheel texture offset if applicable.
    */

    float TakeoffTime, TakeoffTimer;
    float LandTime, LandTimer;

    void UpdateState(float deltaTime)
    {
        /* 
        Seats
        */

        foreach (var section in Seats)
        {
            section.Tick(deltaTime);
        }


        /*
        Driver input
        */

        Vector3 Input = Vector3.zero;
        try {
        DriverController = DriverSeat.GetController();
        }
        catch 
        {
            Debug.Log(F.EntityClass.Name);
        }
        if (DriverController != null) 
        {
            Input.x = DriverController.MoveDirection.x;
            Input.y = DriverController.MoveDirection.y;
            Input.z = DriverController.mouseX;
        }
        else 
        {
            return;
        }

        // TODO: Implement gradual damage when abandoned
        if (CurrentState == PhxFlyerState.Grounded)
        {
            if (DriverController.Jump)
            {
                DriverController.Jump = false;

                TakeoffTime = F.TakeoffTime;  
                TakeoffTimer = TakeoffTime;

                if (TakeOffPlayer.IsValid())
                {
                    TakeOffPlayer.SetPlaybackSpeed(1f);
                    TakeOffPlayer.Play();                    
                }

                CurrentState = PhxFlyerState.TakingOff;

                foreach (PhxEffect ThrustEff in ThrustEffects)
                {
                    ThrustEff.SetLooping(true);
                    ThrustEff.Play();
                } 
            }
        }
        else if (CurrentState == PhxFlyerState.TakingOff)
        {
            TakeoffTimer -= deltaTime;
            if (TakeoffTimer < 0f)
            {
                CurrentState = PhxFlyerState.Flying;
                LocalVel = Vector3.zero;
                //LocalAngVel = Quaternion.identity;
            }
        }
        else if (CurrentState == PhxFlyerState.Landing)
        {
            /*
            LandTimer -= deltaTime;
            if (LandTimer < 0f)
            {
                CurrentState = PhxFlyerState.Grounded;
            }
            */
        }
        else if (CurrentState == PhxFlyerState.Flying)
        {
            if (DriverController.Jump)
            {
                DriverController.Jump = false;
                
                if (TakeOffPlayer.IsValid())
                {
                    TakeOffPlayer.SetPlaybackSpeed(-1f);//TakeOffPlayer.GetDuration() / F.TakeoffTime);
                    TakeOffPlayer.Play(); 

                    foreach (PhxEffect ThrustEff in ThrustEffects)
                    {
                        ThrustEff.Stop();
                    }  
                }

                CurrentState = PhxFlyerState.Landing;
            }
            else 
            {
                /*
                Vehicle pose
                */

                if (Poser != null)
                {
                    float blend = 2f * deltaTime;

                    if (Vector3.Magnitude(Input) < .001f)
                    {
                        Poser.SetState(PhxNinePoseState.Idle, blend);
                    }
                    else 
                    {
                        if (Input.x > .01f)
                        {
                            Poser.SetState(PhxNinePoseState.StrafeRight, blend);           
                        }

                        if (Input.x < -.01f)
                        {
                            Poser.SetState(PhxNinePoseState.StrafeLeft, blend);            
                        }

                        if (Input.y < 0f) 
                        {
                            if (Input.z > .01f)
                            {
                                Poser.SetState(PhxNinePoseState.BackwardsTurnLeft, blend);            
                            }
                            else if (Input.z < -.01f)
                            {
                                Poser.SetState(PhxNinePoseState.BackwardsTurnRight, blend);            
                            }
                            else
                            {
                                Poser.SetState(PhxNinePoseState.Backwards, blend);            
                            }
                        }
                        else
                        {
                            if (Input.z > .01f)
                            {
                                Poser.SetState(PhxNinePoseState.ForwardTurnLeft, blend);            
                            }
                            else if (Input.z < -.01f)
                            {
                                Poser.SetState(PhxNinePoseState.ForwardTurnRight, blend);            
                            }
                            else
                            {
                                Poser.SetState(PhxNinePoseState.Forward, blend);            
                            }
                        }
                    }
                }
            }            
        }
    }



    Vector3 LocalVel = Vector3.zero;
    Vector3 LocalAngVel = Vector3.zero;
    PhxPawnController DriverController;

    LayerMask Mask = (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    void UpdatePhysics(float deltaTime)
    {   
        //LocalVel = Vector3.zero; 

        if (CurrentState == PhxFlyerState.TakingOff)
        {
            //Body.MovePosition(Body.position += Vector3.up * F.TakeoffSpeed * deltaTime);
            LocalVel = transform.InverseTransformDirection(Vector3.up) * F.TakeoffSpeed;
        }
        else if (CurrentState == PhxFlyerState.Landing)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, F.LandingSpeed * deltaTime + RealLandedHeight, Mask, QueryTriggerInteraction.Ignore))
            {
                CurrentState = PhxFlyerState.Grounded;
                //Debug.LogFormat("Landing at distance: {0}", hit.distance);
            }
            else 
            {
                //Body.MovePosition(Body.position += Vector3.down * F.LandingSpeed * deltaTime);
                LocalVel = transform.InverseTransformDirection(Vector3.down) * F.LandingSpeed;
            }
        }
        else if (CurrentState == PhxFlyerState.Flying)
        {
            float Fwd = 0f;

            DriverController = DriverSeat.GetController();
            if (DriverController != null) 
            {
                //Rotation
                //Quaternion turnRotation = Quaternion.Euler(new Vector3(0f,  0f) * deltaTime);
                Quaternion rot = Quaternion.Euler(new Vector3(16f * DriverSeat.PitchRate * DriverController.mouseY, 16f * DriverSeat.TurnRate * DriverController.mouseX, - 16f * F.RollRate * DriverController.MoveDirection.x) * deltaTime);
                Body.MoveRotation(Body.rotation * rot);

                Fwd = DriverController.MoveDirection.y;
            }

            float Speed = LocalVel.z;
            if (Fwd < 0f)
            {
                if (Speed > F.MinSpeed)
                {
                    Speed -= F.Acceleraton * deltaTime;
                }
            }
            else if (Fwd == 0f)
            {
                if (Speed > F.MidSpeed)
                {
                    Speed -= F.Acceleraton * deltaTime;
                }
                else 
                {
                    Speed += F.Acceleraton * deltaTime;
                }
            }
            else 
            {
                if (Speed < F.MaxSpeed)
                {
                    Speed += F.Acceleraton * deltaTime;
                }
            }

            LocalVel = Vector3.forward * Speed;
        }

        if (CurrentState != PhxFlyerState.Grounded)
        {
            Body.MovePosition(Body.position + deltaTime * transform.TransformDirection(LocalVel));            
        }

    }



    public override Vector3 GetCameraPosition()
    {
        return Seats[0].GetCameraPosition();
    }

    public override Quaternion GetCameraRotation()
    {
        return Seats[0].GetCameraRotation();
    }


    public override bool IncrementSlice(out float progress)
    {
        progress = SliceProgress;
        return false;
    }


    void OnCollisionEnter(Collision Coll)
    {
        if (Coll.gameObject.layer != LayerMask.NameToLayer("OrdnanceAll"))
        {
            
        }
    }

    void OnCollisionStay(Collision Coll)
    {
        if (Coll.gameObject.layer != LayerMask.NameToLayer("OrdnanceAll"))
        {
            //Debug.LogFormat("Impulse vec: {0}", Coll.impulse.ToString("F2"));
        }
    }




    public override void Tick(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick Flyer");
        base.Tick(deltaTime);
        UpdateState(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public override void TickPhysics(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick Flyer Physics");
        UpdatePhysics(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
