using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void ContinueClicked()
    {
        GameRuntime.Instance.RemoveMenu();
    }

    public void BackToMainMenuClicked()
    {
        GameRuntime.Instance.EnterMainMenu();
    }
}
