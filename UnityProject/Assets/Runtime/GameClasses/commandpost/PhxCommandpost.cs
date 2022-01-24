using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using LibSWBF2.Wrappers;

public class PhxCommandpost : PhxInstance<PhxCommandpost.ClassProperties>, IPhxTickable
{
    PhxMatch Match => PhxGame.GetMatch();
    PhxScene Scene => PhxGame.GetScene();

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

    // SWBF Instance Properties
    public PhxProp<PhxRegion> CaptureRegion = new PhxProp<PhxRegion>(null);
    public PhxProp<PhxRegion> ControlRegion = new PhxProp<PhxRegion>(null);
    public PhxProp<SWBFPath>  SpawnPath     = new PhxProp<SWBFPath>(null);

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject   HoloIcon;
    public HDAdditionalLightData Light;

    [Header("Settings")]
    public Vector2 CapturePitch = new Vector2(0.5f, 1.5f);
    public float   HoloPresenceSpeed = 1.0f;


    public int   CaptureTeam { get; private set; }
    public bool  CaptureDisputed { get; private set; }
    public bool  CaptureToNeutral { get; private set; }

    public float CaptureTimer;
    int   CaptureCount;
    AudioSource AudioAction;
    AudioSource AudioAmbient;
    AudioSource AudioCapture;
    HashSet<PhxPawnController> CaptureControllers = new HashSet<PhxPawnController>();

    // cache
    bool  bInitInstance => C != null;
    float HoloWidthStart;
    float HoloWidthEnd;
    float LightIntensity;
    Color HoloColor;
    float HoloAlpha;
    float HoloPresence = 1.0f;
    float HoloPresenceDest = 1.0f;
    float HoloPresenceVel;
    float LastHoloPresence;

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


        Team.OnValueChanged += ApplyTeam;
        CaptureRegion.OnValueChanged += (PhxRegion oldRegion) =>
        {
            if (oldRegion != null)
            {
                oldRegion.OnEnter -= AddToCapture;
                oldRegion.OnLeave -= RemoveFromCapture;
            }

            PhxRegion newRegion = CaptureRegion.Get();
            if (newRegion != null)
            {
                newRegion.OnEnter += AddToCapture;
                newRegion.OnLeave += RemoveFromCapture;
            }
        };
    }

    public override void Destroy()
    {
        
    }

    public float GetCaptureProgress()
    {
        return CaptureTimer / C.CaptureTime;
    }    

    public void RemoveFromCapture(IPhxControlableInstance other)
    {
        PhxPawnController controller = other.GetController();
        if (controller != null)
        {
            CaptureControllers.Remove(controller);
            controller.CapturePost = null;
            RefreshCapture();
        }
    }

    public void UpdateColor()
    {
        HoloColor = PhxGame.GetMatch().GetTeamColor(Team);
        HoloRay?.material.SetColor("_EmissiveColor", HoloColor);
        if (Light != null)
        {
            Light.color = HoloColor;
        }
    }

    public void Tick(float deltaTime)
    {
        if (!bInitInstance) return;

        if (CaptureCount > 0)
        {
            if (CaptureDisputed)
            {
                // TODO: play dispute sound
            }
            else
            {
                float progress;
                float captureMultiplier = Mathf.Sqrt(CaptureCount);
                CaptureTimer += deltaTime * captureMultiplier;
                if (Team == 0)
                {
                    if (C.CaptureTime - CaptureTimer <= HoloPresenceSpeed * captureMultiplier * 2f)
                    {
                        HoloPresenceDest = 0.0f;
                    }

                    progress = CaptureTimer / C.CaptureTime;
                    if (CaptureTimer >= C.CaptureTime)
                    {
                        Team.Set(CaptureTeam);
                        progress = 0.0f;
                    }

                    CaptureToNeutral = false;
                    AudioCapture.pitch = Mathf.Lerp(CapturePitch.x, CapturePitch.y, progress);
                }
                else if (CaptureTeam != Team)
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

                    CaptureToNeutral = true;
                    AudioCapture.pitch = Mathf.Lerp(CapturePitch.y, CapturePitch.x, progress);
                }
            }
        }
        else
        {
            HoloPresenceDest = 1.0f;
            CaptureTimer = Mathf.Max(CaptureTimer - deltaTime * 0.1f, 0.0f);
            AudioCapture.pitch = 0.0f;
        }

        HoloPresence = Mathf.SmoothDamp(HoloPresence, HoloPresenceDest, ref HoloPresenceVel, HoloPresenceSpeed);
        //HoloPresence = Mathf.Lerp(HoloPresence, HoloPresenceDest, deltaTime * HoloPresenceSpeed);
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

    void AddToCapture(IPhxControlableInstance other)
    {
        PhxPawnController controller = other.GetController();
        if (controller != null)
        {
            CaptureControllers.Add(controller);
            controller.CapturePost = this;
            RefreshCapture();
        }
    }

    void RefreshCapture()
    {
        CaptureTeam = 0;
        CaptureDisputed = false;
        ushort[] teamCounts = new ushort[255];

        foreach (PhxPawnController controller in CaptureControllers)
        {
            if (++teamCounts[controller.Team] > teamCounts[CaptureTeam])
            {
                CaptureTeam = controller.Team;
            }

            if (CaptureTeam > 0)
            {
                CaptureDisputed = CaptureDisputed ||
                    (controller.Team != CaptureTeam && !Match.IsFriend(controller.Team, CaptureTeam));
            }
        }

        CaptureCount = teamCounts[CaptureTeam];
    }

    void ApplyTeam(int oldTeam)
    {
        if (Team == oldTeam)
        {
            // nothing to do
            return;
        }

        CaptureTimer = 0.0f;

        AudioCapture.clip = Team == 0 ? C.ChargeSound.Get<AudioClip>(0) : C.DischargeSound.Get<AudioClip>(0);
        AudioCapture.Play();

        AudioAmbient.loop = true;
        AudioAction.clip = Team == 0 ? C.LostSound.Get<AudioClip>(0) : C.CapturedSound.Get<AudioClip>(0);
        AudioAction.Play();

        HoloPresence = 0.0f;
        HoloPresenceDest = 1.0f;

        if (Team == 0)
        {
            PhxLuaEvents.Invoke(PhxLuaEvents.Event.OnFinishNeutralize, Scene.GetInstanceIndex(this));
        }
        else
        {
            PhxLuaEvents.Invoke(PhxLuaEvents.Event.OnFinishCapture, Scene.GetInstanceIndex(this));
            PhxLuaEvents.Invoke(PhxLuaEvents.Event.OnFinishCaptureName, name, Scene.GetInstanceIndex(this));
            PhxLuaEvents.Invoke(PhxLuaEvents.Event.OnFinishCaptureTeam, Team.Get(), Scene.GetInstanceIndex(this));
        }

        RefreshCapture();
        UpdateColor();
    }
}
