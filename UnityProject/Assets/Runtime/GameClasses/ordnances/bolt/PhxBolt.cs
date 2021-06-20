using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class PhxBoltClass : PhxOrdnanceClass
{
    public PhxProp<Texture2D> LaserTexture = new PhxProp<Texture2D>(null);
    public PhxProp<Color> LaserGlowColor = new PhxProp<Color>(Color.white);
    public PhxProp<Color> LightColor = new PhxProp<Color>(Color.black);
    public PhxProp<float> LightRadius = new PhxProp<float>(1f);
    public PhxProp<float> LaserLength = new PhxProp<float>(1f);
    public PhxProp<float> LaserWidth = new PhxProp<float>(1f);
    public PhxProp<float> Velocity = new PhxProp<float>(1f);

    public PhxProp<string> ImpactEffectStatic = new PhxProp<string>(null);
}


[RequireComponent(typeof(LineRenderer), typeof(Rigidbody), typeof(Light))]
public class PhxBolt : PhxOrdnance
{

    PhxBoltClass BoltClass;

    public BoxCollider Coll { get; private set; }
    public Action<PhxBolt, Collision> OnHit;

    float EmissionIntensity = 25f;

    Rigidbody Body;

    bool RenderHD = true;

    Light Light;
    HDAdditionalLightData HDLightData;

    LineRenderer Renderer;


    public override void Setup(IPhxWeapon Originator, Vector3 Pos, Quaternion Rot)
    {
        gameObject.SetActive(true);

        //Originator.GetFirePoint(out Vector3 Pos, out Quaternion Rot);
        
        Body.transform.position = Pos;
        Body.transform.rotation = Rot;
        Body.velocity = Body.transform.forward * BoltClass.Velocity;

        // Will need to unignore these!!!!
        foreach (Collider IgnoredCollider in Originator.GetIgnoredColliders())
        {
            Physics.IgnoreCollision(IgnoredCollider, Coll);
        }

        TimeAlive = 0f;
    }



    public override void Init(PhxOrdnanceClass OClass)
    {
        IsInitialized = true;

        BoltClass = OClass as PhxBoltClass;

        RenderHD = !(GraphicsSettings.renderPipelineAsset == null); 
        if (RenderHD)
        {
            HDLightData.color = BoltClass.LightColor;
        }
        else
        {
            Light.color = BoltClass.LightColor;
        }

        Renderer.startWidth = BoltClass.LaserWidth;
        Renderer.endWidth = BoltClass.LaserWidth;
        Renderer.SetPosition(1, new Vector3(0f, 0f, BoltClass.LaserLength * 2f));

        if (RenderHD)
        {
            Renderer.material.SetTexture("_UnlitColorMap", BoltClass.LaserTexture);
            Renderer.material.SetTexture("_EmissiveColorMap", BoltClass.LaserTexture);
            Renderer.material.SetColor("_EmissiveColor", BoltClass.LightColor.Get() * EmissionIntensity);
        }
        else 
        {
            Renderer.material.SetTexture("_MainTex", BoltClass.LaserTexture);
            Renderer.material.EnableKeyword("_EMISSION");
            Renderer.material.SetTexture("_EmissionMap", BoltClass.LaserTexture);
            //Renderer.material.SetColor("_EmissionColor", Color.white);
        }    
    }


    protected override void Release()
    {
        gameObject.SetActive(false);
    }



    void Awake()
    {
        EmissionIntensity = Mathf.Pow(2f, 20f);

        Coll = GetComponent<BoxCollider>();
        Body = GetComponent<Rigidbody>();
        Light = GetComponent<Light>();
        HDLightData = GetComponent<HDAdditionalLightData>();
        Renderer = GetComponent<LineRenderer>();

        gameObject.layer = LayerMask.NameToLayer("OrdnanceAll");
    }


    void OnCollisionEnter(Collision coll)
    {
        OnHit?.Invoke(this, coll);

        ContactPoint Point = coll.GetContact(0);

        Vector3 Pos = Point.point;
        Quaternion Rot = Quaternion.LookRotation(Point.normal, Vector3.up);

        SCENE.EffectsManager.PlayEffectOnce(BoltClass.ImpactEffectStatic.Get(), Pos, Rot);

        Release();
    }
}
