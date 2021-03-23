using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : IMenu
{
    GameMatch MTC => GameRuntime.GetMatch();

    [Header("References")]
    public Button BtnContinue;
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

        BtnFreeCam.GetComponentInChildren<Text>().text = MTC.PlayerST != GameMatch.PlayerState.FreeCam ? "Free Cam" : "Character Selection";

        BtnContinue.onClick.AddListener(Continue);
        BtnFreeCam.onClick.AddListener(FreeCam);
        BtnNextMap.onClick.AddListener(NextMap);
        BtnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        BtnQuit.onClick.AddListener(Quit);
    }

    void Continue()
    {
        GameRuntime.Instance.RemoveMenu();
    }

    void FreeCam()
    {
        if (MTC.PlayerST == GameMatch.PlayerState.CharacterSelection)
        {
            MTC.SetPlayerState(GameMatch.PlayerState.FreeCam);
        }
        else if (MTC.PlayerST == GameMatch.PlayerState.FreeCam)
        {
            MTC.SetPlayerState(GameMatch.PlayerState.CharacterSelection);
        }
    }

    void NextMap()
    {
        GameRuntime.Instance.NextMap();
    }

    void BackToMainMenu()
    {
        GameRuntime.Instance.EnterMainMenu();
    }

    void Quit()
    {
        Application.Quit();
    }
}
