
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHover : PhxVehicle<PhxHover.PhxHoverProperties>, IPhxTrackable
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeMatch MTC => PhxGameRuntime.GetMatch();
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    static PhxCamera CAM => PhxGameRuntime.GetCamera();


    public class PhxHoverProperties : PhxVehicleProperties
    {
        public PhxProp<float> Acceleration = new PhxProp<float>(5.0f);
        public PhxProp<float> Deceleration = new PhxProp<float>(5.0f);

        public PhxProp<float> ForwardSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> ReverseSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> StrafeSpeed = new PhxProp<float>(5.0f);

        public PhxProp<float> BoostSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> BoostAcceleration = new PhxProp<float>(5.0f);


        public PhxProp<float> SetAltitude = new PhxProp<float>(.5f);

        public PhxProp<float> LiftSpring = new PhxProp<float>(.5f);

        public PhxProp<float> GravityScale = new PhxProp<float>(.5f);

        public PhxProp<float> SpinRate = new PhxProp<float>(1.7f);
        public PhxProp<float> TurnRate = new PhxProp<float>(1.7f);

        public PhxProp<Vector2> PitchLimits = new PhxProp<Vector2>(Vector2.zero);  
        public PhxProp<Vector2> YawLimits = new PhxProp<Vector2>(Vector2.zero); 
    }


    
    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


    private List<PhxAimer> Aimers;

    private List<PhxVehicleTurret> Turrets;


    Rigidbody Body;

    PhxPawnController Controller = null;

    PhxInstance Aim;



    public bool TryEnterVehicle(PhxSoldier soldier, 
                                out string NinePoseAnim, 
                                out Transform PilotPosition)
    {
        // Find first available seat
        GameObject SoldierObj = soldier.gameObject;

        NinePoseAnim = "";
        PilotPosition = null;

        if (false)//Driver == null)
        {
            Controller = Driver.GetController();
            PhxGameRuntime.GetCamera().FollowTrackable(this);

            PilotPosition = transform;

            return true;
        }
        else 
        {
            foreach (var turret in Turrets)
            {
                if (turret.Occupant == null)
                {
                    PilotPosition = turret.PilotPosition;
                    turret.SetOccupant(soldier);
                    PhxGameRuntime.GetCamera().FollowTrackable(turret);

                    return true;
                }
            }
        }

        return false;
    }


    public bool TrySwitchSeat(int index)
    {
        // Get next index

        return true;

        //int nextIndex = ;





    }


    void EjectDriver()
    {
        if (Driver != null)
        {
            PhxGameRuntime.GetCamera().Follow(Driver);
        }

        Driver = null;
        Controller = null;
    }



    public int InitWeapSection(uint[] properties, string[] values, int i, bool print = false)
    {
        PhxAimer CurrAimer = new PhxAimer();

        while (++i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("HierarchyLevel"))
            {
                CurrAimer.HierarchyLevel = 1;
            }
            else if (properties[i] == HashUtils.GetFNV("AimerPitchLimits"))
            {
                CurrAimer.PitchRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerYawLimits"))
            {
                CurrAimer.YawRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("BarrelNodeName"))
            {
                CurrAimer.BarrelNode = UnityUtils.FindChildTransform(transform, values[i]);
            }     
            else if (properties[i] == HashUtils.GetFNV("AimerNodeName"))
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(transform, values[i]);
            }            


            if (properties[i] == HashUtils.GetFNV("WEAPONSECTION") || 
                properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                properties[i] == HashUtils.GetFNV("CHUNKSECTION") || 
                properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                CurrAimer.Init();

                if (Aimers.Count > 0 && Aimers[Aimers.Count - 1].HierarchyLevel > CurrAimer.HierarchyLevel)
                {
                    Aimers[Aimers.Count - 1].ChildAimer = CurrAimer;
                    if (print)
                    {
                        Debug.LogFormat("Added aimer: {0} with barrel: {1} as child of aimer: {2}", CurrAimer.Node.name, CurrAimer.BarrelNode == null ? "null" : CurrAimer.BarrelNode.name, Aimers[Aimers.Count - 1].Node.name);                        
                    }
                }
                else 
                {
                    Aimers.Add(CurrAimer);
                }


                if (properties[i] == HashUtils.GetFNV("NextAimer"))
                {
                    CurrAimer = new PhxAimer();
                }
                else 
                {
                    break;
                }
            }
        }

        return i;
    } 


    Vector3 EyePointOffset;
    Vector3 TrackCenter;
    Vector3 TrackOffset;


    public override void Init()
    {
        BoxCollider TestCollider = gameObject.AddComponent<BoxCollider>();
        TestCollider.size = UnityUtils.GetMaxBounds(gameObject).extents * 2.5f;
        TestCollider.isTrigger = true;

        FillSections(C.Flyer);

        PruneMeshColliders(transform);

        BoxCollider PhysCollider = gameObject.AddComponent<BoxCollider>();
        PhysCollider.size = UnityUtils.GetMaxBounds(gameObject).extents * 2.0f;
        
        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = 1.0f;// C.GravityScale;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        Body.constraints = RigidbodyConstraints.FreezeRotationZ;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;



        //Set up Aimers and Turrets
        Aimers = new List<PhxAimer>();
        Turrets = new List<PhxVehicleTurret>();


        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        int i = 0;
        int TurretIndex = 1;
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("FLYERSECTION"))
            {
                if (values[i] == "BODY")
                {
                    while (i < properties.Length && properties[i] != HashUtils.GetFNV("FLYERSECTION"))
                    {
                        if (properties[i] == HashUtils.GetFNV("EyePointOffset"))
                        {
                            EyePointOffset = PhxUtils.Vec3FromString(values[i]);
                        }
                        else if (properties[i] == HashUtils.GetFNV("TrackCenter"))
                        {
                            TrackCenter = PhxUtils.Vec3FromString(values[i]);
                        }
                        else if (properties[i] == HashUtils.GetFNV("TrackOffset"))
                        {
                            TrackOffset = PhxUtils.Vec3FromString(values[i]);
                        }
                        
                        
                        if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
                        {
                            i = InitWeapSection(properties, values, i, EC.Name == "rep_hover_fightertank");
                        }
                        else 
                        {
                            i++;
                        }
                    }
                }
                else 
                {
                    Turrets.Add(new PhxVehicleTurret(properties, values, i, transform, TurretIndex++));
                }
            }

            i++;
        }
    }


    // Current top-level Aimer
    int CurrAimer = 0;

    Vector3 RestDir = new Vector3(0.0f, 0.5f, 6.0f);
    Vector3 ViewDirection = new Vector3(0.0f, 0.5f, 6.0f);


    float PitchAccum = 0.0f;
    float YawAccum = 0.0f;

    void Update()
    {
        foreach (var turret in Turrets)
        {
            turret.Update();
        }

        if (Controller == null) { return; }

        if (Controller.TryEnterVehicle)
        {
            Driver.SetFree(transform.position + Vector3.up * 2.0f);
            EjectDriver();
            return;
        }


        if (Controller.ShootPrimary)
        {
            if (Aimers.Count > 0)
            {
                Debug.LogFormat("Firing aimer {0}", CurrAimer);
                if (Aimers[CurrAimer].Fire())
                {
                    CurrAimer++;
                }

                if (CurrAimer >= Aimers.Count)
                {
                    CurrAimer = 0;
                }
            }
        }


        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, -8f, 25f);

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, 0f, 0f);        

        ViewDirection = Quaternion.Euler(3f * PitchAccum, 0f, 0f) * TrackOffset;


        Vector3 TargetPos = transform.TransformPoint(ViewDirection);

        if (Physics.Raycast(TargetPos, TargetPos - CAM.transform.position, out RaycastHit hit, 1000f))
        {
            TargetPos = hit.point;

            PhxInstance GetInstance(Transform t)
            {
                PhxInstance inst = t.gameObject.GetComponent<PhxInstance>();
                if (inst == null && t.parent != null)
                {
                    return GetInstance(t.parent);
                }
                return inst;
            }

            Aim = GetInstance(hit.collider.gameObject.transform);
        }


        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos);
            Aimer.UpdateBarrel();
        }
    }



    private Vector3 DecelerateToRest(Vector3 localVelocity)
    {
        if (Mathf.Abs(localVelocity.x) > 0.0f || Mathf.Abs(localVelocity.z) > 0.0f)
        {
            //localVelocity.z = 0.0f;
            //localVelocity.x = 0.0f;
        }

        return localVelocity;
    }





    Vector3 localVel = Vector3.zero;

    void FixedUpdate()
    {
        localVel = transform.worldToLocalMatrix * Body.velocity;

        // Maintain hover
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit))
        {
            if (hit.distance < C.SetAltitude)
            {
                Body.AddForce(Vector3.up * 9.8f, ForceMode.Acceleration);
            }
        }


        if (Controller == null)
        {
            DecelerateToRest(localVel);
            return;
        }


        float rotRate = Vector3.Magnitude(localVel) < .1f ? C.SpinRate : C.TurnRate;

        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, 3f * rotRate * Controller.mouseX, 0f) * Time.fixedDeltaTime);
        Body.MoveRotation(Body.rotation * deltaRotation);



        float strafe = Controller.MoveDirection.x;
        float drive = Controller.MoveDirection.y;

        float forwardForce, strafeForce;

        // If moving in opposite direction of current vel...
        if (localVel.z > 0.0f && drive < 0.0f)
        {
            forwardForce = drive * C.Deceleration;
        }
        else
        {
            forwardForce = drive * C.Acceleration;
        }

        // ''
        if (localVel.x - strafe > localVel.x)
        {
            strafeForce = strafe * C.Deceleration;
        }
        else 
        {
            strafeForce = strafe * C.Acceleration;
        }

        Body.AddRelativeForce(new Vector3(strafeForce, 0.0f, forwardForce), ForceMode.Acceleration);


        // Will scrap soon, keeping for now
        localVel.x = Mathf.Clamp(localVel.x, -C.StrafeSpeed, C.StrafeSpeed);
        localVel.z = Mathf.Clamp(localVel.z, -C.ReverseSpeed, C.ForwardSpeed);

        Body.velocity = transform.localToWorldMatrix * localVel;
    }


    public Vector3 GetCameraPosition()
    {
        return transform.TransformPoint(EyePointOffset + TrackCenter);
    }

    public Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(transform.TransformDirection(ViewDirection), Vector3.up);
    }




    public override void BindEvents(){}
    public override void Fixate(){}
    public override IPhxWeapon GetPrimaryWeapon(){ return null; }
    public void AddAmmo(float amount){}
    public override void PlayIntroAnim(){}
    public override PhxInstance GetAim(){ return Aim; }
    void StateFinished(int layer){}
    public void AddHealth(float amount){}
}
