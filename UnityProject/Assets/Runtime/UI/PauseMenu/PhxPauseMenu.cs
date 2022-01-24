using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxPauseMenu : PhxMenuInterface
{
    PhxMatch MTC => PhxGame.GetMatch();

    [Header("References")]
    public Button BtnContinue;
    public Button BtnRespawn;
    public Button BtnFreeCam;
    public Button BtnNextMap;
    public Button BtnBackToMainMenu;
    public Button BtnQuit;


    public override void Clear()
    {

    }

    void Start()
    {
        Debug.Assert(BtnContinue       != null);
        Debug.Assert(BtnFreeCam        != null);
        Debug.Assert(BtnNextMap        != null);
        Debug.Assert(BtnBackToMainMenu != null);
        Debug.Assert(BtnQuit           != null);

        BtnFreeCam.GetComponentInChildren<Text>().text = MTC.PlayerST != PhxMatch.PhxPlayerState.FreeCam ? "Free Cam" : "Character Selection";

        BtnContinue.onClick.AddListener(Continue);
        BtnRespawn.onClick.AddListener(Respawn);
        BtnFreeCam.onClick.AddListener(FreeCam);
        BtnNextMap.onClick.AddListener(NextMap);
        BtnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        BtnQuit.onClick.AddListener(Quit);
    }

    void Continue()
    {
        PhxGame.Instance.RemoveMenu();
    }

    void Respawn()
    {
        MTC.KillPlayer();
    }

    void FreeCam()
    {
        if (MTC.PlayerST == PhxMatch.PhxPlayerState.CharacterSelection)
        {
            MTC.SetPlayerState(PhxMatch.PhxPlayerState.FreeCam);
        }
        else if (MTC.PlayerST == PhxMatch.PhxPlayerState.FreeCam)
        {
            MTC.SetPlayerState(PhxMatch.PhxPlayerState.CharacterSelection);
        }
    }

    void NextMap()
    {
        PhxGame.Instance.NextMap();
    }

    void BackToMainMenu()
    {
        PhxGame.Instance.EnterMainMenu();
    }

    void Quit()
    {
        Application.Quit();
    }
}
