using System.Collections.Generic;
using UnityEngine;

using LibSWBF2.Utils;



public class PhxExplosionClass : PhxClass 
{
    public PhxProp<float> Damage = new PhxProp<float>(400f);
    public PhxProp<float> DamageRadiusInner = new PhxProp<float>(1f);
    public PhxProp<float> DamageRadiusOuter = new PhxProp<float>(3f);

    public PhxProp<float> Push = new PhxProp<float>(10f);
    public PhxProp<float> PushRadiusInner = new PhxProp<float>(4f);
    public PhxProp<float> PushRadiusOuter = new PhxProp<float>(4f);

    public PhxProp<float> Shake = new PhxProp<float>(.5f);
    public PhxProp<float> ShakeLength = new PhxProp<float>(.6f);
    public PhxProp<float> ShakeRadiusInner = new PhxProp<float>(8f);
    public PhxProp<float> ShakeRadiusOuter = new PhxProp<float>(12f);

    public PhxProp<string> Effect = new PhxProp<string>(null);
    public PhxProp<string> WaterEffect = new PhxProp<string>(null);

    public PhxProp<float> LifeSpan = new PhxProp<float>(3f);

    public PhxProp<float> VehicleScale = new PhxProp<float>(0f);
    public PhxProp<float> PersonScale = new PhxProp<float>(1f);
    public PhxProp<float> DroidScale = new PhxProp<float>(1f);
    public PhxProp<float> AnimalScale = new PhxProp<float>(1f);
    public PhxProp<float> BuildingScale = new PhxProp<float>(.25f);

    public PhxProp<Color> LightColor = new PhxProp<Color>(Color.white);
    public PhxProp<float> LightRadius = new PhxProp<float>(7f);
    public PhxProp<float> LightDuration = new PhxProp<float>(1f);    
}



public static class PhxExplosionManager
{
    static PhxGame Game => PhxGame.Instance;
    static PhxScene Scene => PhxGame.GetScene();


    public static void AddExplosion(PhxPawnController Originator, PhxExplosionClass Exp, Vector3 Position, Quaternion Rotation)
    {
        if (Exp == null)
            return;


        // Play effect
        Scene.EffectsManager.PlayEffectOnce(Exp.Effect.Get(), Position, Rotation);

        // Get max of DamageRadiusOuter, ShakeRadiusOuter, and PushRadiusOuter as 
        // sphere cast radius.

        // Sphere cast on vehicles | soldiers | statics

        // Apply damage, push, credit Originator, etc
    }
}
