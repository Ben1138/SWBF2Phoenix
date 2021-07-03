
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

    private List<PhxVehicleSection> Sections;


    Rigidbody Body;

    PhxPawnController Controller = null;

    PhxHoverMainSection DriverSection = null;

    PhxInstance Aim;



    public int GetNextAvailableSeat(int startIndex = -1)
    {
        int numSeats = Sections.Count;
        int i = (startIndex + 1) % numSeats;
        
        while (i != startIndex)
        {
            if (Sections[i].Occupant == null)
            {
                return i;
            }

            if (startIndex == -1 && i == numSeats - 1)
            {
                return -1;
            }

            i = (startIndex + 1) % numSeats;
        }

        return -1;
    }



    public bool TryEnterVehicle(PhxSoldier soldier, 
                                out string NinePoseAnim, 
                                out Transform PilotPosition)
    {
        NinePoseAnim = "";
        PilotPosition = null;

        // Find first available seat
        int seat = GetNextAvailableSeat();

        if (seat == -1)
        {
            return false;
        }
        else 
        {
            Sections[seat].SetOccupant(soldier);
            PhxGameRuntime.GetCamera().FollowTrackable(Sections[seat]);
            return true;
        }
    }


    public bool TrySwitchSeat(int index)
    {
        // Get next index
        int seat = GetNextAvailableSeat(index);

        if (seat == -1)
        {
            return false;
        }
        else
        {
            Sections[seat].SetOccupant(Sections[index].Occupant);
            return true;
        }
    }


    public bool Eject(int i)
    {
        if (i >= Sections.Count)
        {
            return false;
        }

        if (Sections[i] != null || Sections[i].Occupant != null)
        {
            Sections[i].Occupant.SetFree(transform.position + Vector3.up * 2.0f);
            Sections[i].Occupant = null;
            PhxGameRuntime.GetCamera().Follow(Sections[i].Occupant);

            return true;
        }
        else 
        {
            return false;
        }
    }



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


        Sections = new List<PhxVehicleSection>();


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
                    DriverSection = new PhxHoverMainSection(properties, values, ref i, this, EC.Name == "rep_hover_fightertank");
                    Sections.Add(DriverSection);                
                }
                else 
                {
                    Sections.Add(new PhxVehicleTurret(properties, values, ref i, transform, TurretIndex++));
                }
            }
            else 
            {
                i++;
            }
        }
    }


    void Update()
    {
        foreach (var section in Sections)
        {
            section.Update();
        }
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


        float rotRate = Vector3.Magnitude(localVel) < .1f ? C.SpinRate : C.TurnRate;

        Vector3 DriverInput = DriverSection.GetDriverInput();


        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, 3f * rotRate * DriverInput.z, 0f) * Time.fixedDeltaTime);
        Body.MoveRotation(Body.rotation * deltaRotation);


        float strafe = DriverInput.x;
        float drive = DriverInput.y;

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
        return Sections[0].GetCameraPosition();
    }

    public Quaternion GetCameraRotation()
    {
        return Sections[0].GetCameraRotation();
    }


    public override bool IncrementSlice(out float progress)
    {
        progress = SliceProgress;
        return false;
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
