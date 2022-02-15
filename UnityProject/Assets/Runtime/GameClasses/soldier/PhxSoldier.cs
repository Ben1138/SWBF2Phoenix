using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxSoldier : PhxControlableInstance<PhxSoldier.ClassProperties>, ICraAnimated, IPhxTickable
{
    static PhxGame Game => PhxGame.Instance;
    static PhxMatch Match => PhxGame.GetMatch();
    static PhxScene Scene => PhxGame.GetScene();
    static PhxCamera Camera => PhxGame.GetCamera();


    public class ClassProperties : PhxClass
    {
        public PhxProp<Texture2D> MapTexture = new PhxProp<Texture2D>(null);
        public PhxProp<float> MapScale = new PhxProp<float>(1.0f);
        public PhxProp<float> MapViewMin = new PhxProp<float>(1.0f);
        public PhxProp<float> MapViewMax = new PhxProp<float>(1.0f);
        public PhxProp<float> MapSpeedMin = new PhxProp<float>(1.0f);
        public PhxProp<float> MapSpeedMax = new PhxProp<float>(1.0f);

        public PhxProp<string> HealthType = new PhxProp<string>("person");
        public PhxProp<float>  MaxHealth = new PhxProp<float>(100.0f);

        // Are these two the same? If so, which one has precedence?
        public PhxProp<string> AnimationName = new PhxProp<string>("human");
        public PhxProp<string> SkeletonName = new PhxProp<string>("human");

        public PhxProp<float> MaxSpeed = new PhxProp<float>(1.0f);
        public PhxProp<float> MaxStrafeSpeed = new PhxProp<float>(1.0f);
        public PhxProp<float> MaxTurnSpeed = new PhxProp<float>(1.0f);
        public PhxProp<float> JumpHeight = new PhxProp<float>(1.0f);
        public PhxProp<float> JumpForwardSpeedFactor = new PhxProp<float>(1.0f);
        public PhxProp<float> JumpStrafeSpeedFactor = new PhxProp<float>(1.0f);
        public PhxProp<float> RollSpeedFactor = new PhxProp<float>(1.0f);
        public PhxProp<float> Acceleration = new PhxProp<float>(1.0f);
        public PhxProp<float> SprintAccelerateTime = new PhxProp<float>(1.0f);

        public PhxMultiProp ControlSpeed = new PhxMultiProp(typeof(string), typeof(float), typeof(float), typeof(float));

        public PhxProp<float> EnergyBar = new PhxProp<float>(1.0f);
        public PhxProp<float> EnergyRestore = new PhxProp<float>(1.0f);
        public PhxProp<float> EnergyRestoreIdle = new PhxProp<float>(1.0f);
        public PhxProp<float> EnergyDrainSprint = new PhxProp<float>(1.0f);
        public PhxProp<float> EnergyMinSprint = new PhxProp<float>(1.0f);
        public PhxProp<float> EnergyCostJump = new PhxProp<float>(0.0f);
        public PhxProp<float> EnergyCostRoll = new PhxProp<float>(1.0f);

        public PhxProp<float> AimValue = new PhxProp<float>(1.0f);
        public PhxProp<float> AimFactorPostureSpecial = new PhxProp<float>(1.0f);
        public PhxProp<float> AimFactorPostureStand = new PhxProp<float>(1.0f);
        public PhxProp<float> AimFactorPostureCrouch = new PhxProp<float>(1.0f);
        public PhxProp<float> AimFactorPostureProne = new PhxProp<float>(1.0f);
        public PhxProp<float> AimFactorStrafe = new PhxProp<float>(0.0f);
        public PhxProp<float> AimFactorMove = new PhxProp<float>(1.0f);

        public PhxPropertySection Weapons = new PhxPropertySection(
            "WEAPONSECTION",
            ("WeaponName",    new PhxProp<string>(null)),
            ("WeaponAmmo",    new PhxProp<int>(0)),
            ("WeaponChannel", new PhxProp<int>(0))
        );

        public PhxProp<string> AISizeType = new PhxProp<string>("SOLDIER");
    }

    // Original SWBF2 Control States, see: com_inf_default.odf
    enum PhxControlState
    {
        Stand,
        Crouch,
        Prone,
        Sprint,
        Jet,
        Jump,
        Roll,
        Tumble,
    }

    enum PhxSoldierContext
    {
        Free,
        Pilot,
    }

    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


    PhxSoldierContext Context = PhxSoldierContext.Free;

    // Vehicle related fields
    PhxSeat CurrentSeat;

    PhxPoser Poser;

    PhxAnimHuman Animator;
    PhxAnimHandle StateMachine;
    Rigidbody Body;

    // Important skeleton bones
    Transform HpWeapons;
    Transform Spine;
    Transform Neck;

    PhxControlState State;

    // Physical raycast downwards
    bool Grounded;
    int GroundedLayerMask;

    // How long to still be alerted after the last fire / hit
    const float AlertTime = 3f;
    float AlertTimer;

    // Minimum time not grounded, after which we're considered falling
    const float FallTime = 0.2f;

    // Count time while jumping/falling
    float FallTimer;

    // Minimum time we're considered falling when jumping
    const float JumpTime = 0.2f;
    float JumpTimer;

    // When > 0, we're currently landing
    float LandTimer;

    // Time it takes to turn left/right when idle (not walking)
    const float TurnTime = 0.2f;
    float TurnTimer;
    Quaternion TurnStart;

    Vector3 CurrSpeed;

    // Settings for stairs/steps/slopes
    const float MaxStepHeight = 0.31f;
    static readonly Vector3 StepCheckOffset = new Vector3(0f, MaxStepHeight + 0.1f,  0.4f);
    const float StepUpForceMulti = 20.0f;

    bool IsFixated => Body == null;


    bool bHasLookaroundIdleAnim = false;
    bool bHasCheckweaponIdleAnim = false;
    bool LastIdle = false;
    const float IdleTime = 10f;

    // <stance>, <thrustfactor> <strafefactor> <turnfactor>
    float[][] ControlValues;

    // First array index is whether:
    // - 0 : Primary Weapon
    // - 1 : Secondary Weapon
    IPhxWeapon[][] Weapons = new IPhxWeapon[2][];
    int[] WeaponIdx = new int[2] { -1, -1 };


    public override void Init()
    {
        gameObject.layer = LayerMask.NameToLayer("SoldierAll");

        ViewConstraint.x = 45f;

        // TODO: base turn speed in degreees/sec really 45?
        MaxTurnSpeed.y = 45f * C.MaxTurnSpeed;

        // Assume we're grounded on spawn
        Grounded = true;

        PhxControlState[] states = (PhxControlState[])Enum.GetValues(typeof(PhxControlState));
        ControlValues = new float[states.Length][];
        for (int i = 0; i < states.Length; ++i)
        {
            ControlValues[i] = GetControlSpeed(states[i]);
        }

        HpWeapons = PhxUtils.FindTransformRecursive(transform, "hp_weapons");
        Neck = PhxUtils.FindTransformRecursive(transform, "bone_neck");
        Spine = PhxUtils.FindTransformRecursive(transform, "bone_b_spine");
        Debug.Assert(HpWeapons != null);
        Debug.Assert(Spine != null);

        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = 0.1f;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Extrapolate;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        CapsuleCollider coll = gameObject.AddComponent<CapsuleCollider>();
        coll.height = 1.8f;
        coll.radius = 0.3f;
        coll.center = new Vector3(0f, 0.9f, 0f);

        // Idk whether there's a better method for this, but haven't found any
        GroundedLayerMask = 0;
        for (int i = 0; i < 32; ++i)
        {
            GroundedLayerMask |= Physics.GetIgnoreLayerCollision(i, gameObject.layer) ? 0 : 1 << i;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Weapons
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        var weapons = new List<IPhxWeapon>[2]
        {
            new List<IPhxWeapon>(),
            new List<IPhxWeapon>()
        };

        HashSet<string> weaponAnimBanks = new HashSet<string>();

        foreach (Dictionary<string, IPhxPropRef> section in C.Weapons)
        {
            int channel = 0;
            if (section.TryGetValue("WeaponChannel", out IPhxPropRef chVal))
            {
                PhxProp<int> weapCh = (PhxProp<int>)chVal;
                channel = weapCh;
            }
            Debug.Assert(channel >= 0 && channel < 2);

            if (section.TryGetValue("WeaponName", out IPhxPropRef nameVal))
            {
                PhxProp<string> weapCh = (PhxProp<string>)nameVal;
                PhxClass weapClass = Scene.GetClass(weapCh);
                if (weapClass != null)
                {
                    PhxProp<int> medalProp = weapClass.P.Get<PhxProp<int>>("MedalsTypeToUnlock");
                    if (medalProp != null && medalProp != 0)
                    {
                        // Skip medal/award weapons for now
                        continue;
                    }

                    IPhxWeapon weap = Scene.CreateInstance(weapClass, false, HpWeapons) as IPhxWeapon;
                    if (weap != null)
                    {
                        weap.SetIgnoredColliders(new List<Collider>() {gameObject.GetComponent<CapsuleCollider>()});

                        string weapAnimName = weap.GetAnimBankName();
                        if (!string.IsNullOrEmpty(weapAnimName))
                        {
                            weaponAnimBanks.Add(weapAnimName);
                        }

                        weapons[channel].Add(weap);

                        // init weapon as inactive
                        weap.GetInstance().gameObject.SetActive(false);
                        weap.OnShot(() => FireAnimation(channel == 0));
                        weap.OnReload(Reload);
                    }
                    else
                    {
                        Debug.LogWarning($"Instantiation of weapon class '{weapCh}' failed!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot find weapon class '{weapCh}'!");
                }
            }

            // TODO: weapon ammo
        }

        Weapons[0] = weapons[0].Count == 0 ? new IPhxWeapon[1] { null } : weapons[0].ToArray();
        Weapons[1] = weapons[1].Count == 0 ? new IPhxWeapon[1] { null } : weapons[1].ToArray();


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Animation
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        string[] weapAnimBanks = new string[weaponAnimBanks.Count];
        weaponAnimBanks.CopyTo(weapAnimBanks);

        string characterAnim = C.AnimationName;
        if (characterAnim.ToLower() == "human" && C.SkeletonName.Get().ToLower() != "human")
        {
            characterAnim = C.SkeletonName;
        }

        Animator = new PhxAnimHuman(Scene.AnimResolver, transform, characterAnim, weapAnimBanks);
        StateMachine = Scene.StateMachines.StateMachine_NewHuman(Animator);

        // this needs to happen after the Animator is initialized, since swicthing
        // will weapons will most likely cause an animation bank change aswell
        NextWeapon(0);
        //NextWeapon(1);
    }

    public override void Destroy()
    {
        
    }

    public override void Fixate()
    {
        Destroy(Body);
        Body = null;
        Grounded = true;

        Destroy(GetComponent<CapsuleCollider>());
    }

    public override IPhxWeapon GetPrimaryWeapon()
    {
        return Weapons[0][WeaponIdx[0]];
    }

    public void AddHealth(float amount)
    {
        if (amount < 0)
        {
            // we got hit! alert!
            AlertTimer = AlertTime;
        }

        float health = CurHealth + amount;
        if (health <= 0f)
        {
            health = 0;
            // TODO: dead!
        }
        CurHealth.Set(Mathf.Min(health, C.MaxHealth));
    }

    public void AddAmmo(float amount)
    {
        // TODO
    }

    public void NextWeapon(int channel)
    {
        Debug.Assert(channel >= 0 && channel < 2);

        if (WeaponIdx[channel] >= 0 && Weapons[channel][WeaponIdx[channel]] != null)
        {
            Weapons[channel][WeaponIdx[channel]].GetInstance().gameObject.SetActive(false);
        }
        if (++WeaponIdx[channel] >= Weapons[channel].Length)
        {
            WeaponIdx[channel] = 0;
        }
        if (Weapons[channel][WeaponIdx[channel]] != null)
        {
            Weapons[channel][WeaponIdx[channel]].GetInstance().gameObject.SetActive(true);
            Animator.SetActiveWeaponBank(Weapons[channel][WeaponIdx[channel]].GetAnimBankName());
        }
        else
        {
            Debug.LogWarning($"Encountered NULL weapon at channel {channel} and weapon index {WeaponIdx[channel]}!");
        }
    }

    public override void PlayIntroAnim()
    {
        //Animator.PlayIntroAnim();
    }

    void FireAnimation(bool primary)
    {
        //Animator.Anim.SetState(1, Animator.StandShootPrimary);
        //Animator.Anim.RestartState(1);
        Animator.InputShootPrimary.SetBool(true);
    }

    void Reload()
    {
        //IPhxWeapon weap = Weapons[0][WeaponIdx[0]];
        //if (weap != null)
        //{
        //    Animator.Anim.SetState(1, Animator.StandReload);
        //    Animator.Anim.RestartState(1);
        //    float animTime = Animator.Anim.GetCurrentState(1).GetDuration();
        //    Animator.Anim.SetPlaybackSpeed(1, Animator.StandReload, 1f / (weap.GetReloadTime() / animTime));
        //}
    }

    // Undoes SetPilot; called by vehicles/turrets when ejecting soldiers
    public void SetFree(Vector3 position)
    {
        Context = PhxSoldierContext.Free;
        CurrentSeat = null;

        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = 80f;
        Body.drag = 0.2f;
        Body.angularDrag = 10f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GetComponent<SkinnedMeshRenderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;

        transform.parent = null;
        transform.position = position;

        Poser = null;

        Controller.Enter = false;


        if (WeaponIdx[0] >= 0 && Weapons[0][WeaponIdx[0]] != null)
        {
            Weapons[0][WeaponIdx[0]].GetInstance().gameObject.SetActive(true);
        }
    }


    public void SetPilot(PhxSeat section)
    {
        Context = PhxSoldierContext.Pilot;
        CurrentSeat = section;

        if (Body != null)
        {
            Destroy(Body);
            Body = null;
        }

        GetComponent<CapsuleCollider>().enabled = false;


        if (section.PilotPosition == null)
        {
            GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else
        {
            GetComponent<SkinnedMeshRenderer>().enabled = true;

            transform.parent = section.PilotPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (CurrentSeat.PilotAnimationType != PilotAnimationType.None)
            {
                bool isStatic = CurrentSeat.PilotAnimationType == PilotAnimationType.StaticPose;
                string animName = isStatic ? CurrentSeat.PilotAnimation : CurrentSeat.Pilot9Pose;
                
                Poser = new PhxPoser("human_4", "human_" + animName, transform, isStatic);   
            }    
        }

        if (WeaponIdx[0] >= 0 && Weapons[0][WeaponIdx[0]] != null)
        {
            Weapons[0][WeaponIdx[0]].GetInstance().gameObject.SetActive(false);
        }
    }


    // see: com_inf_default
    float[] GetControlSpeed(PhxControlState state)
    {
        foreach (object[] values in C.ControlSpeed.Values)
        {
            string controlName = values[0] as string;
            if (!string.IsNullOrEmpty(controlName) && controlName == state.ToString().ToLowerInvariant())
            {
                return new float[3]
                {
                    (float)values[1],
                    (float)values[2],
                    (float)values[3],
                };
            }
        }
        Debug.LogError($"Cannot find control state '{state}'!");
        return null;
    }

    public void Tick(float deltaTime)
    {
        Profiler.BeginSample("Tick Soldier");
        UpdateState(deltaTime);
        Profiler.EndSample();
    }


    void UpdatePose(float deltaTime)
    {
        Vector4 Input = new Vector4(Controller.MoveDirection.x, Controller.MoveDirection.y, Controller.mouseX, Controller.mouseY);

        if (Poser != null && CurrentSeat != null)
        {
            float blend = 2f * deltaTime;

            if (CurrentSeat.PilotAnimationType == PilotAnimationType.NinePose)
            {
                if (Vector4.Magnitude(Input) < .001f)
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
            else if (CurrentSeat.PilotAnimationType == PilotAnimationType.FivePose)
            {
                if (Mathf.Abs(Input.z) + Mathf.Abs(Input.w) < .001f)
                {
                    Poser.SetState(PhxFivePoseState.Idle, blend);
                    return;
                }

                if (Input.z > .01f)
                {
                Poser.SetState(PhxFivePoseState.TurnRight, blend);            
                }
                else
                {
                    Poser.SetState(PhxFivePoseState.TurnLeft, blend);            
                }

                if (Input.w > .01f)
                {
                    Poser.SetState(PhxFivePoseState.TurnDown, blend);            
                }
                else
                {
                    Poser.SetState(PhxFivePoseState.TurnUp, blend);            
                }
            }
            else if (CurrentSeat.PilotAnimationType == PilotAnimationType.StaticPose)
            {
                Poser.SetState();
            }
            else 
            {
                // Not sure what happens if PilotPosition is defined but PilotAnimation/Pilot9Pose are missing...
            }
        }
    }

    void UpdateState(float deltaTime)
    {
        if (Context == PhxSoldierContext.Pilot && Controller != null)
        {
            //Animator.SetActive(false);
            UpdatePose(deltaTime);
            return;
        }

        //AnimationCorrection();

        AlertTimer = Mathf.Max(AlertTimer - deltaTime, 0f);

        if (Controller == null)
        {
            return;
        }

        // Will work into specific control schema.  Not sure when you can't enter vehicles...
        if (Controller.Enter && Context == PhxSoldierContext.Free)
        {
            PhxVehicle ClosestVehicle = null;
            float ClosestDist = float.MaxValue;
            
            Collider[] PossibleVehicles = Physics.OverlapSphere(transform.position, 5.0f, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider PossibleVehicle in PossibleVehicles)
            {
                GameObject CollidedObj = PossibleVehicle.gameObject;
                if (PossibleVehicle.attachedRigidbody != null)
                {
                    CollidedObj = PossibleVehicle.attachedRigidbody.gameObject;
                }

                PhxVehicle Vehicle = CollidedObj.GetComponent<PhxVehicle>();
                if (Vehicle != null && Vehicle.HasAvailableSeat())
                {
                    float Dist = Vector3.Magnitude(transform.position - Vehicle.transform.position);
                    if (Dist < ClosestDist)
                    {
                        ClosestDist = Dist;
                        ClosestVehicle = Vehicle;
                    }
                }
            }

            if (ClosestVehicle != null)
            {
                CurrentSeat = ClosestVehicle.TryEnterVehicle(this);
                if (CurrentSeat != null)
                {
                    SetPilot(CurrentSeat);
                    Controller.Enter = false;
                    return;
                }
            }
            
        }

        Animator.InputMovementX.SetFloat(Controller.MoveDirection.x);
        Animator.InputMovementY.SetFloat(Controller.MoveDirection.y);
        Animator.InputCrouch.SetBool(Controller.Crouch);
        Animator.InputShootPrimary.SetBool(Controller.ShootPrimary);
        Animator.InputShootSecondary.SetBool(Controller.ShootSecondary);
        Animator.InputEnergy.SetFloat(100.0f);

        if (Controller.NextPrimaryWeapon)
        {
            NextWeapon(0);
        }
        //if (Controller.NextSecondaryWeapon)
        //{
        //    NextWeapon(1);
        //}

        //Vector3 lookWalkForward = Controller.ViewDirection;
        //lookWalkForward.y = 0f;
        //Quaternion lookRot = Quaternion.LookRotation(lookWalkForward);
        //Quaternion moveRot = Quaternion.identity;

        //LandTimer = Mathf.Max(LandTimer - deltaTime, 0f);
        //TurnTimer = Mathf.Max(TurnTimer - deltaTime, 0f);

        //if (LandTimer == 0f)
        //{
        //    float accStep = C.Acceleration * deltaTime;
        //    float thrustFactor = ControlValues[(int)State][0];
        //    float strafeFactor = ControlValues[(int)State][1];
        //    float turnFactor = ControlValues[(int)State][2];

        //    Vector3 moveDirLocal = new Vector3(Controller.MoveDirection.x * turnFactor, 0f, Controller.MoveDirection.y);
        //    Vector3 moveDirWorld = lookRot * moveDirLocal;

        //    // Stand - Crouch - Sprint
        //    if (State == PhxControlState.Stand || State == PhxControlState.Crouch || State == PhxControlState.Sprint)
        //    {
        //        // TODO: base turn speed in degreees/sec really 45?
        //        MaxTurnSpeed.y = 45f * C.MaxTurnSpeed * turnFactor;

        //        if (moveDirLocal.magnitude == 0f)
        //        {
        //            CurrSpeed *= 0.1f * deltaTime;

        //            if (TurnTimer > 0f)
        //            {
        //                lookRot = Quaternion.Slerp(lookRot, TurnStart, TurnTimer / TurnTime);
        //            }
        //            else
        //            {
        //                //float rotDiff = Quaternion.Angle(transform.rotation, lookRot);
        //                float rotDiff = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, lookRot.eulerAngles.y);
        //                if (rotDiff < -40f || rotDiff > 60f)
        //                {
        //                    TurnTimer = TurnTime;
        //                    TurnStart = transform.rotation;
        //                    Animator.Anim.SetState(0, rotDiff < 0f ? Animator.TurnLeft : Animator.TurnRight);
        //                    Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);
        //                    Animator.Anim.RestartState(0);
        //                }

        //                lookRot = transform.rotation;
        //                moveRot = lookRot;
        //            }
        //        }
        //        else
        //        {
        //            CurrSpeed += moveDirWorld * accStep;

        //            float maxSpeed = moveDirLocal.z < 0.2f ? C.MaxStrafeSpeed : C.MaxSpeed;
        //            float forwardFactor = moveDirLocal.z < 0.2f ? strafeFactor : thrustFactor;
        //            CurrSpeed = Vector3.ClampMagnitude(CurrSpeed, maxSpeed * forwardFactor);

        //            moveRot = Quaternion.LookRotation(moveDirWorld);
        //            if (moveDirLocal.z <= 0f)
        //            {
        //                // invert look direction when strafing left/right
        //                moveDirWorld = -moveDirWorld;
        //            }
        //            lookRot = Quaternion.LookRotation(moveDirWorld);
        //        }

        //        if (TurnTimer == 0f)
        //        {
        //            // ---------------------------------------------------------------------------------------------
        //            // Forward
        //            // ---------------------------------------------------------------------------------------------
        //            float walk = Mathf.Clamp01(Controller.MoveDirection.magnitude);
        //            if (Controller.MoveDirection.y <= 0f)
        //            {
        //                // invert animation direction for strafing (left/right)
        //                walk = -walk;
        //            }

        //            if (State == PhxControlState.Sprint)
        //            {
        //                Animator.Anim.SetState(0, Animator.StandSprint);
        //            }
        //            else if (walk > 0.2f && walk <= 0.75f)
        //            {
        //                Animator.Anim.SetState(0, AlertTimer > 0f ? Animator.StandAlertWalk : Animator.StandWalk);
        //                Animator.Anim.SetPlaybackSpeed(0, Animator.StandWalk, walk / 0.75f);
        //            }
        //            else if (walk > 0.75f)
        //            {
        //                Animator.Anim.SetState(0, AlertTimer > 0f ? Animator.StandAlertRun : Animator.StandRun);
        //                Animator.Anim.SetPlaybackSpeed(0, Animator.StandRun, walk);
        //            }
        //            else if (walk < -0.2f)
        //            {
        //                Animator.Anim.SetState(0, AlertTimer > 0f ? Animator.StandAlertBackward : Animator.StandBackward);
        //                Animator.Anim.SetPlaybackSpeed(0, Animator.StandBackward, -walk);
        //            }
        //            else
        //            {
        //                Animator.Anim.SetState(0, AlertTimer > 0f ? Animator.StandAlertIdle : Animator.StandIdle);
        //            }
        //            // ---------------------------------------------------------------------------------------------
        //        }
        //    }

        //    // Stand - Crouch
        //    if (State == PhxControlState.Stand || State == PhxControlState.Crouch)
        //    {
        //        State = Controller.Crouch ? PhxControlState.Crouch : PhxControlState.Stand;

        //        // ---------------------------------------------------------------------------------------------
        //        // Idle
        //        // ---------------------------------------------------------------------------------------------
        //        if (Controller.IdleTime >= IdleTime)
        //        {
        //            if (bHasLookaroundIdleAnim && !bHasCheckweaponIdleAnim)
        //            {
        //                //Anim.SetTrigger(IdleNames[0]);
        //            }
        //            else if (!bHasLookaroundIdleAnim && bHasCheckweaponIdleAnim)
        //            {
        //                //Anim.SetTrigger(IdleNames[1]);
        //            }
        //            else if (bHasLookaroundIdleAnim && bHasCheckweaponIdleAnim)
        //            {
        //                //Anim.SetTrigger(IdleNames[UnityEngine.Random.Range(0, 1)]);
        //            }
        //            Controller.ResetIdleTime();
        //        }
        //        if (!Controller.IsIdle && LastIdle)
        //        {
        //            //Anim.SetTrigger("UnIdle");
        //        }
        //        // ---------------------------------------------------------------------------------------------


        //        // ---------------------------------------------------------------------------------------------
        //        // Shooting
        //        // ---------------------------------------------------------------------------------------------
        //        if (Weapons[0][WeaponIdx[0]].GetReloadProgress() == 1f)
        //        {
        //            if (Controller.ShootPrimary)
        //            {
        //                // only fire when not currently turning
        //                //Weap.Fire = TurnTimer <= 0f;

        //                Weapons[0][WeaponIdx[0]].Fire(Controller, Controller.GetAimPosition());
        //                AlertTimer = AlertTime;
        //            }
        //            else if (Controller.ShootSecondary)
        //            {
        //                Weapons[1][WeaponIdx[1]].Fire(Controller, Controller.GetAimPosition());
        //                AlertTimer = AlertTime;
        //            }
        //            else if (Controller.Reload)
        //            {
        //                Weapons[0][WeaponIdx[0]].Reload();
        //                //Anim.SetTrigger("Reload");
        //            }
        //        }
        //        // ---------------------------------------------------------------------------------------------
        //        if (Controller.NextPrimaryWeapon)
        //        {
        //            NextWeapon(0);
        //        }
        //        if (Controller.NextSecondaryWeapon)
        //        {
        //            NextWeapon(1);
        //        }
        //    }

        //    // Stand - Sprint
        //    if (State == PhxControlState.Stand || State == PhxControlState.Sprint)
        //    {
        //        // ---------------------------------------------------------------------------------------------
        //        // Jumping
        //        // ---------------------------------------------------------------------------------------------
        //        if (Controller.Jump)
        //        {
        //            State = PhxControlState.Jump;
        //            JumpTimer = JumpTime;

        //            Animator.Anim.SetState(0, Animator.Jump);
        //            Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);

        //            Body.AddForce(Vector3.up * Mathf.Sqrt(C.JumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        //        }
        //        // ---------------------------------------------------------------------------------------------
        //    }

        //    // Stand
        //    if (State == PhxControlState.Stand)
        //    {
        //        if (Controller.MoveDirection.y > 0.2f && Controller.Sprint && Weapons[0][WeaponIdx[0]].GetReloadProgress() == 1f)
        //        {
        //            State = PhxControlState.Sprint;
        //        }
        //    }

        //    // Crouch
        //    if (State == PhxControlState.Crouch)
        //    {
        //        if (Controller.Jump)
        //        {
        //            State = PhxControlState.Stand;
        //        }

        //        // TODO: verify
        //        else if (Controller.MoveDirection.y > 0.8f && Controller.Sprint)
        //        {
        //            State = PhxControlState.Sprint;
        //        }
        //    }

        //    // Sprint
        //    if (State == PhxControlState.Sprint)
        //    {
        //        if (Controller.MoveDirection.y < 0.8f || !Controller.Sprint)
        //        {
        //            State = PhxControlState.Stand;
        //        }
        //    }
        //}

        if (IsFixated)
        {
            //transform.rotation = lookRot;
            return;
        }


        Grounded = Physics.OverlapSphere(Body.position, 0.2f, GroundedLayerMask).Length > 1;

        // Not jumping -> Falling
        //if (State != PhxControlState.Jump && !Grounded)
        //{
        //    FallTimer += deltaTime;
        //    if (FallTimer > FallTime)
        //    {
        //        State = PhxControlState.Jump;
        //        Animator.Anim.SetState(0, Animator.Fall);
        //        Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);
        //    }
        //}

        //// Jump/Fall -> Land
        //if (State == PhxControlState.Jump)
        //{
        //    FallTimer += deltaTime;
        //    JumpTimer -= deltaTime;

        //    if (Grounded && JumpTimer < 0f)
        //    {
        //        State = PhxControlState.Stand;

        //        if (FallTimer > 1.5f)
        //        {
        //            LandTimer = 0.9f;
        //            Animator.Anim.SetState(0, Animator.LandHard);
        //            Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);
        //            //Debug.Log($"Land HARD {FallTimer}");
        //        }
        //        else if (FallTimer > 1.2f || Controller.MoveDirection.magnitude < 0.1f)
        //        {
        //            LandTimer = 0.6f;
        //            Animator.Anim.SetState(0, Animator.LandSoft);
        //            Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);
        //            //Debug.Log($"Land Soft {FallTimer}");
        //        }
        //        else
        //        {
        //            LandTimer = 0.05f;
        //            Animator.Anim.SetState(0, Animator.LandSoft);
        //            Animator.Anim.SetState(1, CraMain.Instance.Settings.STATE_NONE);
        //            //Debug.Log($"Land very Soft {FallTimer}");
        //        }

        //        FallTimer = 0f;
        //    }
        //}

        //if (Grounded && LandTimer == 0f && (State == PhxControlState.Stand || State == PhxControlState.Crouch || State == PhxControlState.Sprint))
        //{
        //    //Body.MovePosition(Body.position + CurrSpeed * deltaTime);
        //    Body.AddForce(CurrSpeed - Body.velocity, ForceMode.VelocityChange);
        //    Body.MoveRotation(lookRot);

        //    if (CurrSpeed != Vector3.zero)
        //    {
        //        Vector3 upper = moveRot * StepCheckOffset;

        //        // Handling stairs/steps/slopes
        //        if (Physics.Raycast(Body.position + upper, Vector3.down, out RaycastHit hit, upper.y * 2f))
        //        {
        //            float height = hit.point.y - Body.position.y;
        //            if (Mathf.Abs(height) > 0.05f && Mathf.Abs(height) <= MaxStepHeight)
        //            {
        //                //Debug.Log($"Height: {height}");
        //                Body.AddForce(Vector3.up * height * StepUpForceMulti, ForceMode.VelocityChange);
        //            }
        //        }

        //        //Debug.DrawRay(Body.position + upper, Vector3.down * upper.y, Color.red);
        //    }
        //}

        //Anim.SetBool("Alert", AlertTimer > 0f);
        LastIdle = Controller.IsIdle;
    }

    void OnDrawGizmosSelected()
    {
        if (Body != null)
        {
            Gizmos.DrawSphere(Body.position, 0.2f);
        }
    }

    public Vector3 RotAlt1 = new Vector3(7f, -78f, -130f);
    public Vector3 RotAlt2 = new Vector3(0f, -50f, -75f);
    public Vector3 RotAlt3 = new Vector3(0f, -68f, -81f);
    public Vector3 RotAlt4 = new Vector3(0f, -53f, -77f);

    void AnimationCorrection()
    {
        if (Context == PhxSoldierContext.Pilot) return;

        if (Controller == null/* || FallTimer > 0f || TurnTimer > 0f*/)
        {
            return;
        }

        if (State == PhxControlState.Stand || State == PhxControlState.Crouch)
        {
            //if (Animator.Anim.GetCurrentStateIdx(1) == Animator.StandShootPrimary)
            //{
            //    Spine.rotation = Quaternion.LookRotation(Controller.ViewDirection) * Quaternion.Euler(RotAlt4);
            //}
            //else if (AlertTimer > 0f)
            //{
            //    if (Controller.MoveDirection.magnitude > 0.1f)
            //    {
            //        Spine.rotation = Quaternion.LookRotation(Controller.ViewDirection) * Quaternion.Euler(RotAlt3);
            //    }
            //    else
            //    {
            //        Spine.rotation = Quaternion.LookRotation(Controller.ViewDirection) * Quaternion.Euler(RotAlt2);
            //    }
            //}
            //else if (Neck != null)
            //{
            //    Neck.rotation = Quaternion.LookRotation(Controller.ViewDirection) * Quaternion.Euler(RotAlt1);
            //}
        }
    }

    public CraStateMachine GetStateMachine()
    {
        return Animator.Machine;
    }
}
