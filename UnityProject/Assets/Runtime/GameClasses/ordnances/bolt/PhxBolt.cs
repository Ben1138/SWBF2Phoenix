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
    public PhxProp<float> LaserWidth = new PhxProp<float>(.05f);
    public PhxProp<float> Velocity = new PhxProp<float>(1f);

    public PhxProp<string> ImpactEffectStatic = new PhxProp<string>(null);
    public PhxProp<string> ImpactEffectRigid = new PhxProp<string>(null);
    public PhxProp<string> ImpactEffectSoft = new PhxProp<string>(null);
    public PhxProp<string> ImpactEffectTerrain = new PhxProp<string>(null);
    public PhxProp<string> ImpactEffectWater = new PhxProp<string>(null);
    public PhxProp<string> ImpactEffectShield = new PhxProp<string>(null);

    public PhxProp<PhxClass> ExplosionName = new PhxProp<PhxClass>(null);

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
        if (gameObject.activeSelf)
        {
            OnHit?.Invoke(this, coll);

            ContactPoint Point = coll.GetContact(0);

            if (coll.gameObject.layer == LayerMask.NameToLayer("TerrainAll"))
            {
                SCENE.EffectsManager.PlayEffectOnce(BoltClass.ImpactEffectTerrain.Get(), Point.point, Quaternion.identity);
            }
            else if (coll.gameObject.layer == LayerMask.NameToLayer("BuildingAll") ||
                    coll.gameObject.layer == LayerMask.NameToLayer("BuildingOrdnance"))
            {
                SCENE.EffectsManager.PlayEffectOnce(BoltClass.ImpactEffectStatic.Get(), Point.point, Quaternion.identity);
            }
            else if (coll.gameObject.layer == LayerMask.NameToLayer("SoldierAll"))
            {
                SCENE.EffectsManager.PlayEffectOnce(BoltClass.ImpactEffectSoft.Get(), Point.point, Quaternion.identity);
            }
            else 
            {
                SCENE.EffectsManager.PlayEffectOnce(BoltClass.ImpactEffectRigid.Get(), Point.point, Quaternion.identity);                
            }


            if (BoltClass.ExplosionName.Get() != null)
            {
                PhxExplosionManager.AddExplosion(null, BoltClass.ExplosionName.Get() as PhxExplosionClass, Point.point, Quaternion.identity);            
            }

    
            Release();
        }
    }
}
