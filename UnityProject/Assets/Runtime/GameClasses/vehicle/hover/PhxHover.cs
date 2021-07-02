
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHover : PhxVehicle<PhxHover.PhxHoverProperties>
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

        //public PhxProp<Vector3> 
    }


    
    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


    private List<PhxAimer> Aimers;


    Rigidbody Body;

    PhxPawnController Controller = null;


    public bool TryEnterVehicle(PhxSoldier soldier, 
                                out string NinePoseAnim, 
                                out Transform PilotPosition)
    {
        // Find first available seat
        GameObject SoldierObj = soldier.gameObject;

        NinePoseAnim = null;
        PilotPosition = null;


        if (AddSoldier(soldier))
        {   
            Controller = Driver.GetController();
            CameraFollow();

            NinePoseAnim = "";
            PilotPosition = transform;

            return true;
        }
        else 
        {
            NinePoseAnim = null;//C.Flyer.;
            PilotPosition = null;
            return false;
        }
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



    public Vector2 Vec2FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector2 vOut = new Vector2();
        for (int i = 0; i < 2; i++)
        {
            vOut[i] = float.Parse(v[i]);
        }
        return vOut;
    }


    public int InitWeapSection(uint[] properties, string[] values, int i, bool print = false)
    {
        bool CreateNew = true;
        PhxAimer CurrAimer = new PhxAimer();

        while (++i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("HierarchyLevel"))
            {
                CurrAimer.HierarchyLevel = 1;
            }
            else if (properties[i] == HashUtils.GetFNV("AimerPitchLimits"))
            {
                CurrAimer.PitchRange = Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerYawLimits"))
            {
                CurrAimer.YawRange = Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("BarrelNodeName"))
            {
                CurrAimer.BarrelNode = UnityUtils.FindChildTransform(transform, values[i]);
            }     
            else if (properties[i] == HashUtils.GetFNV("AimerNodeName"))
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(transform, values[i]);
            }            
            else if (properties[i] == HashUtils.GetFNV("NextAimer"))
            {

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


    Vector3 GroundNormal;


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



        //Set up aimers
        Aimers = new List<PhxAimer>();


        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        int i = 0;
        bool Exit = false;
        while (i < properties.Length && !Exit)
        {
            if (properties[i] == HashUtils.GetFNV("FLYERSECTION"))
            {
                if (EC.Name == "rep_hover_fightertank")
                {
                    Debug.Log("Initting flyer sec...");
                }

                i++;
                while (i < properties.Length && properties[i] != HashUtils.GetFNV("FLYERSECTION"))
                {
                    if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
                    {
                        if (EC.Name == "rep_hover_fightertank")
                        {
                            Debug.Log("Initting weap sec...");
                        }
                        i = InitWeapSection(properties, values, i, EC.Name == "rep_hover_fightertank");

                        Exit = true;
                        break;
                    }

                    i++;
                }
            }

            i++;
        }
    }


    int CurrAimer = 0;

    Vector3 ViewDirection = new Vector3(0.0f, 0.5f, 6.0f);


    void Update()
    {
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
                    // If the aimer fires successfully, 
                    CurrAimer++;
                }

                if (CurrAimer >= Aimers.Count)
                {
                    CurrAimer = 0;
                }
            }
        }


        ViewDirection = Quaternion.Euler(3.0f * Controller.mouseY, 0.0f, 0.0f) * ViewDirection;


        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(transform.TransformPoint(ViewDirection));
            Aimer.UpdateBarrel();
        }

        CAM.VehicleFocus = ViewDirection;

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




    void FixedUpdate()
    {
        Vector3 localVel = transform.worldToLocalMatrix * Body.velocity;


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

        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, rotRate * Controller.mouseX, 0.0f) * Time.fixedDeltaTime);
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


        if (localVel.x > C.StrafeSpeed)
        {
            localVel.x = C.StrafeSpeed;
        }

        if (localVel.x < -C.StrafeSpeed)
        {
            localVel.x = -C.StrafeSpeed;
        }

        if (localVel.z < -C.ReverseSpeed)
        {
            localVel.z = -C.ReverseSpeed;
        }

        if (localVel.z > C.ForwardSpeed)
        {
            localVel.z = C.ForwardSpeed;
        }

        Body.velocity = transform.localToWorldMatrix * localVel;

    }




    public override void BindEvents(){}
    public override void Fixate(){}
    public override IPhxWeapon GetPrimaryWeapon(){return null;}
    public void AddAmmo(float amount){}
    public override void PlayIntroAnim(){}
    public override PhxInstance GetAim(){return null;}
    void StateFinished(int layer){}
    public void AddHealth(float amount){}
}
