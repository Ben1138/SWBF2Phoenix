using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct PhxSettings
{
    public string GamePathString;

    public string Language;
    public PhxBootMode BootMode;
    public string BootSWBF2Map;
    public string BootUnityScene;
    public bool InfiniteAmmo;

    public Color ColorNeutral;
    public Color ColorEnemy;
    public Color ColorFriendly;
    public Color ColorLocals;
}

public enum PhxBootMode
{
    MainMenu,
    SWBF2Map,
    UnityScene
}