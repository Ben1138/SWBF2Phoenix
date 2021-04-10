using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class GC_soldier : ISWBFInstance<GC_soldier.ClassProperties>, ISWBFSelectableCharacter
{
    static GameRuntime GAME => GameRuntime.Instance;
    static GameMatch MTC => GameRuntime.GetMatch();
    static SWBFCamera CAM => GameRuntime.GetCamera();

    static readonly string ANIM_CONTROLLER_NAME = "AnimController_soldier";

    public class ClassProperties : ISWBFClass
    {
        public Prop<Texture2D> MapTexture = new Prop<Texture2D>(null);
        public Prop<float> MapScale = new Prop<float>(1.0f);
        public Prop<float> MapViewMin = new Prop<float>(1.0f);
        public Prop<float> MapViewMax = new Prop<float>(1.0f);
        public Prop<float> MapSpeedMin = new Prop<float>(1.0f);
        public Prop<float> MapSpeedMax = new Prop<float>(1.0f);

        public Prop<string> HealthType = new Prop<string>("person");
        public Prop<float>  MaxHealth = new Prop<float>(100.0f);

        // Default animation for soldier classes seems to be hardcoded to "human".
        // For example, there's no "AnimationName" anywhere in the odf hierarchy:
        //   rep_inf_ep3_rifleman -> rep_inf_default_rifleman -> rep_inf_default -> com_inf_default
        public Prop<string> AnimationName = new Prop<string>("human");
        public Prop<string> SkeletonName = new Prop<string>("human");

        public Prop<float> MaxSpeed = new Prop<float>(1.0f);
        public Prop<float> MaxStrafeSpeed = new Prop<float>(1.0f);
        public Prop<float> MaxTurnSpeed = new Prop<float>(1.0f);
        public Prop<float> JumpHeight = new Prop<float>(1.0f);
        public Prop<float> JumpForwardSpeedFactor = new Prop<float>(1.0f);
        public Prop<float> JumpStrafeSpeedFactor = new Prop<float>(1.0f);
        public Prop<float> RollSpeedFactor = new Prop<float>(1.0f);
        public Prop<float> Acceleration = new Prop<float>(1.0f);
        public Prop<float> SprintAccelerateTime = new Prop<float>(1.0f);

        public MultiProp ControlSpeed = new MultiProp(typeof(string), typeof(float), typeof(float), typeof(float));

        public Prop<float> EnergyBar = new Prop<float>(1.0f);
        public Prop<float> EnergyRestore = new Prop<float>(1.0f);
        public Prop<float> EnergyRestoreIdle = new Prop<float>(1.0f);
        public Prop<float> EnergyDrainSprint = new Prop<float>(1.0f);
        public Prop<float> EnergyMinSprint = new Prop<float>(1.0f);
        public Prop<float> EnergyCostJump = new Prop<float>(0.0f);
        public Prop<float> EnergyCostRoll = new Prop<float>(1.0f);

        public Prop<float> AimValue = new Prop<float>(1.0f);
        public Prop<float> AimFactorPostureSpecial = new Prop<float>(1.0f);
        public Prop<float> AimFactorPostureStand = new Prop<float>(1.0f);
        public Prop<float> AimFactorPostureCrouch = new Prop<float>(1.0f);
        public Prop<float> AimFactorPostureProne = new Prop<float>(1.0f);
        public Prop<float> AimFactorStrafe = new Prop<float>(0.0f);
        public Prop<float> AimFactorMove = new Prop<float>(1.0f);

        public Prop<string> AISizeType = new Prop<string>("SOLDIER");

        public MultiProp WeaponName = new MultiProp(typeof(string));
    }

    enum ControlState
    {
        Stand,
        Crouch,
        Prone,
        Sprint,
        Jet,
        Jump,
        Roll,
        Tumble
    }


    public Prop<float> CurHealth = new Prop<float>(100.0f);

    public PawnController Controller;
    Transform HpWeapons;
    Animator Anim;
    Avatar Ava;
    Rigidbody Body;

    ControlState State;

    // Physical raycast downwards
    bool Grounded;
    bool PrevGrounded;

    // how long to still be alerted after the last fire / hit
    const float AlertTime = 3f;
    float AlertTimer;

    // Time we have to fail the raycast to be considered falling
    const float FallTime = 0.5f;
    float FallTimer;

    // time when we start playing the full fall animation
    const float FallAnimTime = 2f;
    float FallAnimTimer;

    // minimum time we're considered falling when jumping
    const float JumpTime = 0.2f;
    float JumpTimer;

    // let's not depend on the animator state animation
    // and measure it ourselfs
    const float LandTime = 0.5f;
    float LandTimer;

    Vector3 CurrSpeed;
    Quaternion CurrDir;
    Quaternion TargetDir;


    bool bHasLookaroundIdleAnim = false;
    bool bHasCheckweaponIdleAnim = false;
    bool LastIdle = false;
    const float IdleTime = 10f;

    string[] IdleNames = new string[]
    {
        "IdleLookaround",
        "IdleCheckweapon"
    };

    // <stance>, <thrustfactor> <strafefactor> <turnfactor>
    float[][] ControlValues;


    public override void Init()
    {
        CurrDir = transform.rotation;
        TargetDir = CurrDir;

        ControlState[] states = (ControlState[])Enum.GetValues(typeof(ControlState));
        ControlValues = new float[states.Length][];
        for (int i = 0; i < states.Length; ++i)
        {
            ControlValues[i] = GetControlSpeed(states[i]);
        }

        HpWeapons = transform.Find("dummyroot/bone_root/bone_a_spine/bone_b_spine/bone_ribcage/bone_r_clavicle/bone_r_upperarm/bone_r_forearm/bone_r_hand/hp_weapons");

        Body = gameObject.AddComponent<Rigidbody>();
        Body.mass = 80f;
        Body.drag = 0f;
        Body.angularDrag = 1f;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        CapsuleCollider coll = gameObject.AddComponent<CapsuleCollider>();
        coll.height = 2f;
        coll.radius = 0.5f;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Animation
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Ava = AvatarBuilder.BuildGenericAvatar(gameObject, "dummyroot");
        Anim = gameObject.AddComponent<Animator>();
        Anim.avatar = Ava;
        Anim.applyRootMotion = false;
        RuntimeAnimatorController runtimeAnimController = Resources.Load<RuntimeAnimatorController>(ANIM_CONTROLLER_NAME);
        if (runtimeAnimController == null)
        {
            Debug.LogError($"Could not get runtime animator controller '{ANIM_CONTROLLER_NAME}'!");
            return;
        }

        AnimatorOverrideController animController = new AnimatorOverrideController(runtimeAnimController);
        Anim.runtimeAnimatorController = animController;
        Anim.cullingMode = AnimatorCullingMode.CullCompletely;

        int numAnims = animController.animationClips.Length;
        var overrides = new KeyValuePair<AnimationClip, AnimationClip>[numAnims];

        for (int i = 0; i < numAnims; ++i)
        {
            AnimationClip src = animController.animationClips[i];
            if (src == null)
            {
                Debug.LogError($"Found unassigned AnimationClip in AnimationController '{ANIM_CONTROLLER_NAME}'!");
                continue;
            }

            string weaponAnimBankName = "rifle"; // TODO
            AnimationClip dst = GetClip(C.SkeletonName, weaponAnimBankName, src.name, "human_0", "tool");
            if (dst == null)
            {
                if (src.name != "stand_idle_lookaround" && src.name != "stand_idle_checkweapon")
                {
                    Debug.LogError($"Cannot find Animation '{src.name}' in AnimationBanks '{C.SkeletonName}' / '{C.AnimationName}'!");
                }
                continue;
            }

            bHasLookaroundIdleAnim  = bHasLookaroundIdleAnim  || src.name == "stand_idle_lookaround";
            bHasCheckweaponIdleAnim = bHasCheckweaponIdleAnim || src.name == "stand_idle_checkweapon";

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(src, dst);
        }
        
        animController.ApplyOverrides(overrides);
    }

    public override void BindEvents()
    {
        
    }

    public void AddHealth(float amount)
    {
        CurHealth.Set(Mathf.Clamp(CurHealth + amount, 0f, C.MaxHealth));
    }

    public void AddAmmo(float amount)
    {
        // TODO
    }

    public void PlayIntroAnim()
    {
        if (bHasLookaroundIdleAnim)
        {
            Anim.SetTrigger("IdleLookaround");
        }
        else
        {
            Anim.SetTrigger("Reload");
        }
    }

    // see: com_inf_default
    float[] GetControlSpeed(ControlState state)
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

    AnimationClip GetClip(string bankName, string weapName, string animName, string fallbackBankName=null, string fallbackWeaponName=null)
    {
        // TODO: There's AnimationName and SkeletonName. Sometimes they mean the same thing !?
        // For example: The super battle droid just has SkeletonName property, but no AnimationName property...
        string fullName = $"{C.SkeletonName}_{weapName}_{animName}";

        uint animCRC = HashUtils.GetCRC(fullName);
        AnimationClip clip = AnimationLoader.Instance.LoadAnimationClip(bankName, animCRC, transform, false, false);
        if (clip == null)
        {
            animCRC = HashUtils.GetCRC(fullName + "_full");
            clip = AnimationLoader.Instance.LoadAnimationClip(bankName, animCRC, transform, false, false);
            if (clip != null) return clip;

            if (fallbackBankName == "human_0")
            {
                clip = GetClip("human_0", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;

                clip = GetClip("human_1", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;

                clip = GetClip("human_2", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;

                clip = GetClip("human_3", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;

                clip = GetClip("human_4", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;

                clip = GetClip("human_sabre", weapName, animName, null, fallbackWeaponName);
                if (clip != null) return clip;
            }

            if (!string.IsNullOrEmpty(fallbackWeaponName))
            {
                clip = GetClip(bankName, fallbackWeaponName, animName, fallbackBankName, null);
                if (clip != null) return clip;
            }
        }
        return clip;
    }

    void Update()
    {
        AlertTimer = Mathf.Max(AlertTimer - Time.deltaTime, 0f);

        if (Controller != null)
        {
            if (Grounded)
            {
                // Stand - Crouch - Sprint
                if (State == ControlState.Stand || State == ControlState.Crouch || State == ControlState.Sprint)
                {
                    // ---------------------------------------------------------------------------------------------
                    // Forward
                    // ---------------------------------------------------------------------------------------------
                    float walk = Controller.WalkDirection.magnitude;
                    if (Controller.WalkDirection.y <= 0f)
                    {
                        walk = -walk;
                    }
                    Anim.SetFloat("Forward", walk);
                    // ---------------------------------------------------------------------------------------------
                }

                // Stand - Crouch
                if (State == ControlState.Stand || State == ControlState.Crouch)
                {
                    // ---------------------------------------------------------------------------------------------
                    // Idle
                    // ---------------------------------------------------------------------------------------------
                    if (Controller.IdleTime >= IdleTime)
                    {
                        if (bHasLookaroundIdleAnim && !bHasCheckweaponIdleAnim)
                        {
                            Anim.SetTrigger(IdleNames[0]);
                        }
                        else if (!bHasLookaroundIdleAnim && bHasCheckweaponIdleAnim)
                        {
                            Anim.SetTrigger(IdleNames[1]);
                        }
                        else if (bHasLookaroundIdleAnim && bHasCheckweaponIdleAnim)
                        {
                            Anim.SetTrigger(IdleNames[UnityEngine.Random.Range(0, 1)]);
                        }
                        Controller.ResetIdleTime();
                    }
                    if (!Controller.IsIdle && LastIdle)
                    {
                        Anim.SetTrigger("UnIdle");
                    }
                    // ---------------------------------------------------------------------------------------------


                    // ---------------------------------------------------------------------------------------------
                    // Shooting
                    // ---------------------------------------------------------------------------------------------
                    if (Controller.ShootPrimary)
                    {
                        Anim.SetTrigger("ShootPrimary");
                        AlertTimer = AlertTime;
                    }
                    else if (Controller.ShootSecondary)
                    {
                        Anim.SetTrigger("ShootSecondary");
                        AlertTimer = AlertTime;
                    }
                    else if (Controller.Reload)
                    {
                        Anim.SetTrigger("Reload");
                    }
                    // ---------------------------------------------------------------------------------------------


                    State = Controller.Crouch ? ControlState.Crouch : ControlState.Stand;
                }

                // Stand - Sprint
                if (State == ControlState.Stand || State == ControlState.Sprint)
                {
                    // ---------------------------------------------------------------------------------------------
                    // Jumping
                    // ---------------------------------------------------------------------------------------------
                    if (Controller.Jump)
                    {
                        Body.AddForce(Vector3.up * Mathf.Sqrt(C.JumpHeight * -2f * Physics.gravity.y) + CurrSpeed, ForceMode.VelocityChange);
                        State = ControlState.Jump;
                        JumpTimer = JumpTime;
                    }
                    // ---------------------------------------------------------------------------------------------
                }

                // Stand
                if (State == ControlState.Stand)
                {
                    if (Controller.WalkDirection.y > 0.2f && Controller.Sprint)
                    {
                        State = ControlState.Sprint;
                    }
                }

                // Crouch
                if (State == ControlState.Crouch)
                {
                    if (Controller.Jump)
                    {
                        State = ControlState.Stand;
                    }

                    // TODO: verify
                    else if (Controller.WalkDirection.y > 0.2f && Controller.Sprint)
                    {
                        State = ControlState.Sprint;
                    }
                }

                // Sprint
                if (State == ControlState.Sprint)
                {
                    if (Controller.WalkDirection.y < 0.8f || !Controller.Sprint)
                    {
                        State = ControlState.Stand;
                    }
                }

                // Jump
                if (State == ControlState.Jump)
                {
                    JumpTimer -= Time.deltaTime;
                    if (Grounded && JumpTimer < 0f)
                    {
                        State = ControlState.Stand;
                    }
                }

                Anim.SetBool("Alert", AlertTimer > 0f);
                LastIdle = Controller.IsIdle;

                FallAnimTimer = 0f;
            }
            else
            {
                FallAnimTimer += Time.deltaTime;
            }

            Anim.SetInteger("State", (int)State);
            Anim.SetBool("Falling", FallAnimTimer >= FallAnimTime);
        }
    }

    void FixedUpdate()
    {
        Grounded = Physics.CheckSphere(transform.position, 0.4f, GameRuntime.PlayerMask, QueryTriggerInteraction.Ignore);

        if (!PrevGrounded && Grounded)
        {
            LandTimer = LandTime;
            FallTimer = 0f;
        }
        else
        {
            LandTimer -= Time.fixedDeltaTime;
        }
        PrevGrounded = Grounded;

        if (Controller != null)
        {
            Vector3 lookWalkForward = Controller.LookingAt - Body.position;
            lookWalkForward.y = 0f;
            Vector3 moveDirLocal = new Vector3(Controller.WalkDirection.x, 0f, Controller.WalkDirection.y);
            Vector3 moveDirWorld = CurrDir * moveDirLocal;

            float accStep      = C.Acceleration * Time.fixedDeltaTime;
            float thrustFactor = ControlValues[(int)State][0];
            float strafeFactor = ControlValues[(int)State][1];
            float turnFactor   = ControlValues[(int)State][2];

            if (moveDirLocal.magnitude == 0f)
            {
                CurrSpeed *= 0.1f * Time.fixedDeltaTime;
            }
            else
            {
                CurrSpeed += moveDirWorld * accStep;

                float forwardFactor = moveDirLocal.z < 0.2f ? strafeFactor : thrustFactor;
                CurrSpeed = Vector3.ClampMagnitude(CurrSpeed, C.MaxSpeed * forwardFactor);
            }

            if (moveDirLocal.magnitude > 0f)
            {
                if (moveDirLocal.z <= 0f)
                {
                    moveDirLocal = -moveDirLocal;
                    moveDirWorld = CurrDir * moveDirLocal;
                }
            }
            else
            {
                moveDirWorld = transform.forward;
            }

            TargetDir = Quaternion.LookRotation(moveDirWorld);

            float angleDiff = Quaternion.Angle(CurrDir, TargetDir);
            float t = Mathf.Clamp01(C.MaxTurnSpeed * turnFactor / angleDiff);
            CurrDir = Quaternion.Slerp(CurrDir, TargetDir, t);
        }

        Body.MoveRotation(CurrDir);
        Body.MovePosition(Body.position + CurrSpeed * Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        if ((State == ControlState.Stand || State == ControlState.Crouch) && AlertTimer > 0f)
        {
            Transform spine = transform.Find("dummyroot/bone_root/bone_a_spine");
            if (spine == null)
            {
                Debug.LogError("Cannot find spine bone!");
                return;
            }

            Vector3 viewDir = (Controller.LookingAt - Body.position).normalized;
            spine.rotation = Quaternion.LookRotation(viewDir) * Quaternion.Euler(0f, -60f, -90f);

            if (GameRuntime.Instance.AimDebug != null)
            {
                GameRuntime.Instance.AimDebug.SetPosition(0, HpWeapons.position);
                GameRuntime.Instance.AimDebug.SetPosition(1, Controller.LookingAt);
            }
        }
    }
}
