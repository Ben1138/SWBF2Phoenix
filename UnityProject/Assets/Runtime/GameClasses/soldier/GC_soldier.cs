using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class GC_soldier : ISWBFInstance<GC_soldier.ClassProperties>
{
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

    Prop<float> CurHealth = new Prop<float>(100.0f);


    public override void Init()
    {
        
    }

    public override void BindEvents()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
