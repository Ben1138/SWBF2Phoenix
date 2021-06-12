using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using LibSWBF2.Wrappers;

public class PhxCommandpost : PhxInstance<PhxCommandpost.ClassProperties>
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<float> NeutralizeTime = new PhxProp<float>(1.0f);
        public PhxProp<float> CaptureTime    = new PhxProp<float>(1.0f);
        public PhxProp<float> HoloTurnOnTime = new PhxProp<float>(1.0f);
        public PhxMultiProp   ChargeSound    = new PhxMultiProp(typeof(AudioClip), typeof(string));
        public PhxMultiProp   CapturedSound  = new PhxMultiProp(typeof(AudioClip), typeof(string));
        public PhxMultiProp   DischargeSound = new PhxMultiProp(typeof(AudioClip), typeof(string));
        public PhxMultiProp   LostSound      = new PhxMultiProp(typeof(AudioClip), typeof(string));

        public PhxProp<Texture2D> MapTexture = new PhxProp<Texture2D>(null);
        public PhxProp<float>     MapScale   = new PhxProp<float>(1.0f);
    }

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject   HoloIcon;
    public HDAdditionalLightData Light;

    // SWBF Instance Properties
    public PhxProp<PhxRegion> CaptureRegion = new PhxProp<PhxRegion>(null);
    public PhxProp<PhxRegion> ControlRegion = new PhxProp<PhxRegion>(null);
    public PhxProp<SWBFPath>  SpawnPath     = new PhxProp<SWBFPath>(null);
    public PhxProp<int>       Team          = new PhxProp<int>(0);

    AudioSource AudioAction;
    AudioSource AudioAmbient;
    AudioSource AudioCapture;

    [Header("Settings")]
    public Vector2 CapturePitch = new Vector2(0.5f, 1.5f);
    public float   CaptureTimer;
    public int     CaptureCount;
    public float   HoloPresenceSpeed = 1.0f;

    // cache
    bool bInitInstance => C != null;
    float HoloWidthStart;
    float HoloWidthEnd;
    float LightIntensity;
    Color HoloColor;
    float HoloAlpha;
    float HoloPresence = 1.0f;
    float HoloPresenceDest = 1.0f;
    float HoloPresenceVel;
    float LastHoloPresence;


    public override void BindEvents()
    {
        Team.OnValueChanged += ApplyTeam;
        CaptureRegion.OnValueChanged += (PhxRegion oldRegion) =>
        {
            if (oldRegion != null)
            {
                oldRegion.OnEnter -= OnCaptureRegionEnter;
                oldRegion.OnLeave -= OnCaptureRegionLeave;
            }

            PhxRegion newRegion = CaptureRegion.Get();
            if (newRegion != null)
            {
                newRegion.OnEnter += OnCaptureRegionEnter;
                newRegion.OnLeave += OnCaptureRegionLeave;
            }
        };
    }

    public override void Init()
    {
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
        AudioAmbient.maxDistance = 30.0f;
        AudioAmbient.Play();

        AudioCapture = gameObject.AddComponent<AudioSource>();
        AudioCapture.spatialBlend = 1.0f;
        AudioCapture.loop = true;
        AudioCapture.pitch = 1.0f;
        AudioCapture.volume = 0.8f;
        AudioCapture.rolloffMode = AudioRolloffMode.Linear;
        AudioCapture.minDistance = 2.0f;
        AudioCapture.maxDistance = 30.0f;

        AudioAction = gameObject.AddComponent<AudioSource>();
        AudioAction.spatialBlend = 1.0f;
        AudioAction.loop = false;
        AudioAction.pitch = 1.1f;
        AudioAction.volume = 0.5f;
        AudioAction.rolloffMode = AudioRolloffMode.Linear;
        AudioAction.minDistance = 2.0f;
        AudioAction.maxDistance = 30.0f;
    }

    void ApplyTeam(int oldTeam)
    {
        CaptureTimer = 0.0f;

        AudioCapture.clip = Team == 0 ? C.ChargeSound.Get<AudioClip>(0) : C.DischargeSound.Get<AudioClip>(0);
        AudioCapture.Play();

        AudioAmbient.loop = true;
        AudioAction.clip = Team == 0 ? C.LostSound.Get<AudioClip>(0) : C.CapturedSound.Get<AudioClip>(0);
        AudioAction.Play();

        HoloPresence = 0.0f;
        HoloPresenceDest = 1.0f;
        UpdateColor();
    }

    void OnCaptureRegionEnter(PhxInstance other)
    {
        //Debug.LogFormat("Capture Region '{0}' entered by '{1}'", CaptureRegion.Get().name, other.name);
    }

    void OnCaptureRegionLeave(PhxInstance other)
    {
        //Debug.LogFormat("Capture Region '{0}' exited by'{1}'", CaptureRegion.Get().name, other.name);
    }

    void UpdateColor()
    {
        HoloColor = PhxGameRuntime.GetMatch().GetTeamColor(Team);
        HoloRay?.material.SetColor("_EmissiveColor", HoloColor);
        if (Light != null)
        {
            Light.color = HoloColor;
        }
    }

    void Update()
    {
        if (!bInitInstance) return;

        if (CaptureCount > 0)
        {
            float progress;
            float captureMultiplier = Mathf.Sqrt(CaptureCount);
            CaptureTimer += Time.deltaTime * captureMultiplier;
            if (Team == 0)
            {
                if (C.CaptureTime - CaptureTimer <= HoloPresenceSpeed * captureMultiplier * 2f)
                {
                    HoloPresenceDest = 0.0f;
                }

                progress = CaptureTimer / C.CaptureTime;
                if (CaptureTimer >= C.CaptureTime)
                {
                    Team.Set(1);
                    progress = 0.0f;
                }
                AudioCapture.pitch = Mathf.Lerp(CapturePitch.x, CapturePitch.y, progress);
            }
            else
            {
                if (C.NeutralizeTime - CaptureTimer <= HoloPresenceSpeed * captureMultiplier * 2f)
                {
                    HoloPresenceDest = 0.0f;
                }

                progress = CaptureTimer / C.NeutralizeTime;
                if (CaptureTimer >= C.NeutralizeTime)
                {
                    Team.Set(0);
                    progress = 0.0f;
                }
                AudioCapture.pitch = Mathf.Lerp(CapturePitch.y, CapturePitch.x, progress);
            }
        }
        else
        {
            HoloPresenceDest = 1.0f;
            CaptureTimer = Mathf.Max(CaptureTimer - Time.deltaTime * 0.1f, 0.0f);
            AudioCapture.pitch = 0.0f;
        }

        HoloPresence = Mathf.SmoothDamp(HoloPresence, HoloPresenceDest, ref HoloPresenceVel, HoloPresenceSpeed);
        //HoloPresence = Mathf.Lerp(HoloPresence, HoloPresenceDest, Time.deltaTime * HoloPresenceSpeed);
        if (HoloPresence != LastHoloPresence)
        {
            HoloColor.a = Mathf.Lerp(0.0f, HoloAlpha, HoloPresence);
            if (Light != null)
            {
                Light.intensity = Mathf.Lerp(0.0f, LightIntensity, HoloPresence);
            }
            if (HoloRay != null)
            {
                HoloRay.startWidth = Mathf.Lerp(0.0f, HoloWidthStart, HoloPresence);
                HoloRay.endWidth   = Mathf.Lerp(0.0f, HoloWidthEnd, HoloPresence);
                HoloRay.material.SetColor("_UnlitColor", HoloColor);
            }
            LastHoloPresence = HoloPresence;
        }
    }
}
