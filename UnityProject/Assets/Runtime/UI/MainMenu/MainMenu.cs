using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGeonosis()
    {
        GameRuntime.Instance.EnterMap("geo1c_con");
    }

    public void StartMustafar()
    {
        GameRuntime.Instance.EnterMap("mus1c_con");
    }

    public void StartCorouscant()
    {
        GameRuntime.Instance.EnterMap("cor1c_con");
    }
}
