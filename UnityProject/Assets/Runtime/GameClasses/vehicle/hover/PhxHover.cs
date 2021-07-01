
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


    public class PhxAimer
    {
        public Transform node;
        public Vector2 pitchRange;
        public Vector3 yawRange;

        bool TestAndLimitRotation()
        {

        }
    }




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
    }


    
    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


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


        //PhxVehicleSection SoldiersSection = AddSoldier(soldier);

        if (AddSoldier(soldier))
        {   
            //NinePoseAnim = SoldiersSection.NinePoseAnim;
            //PilotPosition = SoldiersSection.PilotPosition;

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


    //Vector3 CurrSpeed = 0.0f;


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
        Body.interpolation = RigidbodyInterpolation.Extrapolate;
        Body.constraints = RigidbodyConstraints.FreezeRotationZ;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }



    void Update()
    {
        if (Controller == null)
        {
            return;
        }

        if (Controller.TryEnterVehicle)
        {
            Driver.SetFree(transform.position + Vector3.up * 2.0f);
            EjectDriver();
            return;
        }




        Controller.ViewDirection 







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
