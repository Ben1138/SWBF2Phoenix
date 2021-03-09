using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
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
    public AudioClip CapturedSound;
    public AudioClip LostSound;
    public AudioClip ChargeSound;
    public AudioClip DischargeSound;

    [Header("References")]
    public LineRenderer HoloRay;
    public GameObject HoloIcon;
    public HDAdditionalLightData Light;

    byte TeamID = 0;
    AudioSource AudioAction;
    AudioSource AudioAmbient;
    AudioSource AudioCapture;
    Vector2 CapturePitch = new Vector2(0.5f, 1.5f);
    float CaptureTimer;

    // cache
    bool bInit = false;
    float HoloWidthStart;
    float HoloWidthEnd;
    float LightIntensity;
    Color HoloColor;
    float HoloAlpha;


    public override void Init(Instance inst)
    {
        CL.AssignProp(inst, "NeutralizeTime",    ref NeutralizeTime);
        CL.AssignProp(inst, "CaptureTime",       ref CaptureTime);
        CL.AssignProp(inst, "HoloTurnOnTime",    ref HoloTurnOnTime);
        CL.AssignProp(inst, "CaptureRegion",     ref CaptureRegion);
        CL.AssignProp(inst, "ControlRegion",     ref ControlRegion);
        CL.AssignProp(inst, "CapturedSound",  0, ref CapturedSound);
        CL.AssignProp(inst, "LostSound",      0, ref LostSound);
        CL.AssignProp(inst, "ChargeSound",    0, ref ChargeSound);
        CL.AssignProp(inst, "DischargeSound", 0, ref DischargeSound);

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

        SetTeam(0);
        bInit = true;
    }

    public void SetTeam(byte teamId)
    {
        CaptureTimer = 0.0f;
        TeamID = teamId;

        AudioCapture.clip = TeamID == 0 ? ChargeSound : DischargeSound;
        AudioCapture.Play();

        AudioAction.clip = TeamID == 0 ? LostSound : CapturedSound;
        AudioAction.Play();

        UpdateColor();
    }

    public void UpdateColor()
    {
        HoloColor = GameRuntime.Instance.GetTeamColor(TeamID);
        HoloRay.material.SetColor("_EmissiveColor", HoloColor);
        Light.color = HoloColor;
    }

    void Update()
    {
        if (!bInit) return;

        CaptureTimer += Time.deltaTime;
        float progress;
        if (TeamID == 0)
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
