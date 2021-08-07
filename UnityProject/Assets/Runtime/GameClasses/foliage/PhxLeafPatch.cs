
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;



public class PhxLeafPatchClass : PhxClass 
{
    public PhxProp<float> MinSize = new PhxProp<float>(1.6f);
    public PhxProp<float> MaxSize = new PhxProp<float>(1.6f);
    
    public PhxProp<Vector3> Offset = new PhxProp<Vector3>(Vector3.zero);
    
    public PhxProp<float> Alpha = new PhxProp<float>(1.0f);

    public PhxProp<int> NumParticles = new PhxProp<int>(40);
    public PhxProp<float> MaxDistance = new PhxProp<float>(30f);

    public PhxProp<Texture2D> Texture = new PhxProp<Texture2D>(null);

    public PhxProp<float> Radius = new PhxProp<float>(2.0f);
    public PhxProp<float> Height = new PhxProp<float>(1.5f);

    public PhxProp<float> DarknessMin = new PhxProp<float>(0.0f);
    public PhxProp<float> DarknessMax = new PhxProp<float>(0.0f);

    public PhxProp<int> MaxScatterBirds = new PhxProp<int>(0);
}



// TODO: Custom leafpatch shader
[RequireComponent(typeof(ParticleSystem))]
public class PhxLeafPatch : PhxInstance<PhxLeafPatchClass>
{
    ParticleSystem Leaves;

    bool UseHDRP = false;

    // Will also need trigger coll and timer for emitting birds/falling leaves.


    public override void Init()
    {   
        Leaves = GetComponent<ParticleSystem>();
        Leaves.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer LeavesRenderer = GetComponent<ParticleSystemRenderer>();

        transform.localRotation = Quaternion.identity;
        transform.localPosition = C.Offset.Get();


        var LeavesEmission = Leaves.emission;
        LeavesEmission.enabled = false;

        var LeavesShape = Leaves.shape;
        LeavesShape.enabled = false;

        var LeavesMainModule = Leaves.main;
        LeavesMainModule.startSpeed = 0f;
        LeavesMainModule.startLifetime = 1f;
        LeavesMainModule.startSize = new ParticleSystem.MinMaxCurve(C.MinSize, C.MaxSize);
        LeavesMainModule.maxParticles = C.NumParticles;
        LeavesMainModule.simulationSpace = ParticleSystemSimulationSpace.Local;

        Color MinColor = new Color(C.DarknessMin, C.DarknessMin, C.DarknessMin, C.Alpha);
        Color MaxColor = new Color(C.DarknessMax, C.DarknessMax, C.DarknessMax, C.Alpha);
        LeavesMainModule.startColor = new ParticleSystem.MinMaxGradient(MinColor, MaxColor);

        //var colModule = Leaves.colorOverLifetime;
        //colModule.enabled = true;
        //colModule.color = new ParticleSystem.MinMaxGradient(MinColor, MaxColor);

        LeavesRenderer.alignment = ParticleSystemRenderSpace.View;

        if (C.Texture.Get() == null)
        {
            LeavesRenderer.enabled = false;
        }
        else 
        {
            Material mat = null;
            if (!UseHDRP)
            {
                mat = new Material(Resources.Load<Material>("effects/ParticleNormal"));
                mat.mainTexture = C.Texture.Get();               
            }
            else 
            {
                mat = new Material(Resources.Load<Material>("effects/HDRPParticleNormal"));
                var mainTexID = Shader.PropertyToID("Texture2D_23DD87FD");
                mat.SetTexture(mainTexID, C.Texture.Get());                    
            }

            LeavesRenderer.sharedMaterial = mat;
        }

        byte ByteAlpha = (byte) (255f * C.Alpha);

        for (int j = 0; j < C.NumParticles; j++)
        {
            byte ByteDarkness = (byte) (255f * UnityEngine.Random.Range(C.DarknessMin, C.DarknessMax));

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.startColor = new Color32(ByteDarkness, ByteDarkness, ByteDarkness, ByteAlpha);
            emitParams.startSize = UnityEngine.Random.Range(C.MinSize, C.MaxSize);
            Leaves.Emit(emitParams, 1);            
        }

        ParticleSystem.Particle[] LeafParticles = new ParticleSystem.Particle[C.NumParticles];
        int NumParticles = Leaves.GetParticles(LeafParticles);
        for (int i = 0; i < NumParticles; i++)
        {
            LeafParticles[i].position = new Vector3(
                UnityEngine.Random.Range(-C.Radius, C.Radius), 
                UnityEngine.Random.Range(-C.Height, C.Height), 
                0f
            );

            //byte ByteDarkness = (byte) (255f * UnityEngine.Random.Range(C.DarknessMin, C.DarknessMax));
            //LeafParticles[i].startColor = new Color32(ByteDarkness, ByteDarkness, ByteDarkness, ByteAlpha);
        }

        Leaves.SetParticles(LeafParticles);  

        Leaves.Emit(C.NumParticles);

        Leaves.Pause();
    }

    public override void BindEvents(){}
    public override void Tick(float deltaTime){}
    public override void TickPhysics(float deltaTime){}
}
