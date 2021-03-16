using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public Button BtnContinue;
    public Button BtnNextMap;
    public Button BtnBackToMainMenu;
    public Button BtnQuit;

    void Start()
    {
        Debug.Assert(BtnContinue       != null);
        Debug.Assert(BtnNextMap        != null);
        Debug.Assert(BtnBackToMainMenu != null);
        Debug.Assert(BtnQuit != null);

        BtnContinue.onClick.AddListener(Continue);
        BtnNextMap.onClick.AddListener(NextMap);
        BtnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        BtnQuit.onClick.AddListener(Quit);
    }

    void Continue()
    {
        GameRuntime.Instance.RemoveMenu();
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
