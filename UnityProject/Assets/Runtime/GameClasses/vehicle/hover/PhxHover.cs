
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHover : PhxVehicle
{
    public class ClassProperties : PhxVehicleProperties
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

        public PhxProp<AudioClip> EngineSound = new PhxProp<AudioClip>(null);

        public PhxPropertySection Wheels = new PhxPropertySection(
            "WHEELSECTION",
            ("WheelTexture",  new PhxProp<string>("")),
            ("WheelVelocToV", new PhxProp<float>(0f)),
            ("WheelOmegaToV", new PhxProp<float>(0f))
        );
    }
    
    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);



    Rigidbody Body;

    PhxHoverMainSection DriverSection = null;
    PhxHover.ClassProperties H;


    // If the vehicle has treads, we programmatically set the UV offset
    // of the mat used by the WheelTexture node via its MeshRenderer.
    List<MeshRenderer> WheelRenderers; 


    PhxNinePoser NinePoser;


    AudioSource AudioAmbient;

    public override void Init()
    {
        BoxCollider TestCollider = gameObject.AddComponent<BoxCollider>();
        TestCollider.size = UnityUtils.GetMaxBounds(gameObject).extents * 2.5f;
        TestCollider.isTrigger = true;

        H = C as PhxHover.ClassProperties;

        if (H == null) return;

        PruneMeshColliders(transform);

        //BoxCollider PhysCollider = gameObject.AddComponent<BoxCollider>();
        //PhysCollider.size = UnityUtils.GetMaxBounds(gameObject).extents * 2.0f;
        
        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = H.GravityScale;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        Body.constraints = RigidbodyConstraints.FreezeRotationZ;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        AudioAmbient = gameObject.AddComponent<AudioSource>();
        AudioAmbient.spatialBlend = 1.0f;
        AudioAmbient.clip = H.EngineSound;
        AudioAmbient.pitch = 1.0f;
        AudioAmbient.volume = 0.5f;
        AudioAmbient.rolloffMode = AudioRolloffMode.Linear;
        AudioAmbient.minDistance = 2.0f;
        AudioAmbient.maxDistance = 30.0f;


        Sections = new List<PhxVehicleSection>();

        var EC = H.EntityClass;
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
                    Sections.Add(new PhxVehicleTurret(properties, values, ref i, this, TurretIndex++));
                }
            }
            else 
            {
                i++;
            }
        }


        if (H.AnimationName.Get() != "" && H.FinAnimation.Get() != "")
        {
            NinePoser = new PhxNinePoser(H.AnimationName.Get(), H.FinAnimation.Get(), transform);
        }

        /*
        Get necessary wheel info.  Implementation on hold until PhxModel sorted out.


        WheelRenderers = new List<MeshRenderer>();

        foreach (Dictionary<string, IPhxPropRef> section in H.Wheels)
        {   
            // Named texture, actually refers to node.
            section.TryGetValue("WheelTexture", out IPhxPropRef wheelNode);

            PhxProp<string> WheelNodeName = (PhxProp<string>) wheelNode;

            Debug.LogFormat("Yea found a wheel: {0}", WheelNodeName.Get());

            Model model = ModelLoader.Instance.GetModelWrapper();
            foreach (Segment seg in model.GetSegments())
            {
                if (seg.Tag.Equals(WheelNodeName.Get(), StringComparison.OrdinalIgnoreCase))
                {
                    Transform WheelTx;
                    if (seg.BoneName != "")
                    {
                        WheelTx = UnityUtils.FindChildTransform(transform, WheelNodeName.Get());
                           
                    }
                    else 
                    {
                        WheelTx = transform;
                    }

                    MeshRenderer r = WheelTx.gameObject.GetComponent<MeshRenderer>();
                    foreach (var mat in r.materials)
                    {
                        if (mat.mainTexture.name.Equals())
                    }

                }
            }
        }
        */
    }

    System.Random random = new System.Random();

    Vector2 uvOffset = Vector2.zero;

    void UpdateState(float deltaTime)
    {
        foreach (var section in Sections)
        {
            section.Update();
        }


        if (NinePoser != null)
        {
            if (Vector3.Magnitude(localVel) < 1f)
            {
                NinePoser.SetState(PhxNinePoseState.Forward, deltaTime);
            }

            if (localVel.x > .1f)
            {
                NinePoser.SetState(PhxNinePoseState.StrafeRight, deltaTime);           
            }

            if (localVel.x < .1f)
            {
                NinePoser.SetState(PhxNinePoseState.StrafeLeft, deltaTime);            
            }
        }

        /*
        if (DriverSection.IsOccupied())
        {
            if (NinePoser != null)
            {
                NinePoser.SetState(t, 1f);
            }
        }
        */

        /*
        if (DriverSection.IsOccupied())
        {
            uvOffset.x += deltaTime;

            foreach (var renderer in WheelRenderers)
            {
                if(renderer.enabled)
                {
                    renderer.materials[0].SetTextureOffset(renderer.materials[0].mainTexture.name, uvOffset);
                }
            }
        }
        */
    }



    Vector3 GroundNormal = Vector3.up;


    // Maintain eased alignment with ground normal
    private void AlignToGroundNormal(float deltaTime)
    {
        float angleFactor = 2f * (1f - Vector3.Dot(GroundNormal, transform.up));

        float timeFactor = .1f; //Set to some ODF derived value...

        float normalMult = angleFactor * deltaTime / timeFactor;
        float upMult = 1f - normalMult;

        upMult = 0f;
        normalMult = 1f;

        Vector3 newUp = upMult * transform.up + normalMult * GroundNormal;
        Vector3 newFd = Vector3.Cross(transform.right, newUp);

        /*
        if (gameObject.name == "vehicle_9" && C.EntityClass.Name == "cis_hover_stap" && Input.GetKeyDown(KeyCode.L))
        {
            Debug.LogFormat("Up: {0}, GroundNormal: {1}, upMult: {2}, normalMult {3}, newUp: {4}, newFd: {5}",
                            transform.up.ToString("F6"), GroundNormal.ToString("F6"), upMult,
                            normalMult, newUp.ToString("F6"), newFd.ToString("F6"));
        }
        */

        Body.MoveRotation(Quaternion.LookRotation(newFd, newUp));
    }


    Vector3 localVel;

    void UpdatePhysics(float deltaTime)
    {
        localVel = transform.worldToLocalMatrix * Body.velocity;

        // Maintain hover
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit))
        {
            GroundNormal = hit.normal;
            if (hit.distance < H.SetAltitude)
            {
                Body.AddForce(Vector3.up * H.LiftSpring * 9.8f, ForceMode.Acceleration);
            }
        }


        AlignToGroundNormal(deltaTime);


        float rotRate = Vector3.Magnitude(localVel) < .1f ? H.SpinRate : H.TurnRate;

        Vector3 DriverInput = DriverSection.GetDriverInput();


        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, 6f * rotRate * DriverInput.z, 0f) * deltaTime);
        Body.MoveRotation(Body.rotation * deltaRotation);


        float strafe = DriverInput.x;
        float drive = DriverInput.y;

        float forwardForce, strafeForce;

        // If moving in opposite direction of current vel...
        if (localVel.z > 0.0f && drive < 0.0f)
        {
            forwardForce = drive * H.Deceleration;
        }
        else
        {
            forwardForce = drive * H.Acceleration;
        }

        // ''
        if (localVel.x - strafe > localVel.x)
        {
            strafeForce = strafe * H.Deceleration;
        }
        else 
        {
            strafeForce = strafe * H.Acceleration;
        }

        Body.AddRelativeForce(new Vector3(strafeForce, 0.0f, forwardForce), ForceMode.Acceleration);


        // Will scrap soon, keeping for now
        localVel.x = Mathf.Clamp(localVel.x, -H.StrafeSpeed, H.StrafeSpeed);
        localVel.z = Mathf.Clamp(localVel.z, -H.ReverseSpeed, H.ForwardSpeed);

        Body.velocity = transform.localToWorldMatrix * localVel;
    }


    public override Vector3 GetCameraPosition()
    {
        return Sections[0].GetCameraPosition();
    }

    public override Quaternion GetCameraRotation()
    {
        return Sections[0].GetCameraRotation();
    }


    public override bool IncrementSlice(out float progress)
    {
        progress = SliceProgress;
        return false;
    }


    public override void Tick(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick Hover");
        UpdateState(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public override void TickPhysics(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick Hover Physics");
        UpdatePhysics(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
