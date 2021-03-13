using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using LibSWBF2.Wrappers;

using CL = ClassLoader;

public class GC_commandpost : ISWBFInstance
{
    public class ClassProperties : ISWBFClass
    {
        public Ref<float> NeutralizeTime = new Ref<float>(1.0f);
        public Ref<float> CaptureTime = new Ref<float>(1.0f);
        public Ref<float> HoloTurnOnTime = new Ref<float>(1.0f);
        public Ref<AudioClip> CapturedSound = new Ref<AudioClip>(null);
        public Ref<AudioClip> LostSound = new Ref<AudioClip>(null);
        public Ref<AudioClip> ChargeSound = new Ref<AudioClip>(null);
        public Ref<AudioClip> DischargeSound = new Ref<AudioClip>(null);

        public override void InitClass(EntityClass ec)
        {
            P.Register("NeutralizeTime", NeutralizeTime);
            P.Register("CaptureTime", CaptureTime);
            P.Register("HoloTurnOnTime", HoloTurnOnTime);
            P.Register("CapturedSound", CapturedSound);
            P.Register("LostSound", LostSound);
            P.Register("ChargeSound", ChargeSound);
            P.Register("DischargeSound", DischargeSound);

            CL.AssignProp(ec, "NeutralizeTime", NeutralizeTime);
            CL.AssignProp(ec, "CaptureTime", CaptureTime);
            CL.AssignProp(ec, "HoloTurnOnTime", HoloTurnOnTime);
            CL.AssignProp(ec, "CapturedSound", 0, CapturedSound);
            CL.AssignProp(ec, "LostSound", 0, LostSound);
            CL.AssignProp(ec, "ChargeSound", 0, ChargeSound);
            CL.AssignProp(ec, "DischargeSound", 0, DischargeSound);
        }
    }

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject HoloIcon;
    public HDAdditionalLightData Light;

    ClassProperties C;

    // SWBF Instance Properties
    Ref<Collider> CaptureRegion = new Ref<Collider>(null);
    Ref<Collider> ControlRegion = new Ref<Collider>(null);
    Ref<byte> Team = new Ref<byte>(0);

    AudioSource AudioAction;
    AudioSource AudioAmbient;
    AudioSource AudioCapture;
    Vector2 CapturePitch = new Vector2(0.5f, 1.5f);
    float CaptureTimer;

    // cache
    bool bInitInstance = false;
    float HoloWidthStart;
    float HoloWidthEnd;
    float LightIntensity;
    Color HoloColor;
    float HoloAlpha;


    public override void InitInstance(Instance inst, ISWBFClass classProperties)
    {
        C = (ClassProperties)classProperties;

        P.Register("CaptureRegion", CaptureRegion);
        P.Register("ControlRegion", ControlRegion);
        P.Register("Team", Team);

        CL.AssignProp(inst, "CaptureRegion", CaptureRegion);
        CL.AssignProp(inst, "ControlRegion", ControlRegion);
        CL.AssignProp(inst, "Team",          Team);

        Transform hpHolo = transform.Find("com_bldg_controlzone/hp_hologram");
        if (hpHolo != null)
        {
            GameObject holoPrefab = Resources.Load<GameObject>("cp_holo");
            GameObject holo = Instantiate(holoPrefab, hpHolo);
            HoloRay = holo.GetComponent<LineRenderer>();
            Light = holo.GetComponentInChildren<HDAdditionalLightData>();

            HoloWidthStart = HoloRay.startWidth;
            HoloWidthEnd = HoloRay.endWidth;
            HoloAlpha = HoloRay.material.GetColor("_UnlitColor").a;
            LightIntensity = Light.intensity;
        }

        AudioAmbient = gameObject.AddComponent<AudioSource>();
        AudioAmbient.spatialBlend = 1.0f;
        AudioAmbient.clip = SoundLoader.LoadSound("com_blg_commandpost2");
        AudioAmbient.loop = true;
        AudioAmbient.pitch = 1.0f;
        AudioAmbient.volume = 0.5f;
        AudioAmbient.rolloffMode = AudioRolloffMode.Linear;
        AudioAmbient.minDistance = 2.0f;
        AudioAmbient.maxDistance = 40.0f;
        AudioAmbient.Play();

        AudioCapture = gameObject.AddComponent<AudioSource>();
        AudioCapture.spatialBlend = 1.0f;
        AudioCapture.loop = true;
        AudioCapture.pitch = 1.0f;
        AudioCapture.volume = 0.8f;
        AudioCapture.rolloffMode = AudioRolloffMode.Linear;
        AudioCapture.minDistance = 2.0f;
        AudioCapture.maxDistance = 40.0f;

        AudioAction = gameObject.AddComponent<AudioSource>();
        AudioAction.spatialBlend = 1.0f;
        AudioAction.loop = false;
        AudioAction.pitch = 1.1f;
        AudioAction.volume = 0.5f;
        AudioAction.rolloffMode = AudioRolloffMode.Linear;
        AudioAction.minDistance = 2.0f;
        AudioAction.maxDistance = 40.0f;

        SetTeam(Team);
        bInitInstance = true;
    }

    public void SetTeam(byte teamId)
    {
        CaptureTimer = 0.0f;
        Team.Set(teamId);

        AudioCapture.clip = Team == 0 ? C.ChargeSound : C.DischargeSound;
        AudioCapture.Play();

        AudioAction.clip = Team == 0 ? C.LostSound : C.CapturedSound;
        AudioAction.Play();

        UpdateColor();
    }

    public void UpdateColor()
    {
        HoloColor = GameRuntime.Instance.GetTeamColor(Team);
        HoloRay.material.SetColor("_EmissiveColor", HoloColor);
        Light.color = HoloColor;
    }

    void Update()
    {
        return;




        if (!bInitInstance) return;

        CaptureTimer += Time.deltaTime;
        float progress;
        if (Team == 0)
        {
            progress = CaptureTimer / C.CaptureTime;
            if (CaptureTimer >= C.CaptureTime)
            {
                SetTeam(1);
            }
            AudioCapture.pitch = Mathf.Lerp(CapturePitch.x, CapturePitch.y, CaptureTimer / C.CaptureTime);
        }
        else
        {
            progress = CaptureTimer / C.NeutralizeTime;
            if (CaptureTimer >= C.NeutralizeTime)
            {
                SetTeam(0);
            }
            AudioCapture.pitch = Mathf.Lerp(CapturePitch.y, CapturePitch.x, CaptureTimer / C.CaptureTime);
        }

        float holoPresence = Mathf.Clamp01(Mathf.Sin(progress * Mathf.PI) * 3.0f);
        HoloRay.startWidth = Mathf.Lerp(0.0f, HoloWidthStart, holoPresence);
        HoloRay.endWidth   = Mathf.Lerp(0.0f, HoloWidthEnd,   holoPresence);
        Light.intensity    = Mathf.Lerp(0.0f, LightIntensity, holoPresence);

        HoloColor.a = Mathf.Lerp(0.0f, HoloAlpha, holoPresence);
        HoloRay.material.SetColor("_UnlitColor", HoloColor);
    }
}
