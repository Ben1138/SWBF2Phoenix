using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using LibSWBF2.Wrappers;

public class GC_commandpost : ISWBFInstance
{
    public class ClassProperties : ISWBFClass
    {
        public Ref<float>     NeutralizeTime = new Ref<float>(1.0f);
        public Ref<float>     CaptureTime    = new Ref<float>(1.0f);
        public Ref<float>     HoloTurnOnTime = new Ref<float>(1.0f);
        public Ref<AudioClip> CapturedSound  = new Ref<AudioClip>(null);
        public Ref<AudioClip> LostSound      = new Ref<AudioClip>(null);
        public Ref<AudioClip> ChargeSound    = new Ref<AudioClip>(null);
        public Ref<AudioClip> DischargeSound = new Ref<AudioClip>(null);

        public override void InitClass(EntityClass ec)
        {
            base.InitClass(ec);
            P.Register("NeutralizeTime", NeutralizeTime);
            P.Register("CaptureTime",    CaptureTime);
            P.Register("HoloTurnOnTime", HoloTurnOnTime);
            P.Register("CapturedSound",  CapturedSound);
            P.Register("LostSound",      LostSound);
            P.Register("ChargeSound",    ChargeSound);
            P.Register("DischargeSound", DischargeSound);

            RuntimeScene sc = GameRuntime.GetScene();
            sc.AssignProp(ec, "NeutralizeTime",    NeutralizeTime);
            sc.AssignProp(ec, "CaptureTime",       CaptureTime);
            sc.AssignProp(ec, "HoloTurnOnTime",    HoloTurnOnTime);
            sc.AssignProp(ec, "CapturedSound",  0, CapturedSound);
            sc.AssignProp(ec, "LostSound",      0, LostSound);
            sc.AssignProp(ec, "ChargeSound",    0, ChargeSound);
            sc.AssignProp(ec, "DischargeSound", 0, DischargeSound);
        }
    }

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject   HoloIcon;
    public HDAdditionalLightData Light;

    ClassProperties C;

    // SWBF Instance Properties
    Ref<Region> CaptureRegion = new Ref<Region>(null);
    Ref<Region> ControlRegion = new Ref<Region>(null);
    Ref<byte>   Team          = new Ref<byte>(0);

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

        Transform hpHolo = transform.Find(string.Format("{0}/hp_hologram", C.Name));
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
        AudioAmbient.pitch = 1.0f;
        AudioAmbient.volume = 0.5f;
        AudioAmbient.rolloffMode = AudioRolloffMode.Linear;
        AudioAmbient.minDistance = 2.0f;
        AudioAmbient.maxDistance = 20.0f;
        AudioAmbient.Play();

        AudioCapture = gameObject.AddComponent<AudioSource>();
        AudioCapture.spatialBlend = 1.0f;
        AudioCapture.loop = true;
        AudioCapture.pitch = 1.0f;
        AudioCapture.volume = 0.8f;
        AudioCapture.rolloffMode = AudioRolloffMode.Linear;
        AudioCapture.minDistance = 2.0f;
        AudioCapture.maxDistance = 20.0f;

        AudioAction = gameObject.AddComponent<AudioSource>();
        AudioAction.spatialBlend = 1.0f;
        AudioAction.loop = false;
        AudioAction.pitch = 1.1f;
        AudioAction.volume = 0.5f;
        AudioAction.rolloffMode = AudioRolloffMode.Linear;
        AudioAction.minDistance = 2.0f;
        AudioAction.maxDistance = 20.0f;


        P.Register("CaptureRegion", CaptureRegion);
        P.Register("ControlRegion", ControlRegion);
        P.Register("Team",          Team);

        Team.OnValueChanged += ApplyTeam;
        CaptureRegion.OnValueChanged += () => CaptureRegion.Get().OnEnter += OnCaptureRegionEnter;
        CaptureRegion.OnValueChanged += () => CaptureRegion.Get().OnLeave += OnCaptureRegionLeave;

        RuntimeScene sc = GameRuntime.GetScene();
        sc.AssignProp(inst, "CaptureRegion", CaptureRegion);
        sc.AssignProp(inst, "ControlRegion", ControlRegion);
        sc.AssignProp(inst, "Team",          Team);

        bInitInstance = true;
    }

    void ApplyTeam()
    {
        CaptureTimer = 0.0f;

        AudioCapture.clip = Team == 0 ? C.ChargeSound : C.DischargeSound;
        AudioCapture.Play();

        AudioAmbient.loop = true;
        AudioAction.clip = Team == 0 ? C.LostSound : C.CapturedSound;
        AudioAction.Play();

        UpdateColor();
    }

    void OnCaptureRegionEnter(GameObject other)
    {
        //Debug.LogFormat("Capture Region '{0}' entered by '{1}'", CaptureRegion.Get().name, other.name);
    }

    void OnCaptureRegionLeave(GameObject other)
    {
        //Debug.LogFormat("Capture Region '{0}' exited by'{1}'", CaptureRegion.Get().name, other.name);
    }

    void UpdateColor()
    {
        HoloColor = GameRuntime.Instance.GetTeamColor(Team);
        HoloRay.material.SetColor("_EmissiveColor", HoloColor);
        Light.color = HoloColor;
    }

    void Update()
    {
        if (!bInitInstance) return;

        CaptureTimer += Time.deltaTime;
        float progress;
        if (Team == 0)
        {
            progress = CaptureTimer / C.CaptureTime;
            if (CaptureTimer >= C.CaptureTime)
            {
                Team.Set(1);
            }
            AudioCapture.pitch = Mathf.Lerp(CapturePitch.x, CapturePitch.y, CaptureTimer / C.CaptureTime);
        }
        else
        {
            progress = CaptureTimer / C.NeutralizeTime;
            if (CaptureTimer >= C.NeutralizeTime)
            {
                Team.Set(0);
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
