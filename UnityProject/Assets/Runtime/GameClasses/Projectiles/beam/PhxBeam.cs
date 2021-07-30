using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;



[RequireComponent(typeof(LineRenderer), typeof(Light))]
public class PhxBeam : PhxOrdnance
{

    public class ClassProperties : PhxOrdnance.ClassProperties
    {
        public PhxProp<float> LaserWidth = new PhxProp<float>(5f);
        public PhxProp<Texture2D> LaserTexture = new PhxProp<Texture2D>(null);
        public PhxProp<Color> LightColor = new PhxProp<Color>(Color.white);

        public PhxProp<float> Range = new PhxProp<float>(100f);

        public PhxProp<float> Gravity = new PhxProp<float>(0f);
        public PhxProp<float> Rebound = new PhxProp<float>(0f);

        public PhxProp<bool> PassThrough = new PhxProp<bool>(false);
    }


    static int BeamMask;

    Transform BeamRoot;

    ClassProperties BeamClass;
   
    LineRenderer Renderer;

    Light Light;
    HDAdditionalLightData HDLightData;

    float EmissionIntensity = 25f;



    List<Collider> IgnoredColliders;
    List<int> IgnoredColliderLayers;


    public override void Init(PhxClass OClass)
    {
        BeamClass = OClass as ClassProperties;
        
        HDLightData.color = BeamClass.LightColor;
        
        Renderer.startWidth = BeamClass.LaserWidth;
        Renderer.endWidth = BeamClass.LaserWidth;

        Renderer.material.SetTexture("_UnlitColorMap", BeamClass.LaserTexture);
        Renderer.material.SetTexture("_EmissiveColorMap", BeamClass.LaserTexture);
        Renderer.material.SetColor("_EmissiveColor", BeamClass.LightColor.Get() * EmissionIntensity);

        IsInitialized = true;
    }


    public override void Setup(IPhxWeapon Originator)
    {
        gameObject.SetActive(true);

        Owner = Originator.GetOwnerController();
        OwnerWeapon = Originator;

        TimeAlive = 0f;

        BeamRoot = transform.parent;
        transform.SetParent(Originator.GetFirePoint());
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        IgnoredColliders = Originator.GetIgnoredColliders();
        IgnoredColliderLayers = new List<int>();

        foreach (Collider Coll in IgnoredColliders)
        {
            IgnoredColliderLayers.Add(Coll.gameObject.layer);
        }

    }


    protected override void Release()
    {
        Owner = null;
        OwnerWeapon = null;
        transform.SetParent(BeamRoot);
        gameObject.SetActive(false);

        IgnoredColliders = null;
        IgnoredColliderLayers = null;
    }


    void Update()
    {
        TimeAlive += Time.deltaTime;

        // Will eventually need to check if weapon is still firing
        if (TimeAlive > BeamClass.LifeSpan)
        {
            Release();
            return;
        }
        else 
        {
            // Probably need something more efficient
            if (IgnoredColliders != null)
            {
                foreach (Collider Coll in IgnoredColliders)
                {
                    Coll.gameObject.layer = 2;
                }
            }

            // Do raycast (how to ignore Originator's colliders??)
            // Todo: Stop only if not PassThrough, generalize layers
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, BeamClass.Range, BeamMask, QueryTriggerInteraction.Ignore))
            {
                // Extend linerender to hit
                Renderer.SetPosition(1, new Vector3(0f, 0f, hit.distance));
            }
            else 
            {
                // Extend linerender to full range
                Renderer.SetPosition(1, new Vector3(0f, 0f, BeamClass.Range));
            }

            // ''
            if (IgnoredColliders != null && IgnoredColliderLayers != null)
            {
                for (int i = 0; i < IgnoredColliders.Count; i++)
                {
                    IgnoredColliders[i].gameObject.layer = IgnoredColliderLayers[i];
                }
            }
        }
    }


    void Awake()
    {
        Light = GetComponent<Light>();
        HDLightData = GetComponent<HDAdditionalLightData>();
        
        EmissionIntensity = Mathf.Pow(2f, 20f);

        Renderer = GetComponent<LineRenderer>();

        BeamMask =  (1 << LayerMask.NameToLayer("SoldierAll")) |
                    (1 << LayerMask.NameToLayer("TerrainAll")) |
                    (1 << LayerMask.NameToLayer("VehicleAll")) |
                    (1 << LayerMask.NameToLayer("VehicleOrdnance")) | 
                    (1 << LayerMask.NameToLayer("BuildingAll")) |
                    (1 << LayerMask.NameToLayer("BuildingOrdnance"));
    }
}
