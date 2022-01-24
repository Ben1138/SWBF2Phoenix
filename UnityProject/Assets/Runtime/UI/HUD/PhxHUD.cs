using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxHUD : PhxMenuInterface
{
    PhxGame GAME => PhxGame.Instance;
    PhxMatch Match => PhxGame.GetMatch();
    PhxTimerDB TimerDB => PhxGame.GetTimerDB();

    [Header("References")]
    public Text ReinforcementTeam1;
    public Text ReinforcementTeam2;
    public Text TimerDisplay;
    public Text VicDefTimerDisplay;
    public Text AmmoPrim;
    public PhxUIMap Map;
    public RawImage CaptureDisplay;
    public RawImage Crosshair;

    [Header("Settings")]
    public float AimSpeed = 1f;

    Material CaptureMat;
    Material CrosshairMat;

    float AimFixation;


    public override void Clear()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ReinforcementTeam1 != null);
        Debug.Assert(ReinforcementTeam2 != null);
        Debug.Assert(TimerDisplay       != null);
        Debug.Assert(VicDefTimerDisplay != null);
        Debug.Assert(AmmoPrim           != null);
        Debug.Assert(Map                != null);
        Debug.Assert(CaptureDisplay     != null);
        Debug.Assert(Crosshair          != null);

        ReinforcementTeam1.color = Match.GetTeamColor(1);
        ReinforcementTeam2.color = Match.GetTeamColor(2);

        CaptureMat = CaptureDisplay.materialForRendering;
        CaptureMat.SetTexture("_CaptureIcon", TextureLoader.Instance.ImportUITexture("hud_flag_timer"));

        CrosshairMat = Crosshair.materialForRendering;

        //Reticule.texture = TextureLoader.Instance.ImportUITexture("reticule_rifle");
    }

    // Update is called once per frame
    void Update()
    {
        ReinforcementTeam1.text = Match.GetReinforcementCount(1).ToString();
        ReinforcementTeam2.text = Match.GetReinforcementCount(2).ToString();

        if (Match.Player.Pawn != null)
        {
            Vector3 playerPos = Match.Player.Pawn.GetInstance().transform.position;
            Map.MapOffset.x = -playerPos.x;
            Map.MapOffset.y = -playerPos.z;

            IPhxWeapon weapPrim = Match.Player.Pawn.GetPrimaryWeapon();
            if (weapPrim != null)
            {
                int ammo = weapPrim.GetMagazineAmmo();
                int magazine = weapPrim.GetMagazineSize();

                float reloadProgress = weapPrim.GetReloadProgress();
                if (reloadProgress < 1f)
                {
                    int reload = Mathf.Min(magazine - ammo, weapPrim.GetAvailableAmmo());
                    ammo += (int)(reload * reloadProgress);
                }

                CrosshairMat.SetFloat("_Ammo", ammo);
                CrosshairMat.SetFloat("_Magazin", magazine);

                AmmoPrim.text = weapPrim.GetTotalAmmo().ToString();
            }
            else
            {
                CrosshairMat.SetFloat("_Ammo", 0);
                AmmoPrim.text = "-";
            }

            PhxInstance aim = Match.Player.GetAimObject();
            if (aim != null)
            {
                if (aim.Team != 0)
                {
                    AimFixation = Mathf.Clamp01(AimFixation + Time.deltaTime / AimSpeed);
                }
                else
                {
                    AimFixation = Mathf.Clamp01(AimFixation - Time.deltaTime / AimSpeed);
                }
                CrosshairMat.SetColor("_Color", Match.GetTeamColor(aim.Team));
            }
            else
            {
                AimFixation = Mathf.Clamp01(AimFixation - Time.deltaTime / AimSpeed);
                CrosshairMat.SetColor("_Color", Color.white);
            }

            CrosshairMat.SetFloat("_Fixation", AimFixation);
        }

        PhxCommandpost cp = Match.Player.CapturePost;
        bool displayCapture = cp != null && ((cp.Team != Match.Player.Team) || !Match.IsFriend(cp.CaptureTeam, Match.Player.Team));

        CaptureDisplay.gameObject.SetActive(displayCapture);
        if (displayCapture)
        {
            Color color = Match.GetTeamColor(cp.CaptureTeam);
            float progress = cp.GetCaptureProgress();
            if (cp.CaptureToNeutral)
            {
                color = Match.GetTeamColor(cp.Team);
                progress = 1f - progress;
            }

            // remap for more consistent HUD icon fill
            progress = Mathf.Lerp(0.1f, 0.95f, progress);

            CaptureMat.SetFloat("_CaptureProgress", progress);
            CaptureMat.SetColor("_CaptureColor", color);
            CaptureMat.SetFloat("_CaptureDispute", cp.CaptureDisputed ? 1f : 0f);
        }

        TimerDisplay.gameObject.SetActive(Match.ShowTimer.HasValue);
        if (Match.ShowTimer.HasValue)
        {
            TimerDB.GetTimer(Match.ShowTimer.Value, out var timer);

            TimeSpan t = TimeSpan.FromSeconds(timer.Time);
            TimerDisplay.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            TimerDisplay.color = GAME.Settings.ColorNeutral;
        }

        (int? timerIdx, PhxMatch.PhxTeam.TimerDisplay display) = Match.GetTeamTimer(Match.Player.Team);
        VicDefTimerDisplay.gameObject.SetActive(timerIdx.HasValue);
        if (timerIdx.HasValue)
        {
            Color color = GAME.Settings.ColorNeutral;
            if (display == PhxMatch.PhxTeam.TimerDisplay.Defeat)
            {
                color = GAME.Settings.ColorEnemy;
            }
            else if (display == PhxMatch.PhxTeam.TimerDisplay.Victory)
            {
                color = GAME.Settings.ColorFriendly;
            }

            TimerDB.GetTimer(timerIdx.Value, out var timer);

            TimeSpan t = TimeSpan.FromSeconds(timer.Time);
            VicDefTimerDisplay.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            VicDefTimerDisplay.color = color;
        }
    }
}
