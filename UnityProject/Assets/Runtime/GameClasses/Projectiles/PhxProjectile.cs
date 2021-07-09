using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class PhxBolt : PhxClass
{
    public PhxProp<Texture2D> LaserTexture = new PhxProp<Texture2D>(null);
    public PhxProp<Color> LaserGlowColor = new PhxProp<Color>(Color.white);
    public PhxProp<Color> LightColor = new PhxProp<Color>(Color.black);
    public PhxProp<float> LightRadius = new PhxProp<float>(1f);
    public PhxProp<float> LaserLength = new PhxProp<float>(1f);
    public PhxProp<float> LaserWidth = new PhxProp<float>(1f);
    public PhxProp<float> Velocity = new PhxProp<float>(1f);
    public PhxProp<float> LifeSpan = new PhxProp<float>(1f);

    public PhxProp<float> MaxDamage = new PhxProp<float>(1f);
}


[RequireComponent(typeof(LineRenderer), typeof(Rigidbody), typeof(Light))]
public class PhxProjectile : MonoBehaviour
{
    public BoxCollider Coll { get; private set; }
    public Action<PhxProjectile, Collision> OnHit;

    float EmissionIntensity = 25f;

    PhxPawnController Owner;
    Rigidbody Body;
    HDAdditionalLightData Light;
    LineRenderer Renderer;

    public void Setup(PhxPawnController owner, Vector3 pos, Quaternion rot, PhxBolt bolt)
    {
        Owner = owner;

        Body.transform.position = pos;
        Body.transform.rotation = rot;
        Body.velocity = Body.transform.forward * bolt.Velocity;

        Light.color = bolt.LightColor;

        Renderer.startWidth = bolt.LaserWidth;
        Renderer.endWidth = bolt.LaserWidth;
        Renderer.SetPosition(1, new Vector3(0f, 0f, bolt.LaserLength * 2f));
        Renderer.material.SetTexture("_UnlitColorMap", bolt.LaserTexture);
        Renderer.material.SetTexture("_EmissiveColorMap", bolt.LaserTexture);
        Renderer.material.SetColor("_EmissiveColor", bolt.LightColor.Get() * EmissionIntensity);
    }

    void Awake()
    {
        EmissionIntensity = Mathf.Pow(2f, 20f);

        Coll = GetComponent<BoxCollider>();
        Body = GetComponent<Rigidbody>();
        Light = GetComponent<HDAdditionalLightData>();
        Renderer = GetComponent<LineRenderer>();
    }

    void OnCollisionEnter(Collision coll)
    {
        //Debug.Log($"Projectile '{name}' hit '{coll.collider.name}'!");
        OnHit?.Invoke(this, coll);
    }
}
