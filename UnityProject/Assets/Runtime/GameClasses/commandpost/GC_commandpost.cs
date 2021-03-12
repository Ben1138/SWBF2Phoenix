using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using LibSWBF2.Wrappers;

using CL = ClassLoader;

public class GC_commandpost : ISWBFGameClass
{
    struct ClassProperties
    {
       public float NeutralizeTime;
       public float CaptureTime;
       public float HoloTurnOnTime;
       public AudioClip CapturedSound;
       public AudioClip LostSound;
       public AudioClip ChargeSound;
       public AudioClip DischargeSound;
    }

    [Header("SWBF Instance Properties")]
    public Collider CaptureRegion;
    public Collider ControlRegion;
    public byte Team = 0;

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject HoloIcon;
    public HDAdditionalLightData Light;

    AudioSource AudioAction;
    AudioSource AudioAmbient;
    AudioSource AudioCapture;
    Vector2 CapturePitch = new Vector2(0.5f, 1.5f);
    float CaptureTimer;

    // cache
    bool bInitClass = false;
    bool bInitInstance = false;
    float HoloWidthStart;
    float HoloWidthEnd;
    float LightIntensity;
    Color HoloColor;
    float HoloAlpha;


    public override void InitClass(EntityClass cl)
    {
        if (bInitClass) return;

        CL.AssignProp(cl, "NeutralizeTime",    ref NeutralizeTime);
        CL.AssignProp(cl, "CaptureTime",       ref CaptureTime);
        CL.AssignProp(cl, "HoloTurnOnTime",    ref HoloTurnOnTime);
        CL.AssignProp(cl, "CapturedSound",  0, ref CapturedSound);
        CL.AssignProp(cl, "LostSound",      0, ref LostSound);
        CL.AssignProp(cl, "ChargeSound",    0, ref ChargeSound);
        CL.AssignProp(cl, "DischargeSound", 0, ref DischargeSound);

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

        bInitClass = true;
    }

    public override void InitInstance(Instance inst)
    {
        CL.AssignProp(inst, "CaptureRegion", ref CaptureRegion);
        CL.AssignProp(inst, "ControlRegion", ref ControlRegion);
        CL.AssignProp(inst, "Team",          ref Team);

        SetTeam(Team);
        bInitInstance = true;
    }

    public override void SetProperty(string propName, object propValue)
    {
        
    }

    public override void SetClassProperty(string propName, object propValue)
    {
        throw new NotImplementedException();
    }

    public void SetTeam(byte teamId)
    {
        CaptureTimer = 0.0f;
        Team = teamId;

        AudioCapture.clip = Team == 0 ? ChargeSound : DischargeSound;
        AudioCapture.Play();

        AudioAction.clip = Team == 0 ? LostSound : CapturedSound;
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
            progress = CaptureTimer / CaptureTime;
            if (CaptureTimer >= CaptureTime)
            {
                SetTeam(1);
            }
            AudioCapture.pitch = Mathf.Lerp(CapturePitch.x, CapturePitch.y, CaptureTimer / CaptureTime);
        }
        else
        {
            progress = CaptureTimer / NeutralizeTime;
            if (CaptureTimer >= NeutralizeTime)
            {
                SetTeam(0);
            }
            AudioCapture.pitch = Mathf.Lerp(CapturePitch.y, CapturePitch.x, CaptureTimer / CaptureTime);
        }

        float holoPresence = Mathf.Clamp01(Mathf.Sin(progress * Mathf.PI) * 3.0f);
        HoloRay.startWidth = Mathf.Lerp(0.0f, HoloWidthStart, holoPresence);
        HoloRay.endWidth   = Mathf.Lerp(0.0f, HoloWidthEnd,   holoPresence);
        Light.intensity    = Mathf.Lerp(0.0f, LightIntensity, holoPresence);

        HoloColor.a = Mathf.Lerp(0.0f, HoloAlpha, holoPresence);
        HoloRay.material.SetColor("_UnlitColor", HoloColor);
    }
}
