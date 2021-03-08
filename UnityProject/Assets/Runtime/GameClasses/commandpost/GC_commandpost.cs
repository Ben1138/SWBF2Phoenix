using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

using CL = ClassLoader;

public class GC_commandpost : ISWBFGameClass
{
    [Header("SWBF Properties")]
    public float NeutralizeTime = 1.0f;
    public float CaptureTime = 1.0f;
    public float HoloTurnOnTime = 1.0f;
    public Collider CaptureRegion;
    public Collider ControlRegion;
    public byte TeamID = 0;
    public AudioClip CapturedSound;
    public AudioClip LostSound;

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject HoloIcon;

    AudioSource AudioAction;
    AudioSource AudioAmbient;

    public override void Init(Instance inst)
    {
        CL.AssignProp(inst, "NeutralizeTime",   ref NeutralizeTime);
        CL.AssignProp(inst, "CaptureTime",      ref CaptureTime);
        CL.AssignProp(inst, "HoloTurnOnTime",   ref HoloTurnOnTime);
        CL.AssignProp(inst, "CaptureRegion",    ref CaptureRegion);
        CL.AssignProp(inst, "ControlRegion",    ref ControlRegion);
        CL.AssignProp(inst, "CapturedSound", 0, ref CapturedSound);
        CL.AssignProp(inst, "LostSound",     0, ref LostSound);

        Transform hpHolo = transform.Find("com_bldg_controlzone/hp_hologram");
        if (hpHolo != null)
        {
            GameObject holoPrefab = Resources.Load<GameObject>("cp_holo");
            GameObject holo = Instantiate(holoPrefab, hpHolo);
            HoloRay = holo.GetComponent<LineRenderer>();
        }

        AudioAmbient = gameObject.AddComponent<AudioSource>();
        AudioAction = gameObject.AddComponent<AudioSource>();
        AudioAction.spatialBlend = 1.0f;
        AudioAction.clip = SoundLoader.LoadSound("com_blg_commandpost2");
        AudioAction.loop = true;
        AudioAction.pitch = 1.1f;
        AudioAction.volume = 0.5f;
        AudioAction.rolloffMode = AudioRolloffMode.Linear;
        AudioAction.minDistance = 2.0f;
        AudioAction.maxDistance = 40.0f;
        AudioAction.Play();

        AudioAction = gameObject.AddComponent<AudioSource>();
        AudioAction.spatialBlend = 1.0f;
        AudioAction.clip = CapturedSound;
        AudioAction.pitch = 1.1f;
        AudioAction.volume = 0.5f;
        AudioAction.rolloffMode = AudioRolloffMode.Linear;
        AudioAction.minDistance = 2.0f;
        AudioAction.maxDistance = 40.0f;
    }

    void Update()
    {
        
    }
}
