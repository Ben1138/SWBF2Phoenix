using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGeonosis()
    {
        GameRuntime.Instance.EnterMap("geo1c_con");
    }
}
