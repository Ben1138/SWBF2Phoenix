/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

[RequireComponent(typeof(Rigidbody)]
public class PhxChunk : MonoBehaviour
{
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    Rigidbody Body;


    int NumTerrainCollisions, CurrTerrainCollisions;

    string ChunkTerrainEffect;
    PhxEffect ChunkTrailEffect;

    PhxEffect ChunkSmokeEffect;
    Transform ChunkSmokeNode;

    float ChunkSpeed, ChunkUpFactor;


    public void Init(Dictionary<string, IPhxPropRef> ChunkProperties)
    {
        if (ChunkProperties.TryGetValue("ChunkTerrainCollisions", out IPhxPropRef NumTerrColls))
        {
            NumTerrainCollisions = ((PhxProp<float>) NumTerrColls).Get();
        }

        if (ChunkProperties.TryGetValue("ChunkTerrainEffect", out IPhxPropRef ChunkTerrEff))
        {
            ChunkTerrainEffect = ((PhxProp<string>) ChunkTerrEff).Get();
        }

    }


    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("TerrainAll"))
        {
            CurrTerrainCollisions++;

            ContactPoint Point = coll.GetContact(0);

            Vector3 Pos = Point.point;
            Quaternion Rot = Quaternion.LookRotation(Point.normal, Vector3.up);

            SCENE.EffectsManager.PlayEffectOnce(ChunkTerrainEffect, Pos, Rot);
        }



        if (CurrTerrainCollisions >= NumTerrainCollisions)
        {
            Body.enabled = false;
        }
    }



    void Awake()
    {
        Body = GetComponent<Rigidbody>();
    }
}
*/
