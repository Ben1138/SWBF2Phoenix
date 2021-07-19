
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHover : PhxVehicle
{
    protected class PhxHoverSpring
    {
        public float OmegaXFactor = 0f; // neg if z coord is pos 
        public float OmegaZFactor = 0f; // neg if x coord is neg
        public Vector3 Position;
        public float Length = 0f;
        public float Scale;

        public PhxHoverSpring(Vector4 PosScale)
        {
            Scale = PosScale.w;
            Position = new Vector3(PosScale.x, PosScale.y, PosScale.z);

			if (Position.x < -.001)
        	{
        		OmegaZFactor = -1f;
        	}
        	else if (Position.x > .001)
        	{
        		OmegaZFactor = 1f;
        	}

        	if (Position.z < -.001)
        	{
        		OmegaXFactor = 1f;
        	}
        	else if (Position.z > .001)
        	{
        		OmegaXFactor = -1f;
        	} 

        	Length = Scale;
        }

        public string ToString()
        {
            return String.Format("Position: {0}, Scale: {1}, Length: {2}", Position.ToString("F2"), Scale, Length);
        }
    }


    public class ClassProperties : PhxVehicleProperties
    {
        public PhxProp<float> Acceleration = new PhxProp<float>(5.0f);
        public PhxProp<float> Deceleration = new PhxProp<float>(5.0f);

        public PhxProp<float> ForwardSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> ReverseSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> StrafeSpeed = new PhxProp<float>(5.0f);

        public PhxProp<float> BoostSpeed = new PhxProp<float>(5.0f);
        public PhxProp<float> BoostAcceleration = new PhxProp<float>(5.0f);

        //  This is the altitude the hover SPAWNS at.  Actual hover height
        // is controlled by the spring settings!
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

        public PhxProp<float> VelocitySpring = new PhxProp<float>(.5f);
        public PhxProp<float> VelocityDamp   = new PhxProp<float>(.5f);
        public PhxProp<float> OmegaXSpring   = new PhxProp<float>(.5f);
        public PhxProp<float> OmegaXDamp     = new PhxProp<float>(.5f);
        public PhxProp<float> OmegaZSpring   = new PhxProp<float>(1.7f);
        public PhxProp<float> OmegaZDamp     = new PhxProp<float>(1.7f);
    }
    
    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);



    Rigidbody Body;

    PhxHoverMainSection DriverSection = null;
    PhxHover.ClassProperties H;


    // If the vehicle has treads, we update the UV offset
    // of the mat used by the WheelTexture node via its MeshRenderer.
    List<MeshRenderer> WheelRenderers; 

    // Poser for movement anims
    PhxPoser Poser;

    // Springs for precise movement
    List<PhxHoverSpring> Springs;

    AudioSource AudioAmbient;


    List<int> CollisionMasks;
    List<GameObject> CollisionNodes;



    // Paired object with kinematic rigidbody and SO collision
    // Used to isolate concave collision so non-kinematic physics
    // can be used on vehicle.
    GameObject SOColliderObject;

    [Serializable]
    public class SpringForce 
    {
    	public float VelForce;
    	public float VelDamp;

    	public float XRot;
    	public float XDamp;

    	public float ZRot;
    	public float ZDamp;

    	public float Penetration;
    }

    public List<SpringForce> SpringForces;


    void ResetCollisionLayer()
    {
        int i = 0;
        foreach (GameObject Node in CollisionNodes)
        {
            Node.layer = CollisionMasks[i++];
        }
    }

    void SetCollisionLayer(int layer)
    {
        foreach (GameObject Node in CollisionNodes)
        {
            Node.layer = layer;
        }
    }



    HashSet<Collider> ObjectColliders;



    // for debugging
    bool DBG = false;

    Vector3 startPos;

    public override void Init()
    {
        SetupEnterTrigger();

        H = C as PhxHover.ClassProperties;

        if (H == null) return;

        transform.position += Vector3.up * H.SetAltitude;

        startPos = transform.position;

        List<MeshCollider> MeshColliders = GetMeshColliders(transform);
        if (MeshColliders.Count > 0)
        {
            //SOColliderObject = new GameObject(gameObject.name + "_SO");
            //var soBody = SOColliderObject.AddComponent<Rigidbody>();
            //soBody.isKinematic = true;

            foreach (MeshCollider MC in MeshColliders)
            {
                //var SOMC = SOColliderObject.AddComponent<MeshCollider>();
                //SOMC.sharedMesh = MC.sharedMesh;
                //SOMC.enabled = false;
                Destroy(MC);
            }

            //SOColliderObject.AddComponent<PhxSeparateCollider>();
        }

        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = H.GravityScale;
        Body.useGravity = true;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Body.isKinematic = false;

        // These get calculated automatically when adding colliders/children IF 
        // they are not set manually beforehand!!
        Body.centerOfMass = Vector3.zero;
        Body.inertiaTensor = new Vector3(1f,1f,1f);

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

        i = 0;
        PhxHoverSpring CurrSpring = null;
        Springs = new List<PhxHoverSpring>();

        SpringForces = new List<SpringForce>();
        
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("AddSpringBody"))
            {
                CurrSpring = new PhxHoverSpring(PhxUtils.Vec4FromString(values[i]));
                Springs.Add(CurrSpring);
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.radius = CurrSpring.Scale;
                sc.center = CurrSpring.Position;
                sc.isTrigger = true;
                sc.enabled = false;
                
                SpringForces.Add(new SpringForce());
            }
            else if (properties[i] == HashUtils.GetFNV("BodySpringLength"))
            {
                if (CurrSpring != null)
                {   
                    CurrSpring.Length = float.Parse(values[i]);
                }
            }
            else if (properties[i] == HashUtils.GetFNV("BodyOmegaXSpringFactor"))
            {
                if (CurrSpring != null)
                {
                    CurrSpring.OmegaXFactor = float.Parse(values[i]);
                }
            }
            else if (properties[i] == HashUtils.GetFNV("BodyOmegaZSpringFactor"))
            {
                if (CurrSpring != null)
                {
                    CurrSpring.OmegaZFactor = float.Parse(values[i]);
                }
            }

            i++;
        }
        

        if (H.AnimationName.Get() != "" && H.FinAnimation.Get() != "")
        {
            Poser = new PhxPoser(H.AnimationName.Get(), H.FinAnimation.Get(), transform);
        }

        List<Transform> Nodes = UnityUtils.GetChildTransforms(transform);
        Nodes.Add(transform);

        CollisionNodes = new List<GameObject>();
        CollisionMasks = new List<int>();

        foreach (Transform Node in Nodes)
        {
            if (Node.gameObject.GetComponent<Collider>() != null)
            {
                CollisionNodes.Add(Node.gameObject);
                CollisionMasks.Add(Node.gameObject.layer);
            }
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



    /*
    Update each section, pose if the poser is set, and wheel texture offset if applicable.
    */

    Vector2 WhellUVOffset = Vector2.zero;

    void UpdateState(float deltaTime)
    {
        if (SOColliderObject != null)
        {
            SOColliderObject.transform.position = transform.position;
            SOColliderObject.transform.rotation = transform.rotation;
        }

        foreach (var section in Sections)
        {
            section.Update();
        }

        Vector3 Input = Vector3.zero;
        DriverController = DriverSection.GetController();
        if (DriverController != null) 
        {
            Input.x = DriverController.MoveDirection.x;
            Input.y = DriverController.MoveDirection.y;
            Input.z = DriverController.mouseX;
        }

        if (Poser != null)
        {
            float blend = 2f * deltaTime;

            if (Vector3.Magnitude(Input) < .001f)
            {
                Poser.SetState(PhxNinePoseState.Idle, blend);
                return;
            }

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

        /*
        if (DriverSection.IsOccupied())
        {
            WhellUVOffset.x += deltaTime;

            foreach (var renderer in WheelRenderers)
            {
                if(renderer.enabled)
                {
                    renderer.materials[0].SetTextureOffset(renderer.materials[0].mainTexture.name, WhellUVOffset);
                }
            }
        }
        */
    }


    
    /* 
        Each spring is processed by shooting a ray from its origin via the vehicle's down
        vector.  If the ray hits another object and does so within the radius (Scale) of the
        spring, a seperate torque and force will be applied to the vehicle.

        The exact function of the various parameters is still unknown, ideally I'll have an 
        equation done/tested soon.  But this method produces gamelike behavior from the 
        parameters.

        Possible upgrades depend on performance trade off:
            
            - Cast spheres and compute penetration between colliders manually with 
            Physics.OverlapSphere and Physics.ComputePenetration.  Springs ingame are
            spherical, though I bet a downward raycast will be sufficient.  
            
            - Maybe use actual Unity springs?  Haven't bothered, seemed unnecessary.

        Confusion on parameters remains:

            - OmegaXSpringFactor is used to compensate for an imbalance of springs.  Eg, when two have
            negative z coords and one has positive, the latter spring will have a negative OmegaXSpringFactor (stap, speeders).
            The opposite is also true (AAT).  What do these values default to?  Are they calculated from the spring
            location?

            - Omega is typically used with angular quantities, so I assume the explicit references mean
            the engine computes an upward force and a torque seperately.

            - How are the various global spring parameters like LiftSpring/Damp and VelocitySpring/Damp used and
            combined with the local ones? 
    */  

    void UpdateSprings(float deltaTime)
    {
        Vector3 netForce = Vector3.zero;
        Vector3 netPos = Vector3.zero;

        LayerMask Mask = 1 << 2;
        SetCollisionLayer(2);

        for (int CurrSpringIndex = 0; CurrSpringIndex < Springs.Count; CurrSpringIndex++)
        {
            var CurrSpring = Springs[CurrSpringIndex];

            if (Physics.Raycast(transform.TransformPoint(CurrSpring.Position), -transform.up, out RaycastHit hit, CurrSpring.Scale, ~Mask, QueryTriggerInteraction.Ignore))
            {
            	float Penetration = CurrSpring.Length - hit.distance;
                SpringForces[CurrSpringIndex].Penetration = Penetration;

                if (hit.collider.gameObject != gameObject && Penetration > 0f)
                {
                	// Check for imminent hard collision
                	// ...

                    float XRotCoeff = 2f * H.OmegaXSpring * CurrSpring.OmegaXFactor * Penetration;
                    float XDampRotCoeff = .3f * H.OmegaXDamp * -Vector3.Dot(Body.angularVelocity, transform.right);
                    Body.AddRelativeTorque(600f * deltaTime * Vector3.right * (XRotCoeff + XDampRotCoeff));

                    SpringForces[CurrSpringIndex].XRot = XRotCoeff;
                    SpringForces[CurrSpringIndex].XDamp = XDampRotCoeff;

                    float ZRotCoeff = H.OmegaZSpring * CurrSpring.OmegaZFactor * Penetration;
                    float ZRotDampCoeff = .5f * H.OmegaZDamp * -Vector3.Dot(Body.angularVelocity, transform.forward);
                    Body.AddRelativeTorque(800f * deltaTime * Vector3.forward * (ZRotCoeff + ZRotDampCoeff));
                    
                    SpringForces[CurrSpringIndex].ZRot = ZRotCoeff;
                    SpringForces[CurrSpringIndex].ZDamp = ZRotDampCoeff;

                    Vector3 VelSpringForce = Vector3.up * H.VelocitySpring * Penetration;
                    Vector3 VelDampForce = .3f * Vector3.up * H.VelocityDamp * -Body.velocity.y;
                    Body.AddForce(800f * deltaTime * (VelSpringForce + VelDampForce), ForceMode.Acceleration);                

                    SpringForces[CurrSpringIndex].VelForce = VelSpringForce.y;
                    SpringForces[CurrSpringIndex].VelDamp = VelDampForce.y;
                }
                else 
                {
                    SpringForces[CurrSpringIndex].XRot = 0f;
                    SpringForces[CurrSpringIndex].XDamp = 0f;                	
                    SpringForces[CurrSpringIndex].ZRot = 0f;
                    SpringForces[CurrSpringIndex].ZDamp = 0f;
                    SpringForces[CurrSpringIndex].VelForce = 0f;
                    SpringForces[CurrSpringIndex].VelDamp = 0f;
                }  
            }
        }

        ResetCollisionLayer();
    }


    Vector3 LocalVel = Vector3.zero;
    Vector3 LocalAngVel = Vector3.zero;
    PhxPawnController DriverController;

    void UpdatePhysics(float deltaTime)
    {
        LocalVel = transform.worldToLocalMatrix * Body.velocity;
        LocalAngVel = transform.worldToLocalMatrix * Body.angularVelocity;
    
        UpdateSprings(deltaTime);


        DriverController = DriverSection.GetController();
        if (DriverController == null) 
        {
            return;
        }

        // If we're moving, we Spin, if not we Turn
        float rotRate = Vector3.Magnitude(LocalVel) < .1f ? H.SpinRate : H.TurnRate;

        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, 16f * rotRate * DriverController.mouseX, 0f) * deltaTime);
        Body.MoveRotation(Body.rotation * deltaRotation);

        float strafe = DriverController.MoveDirection.x;
        float drive  = DriverController.MoveDirection.y;

        float forwardForce, strafeForce;

        // If moving in opposite direction of current vel...
        if (LocalVel.z >= 0f && drive < 0f)
        {
            forwardForce = drive * H.Deceleration;
        }
        else
        {
            forwardForce = drive * H.Acceleration;
        }

        // ''
        if (LocalVel.x - strafe > LocalVel.x)
        {
            strafeForce = strafe * H.Deceleration;
        }
        else 
        {
            strafeForce = strafe * H.Acceleration;
        }

        // engine accel, don't add force here because we want to limit local velocity manually
        LocalVel += deltaTime * new Vector3(strafeForce, 0f, forwardForce) / Body.mass;

        // clamp speeds by ODF vals, for now doesn't damp
        LocalVel.x = Mathf.Clamp(LocalVel.x, -H.StrafeSpeed, H.StrafeSpeed);
        LocalVel.z = Mathf.Clamp(LocalVel.z, -H.ReverseSpeed, H.ForwardSpeed);

        Body.velocity = transform.localToWorldMatrix * LocalVel;
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
