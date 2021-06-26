using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxPauseMenu : PhxMenuInterface
{
    PhxRuntimeMatch MTC => PhxGameRuntime.GetMatch();

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

        BtnFreeCam.GetComponentInChildren<Text>().text = MTC.PlayerST != PhxRuntimeMatch.PhxPlayerState.FreeCam ? "Free Cam" : "Character Selection";

        BtnContinue.onClick.AddListener(Continue);
        BtnRespawn.onClick.AddListener(Respawn);
        BtnFreeCam.onClick.AddListener(FreeCam);
        BtnNextMap.onClick.AddListener(NextMap);
        BtnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        BtnQuit.onClick.AddListener(Quit);
    }

    void Continue()
    {
        PhxGameRuntime.Instance.RemoveMenu();
    }

    void Respawn()
    {
        MTC.KillPlayer();
    }

    void FreeCam()
    {
        if (MTC.PlayerST == PhxRuntimeMatch.PhxPlayerState.CharacterSelection)
        {
            MTC.SetPlayerState(PhxRuntimeMatch.PhxPlayerState.FreeCam);
        }
        else if (MTC.PlayerST == PhxRuntimeMatch.PhxPlayerState.FreeCam)
        {
            MTC.SetPlayerState(PhxRuntimeMatch.PhxPlayerState.CharacterSelection);
        }
    }

    void NextMap()
    {
        PhxGameRuntime.Instance.NextMap();
    }

    void BackToMainMenu()
    {
        PhxGameRuntime.Instance.EnterMainMenu();
    }

    void Quit()
    {
        Application.Quit();
    }
}
