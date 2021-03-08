using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMatch : MonoBehaviour
{
    public static GameMatch Instance { get; private set; }

    public Color NeutralColor  = new Color(1.0f, 1.0f, 1.0f);
    public Color FriendlyColor = new Color(0.0f, 0.0f, 1.0f);
    public Color EnemyColorr   = new Color(1.0f, 0.0f, 0.0f);
    public Color LocalsColorr  = new Color(1.0f, 1.0f, 0.0f);


    public void Awake()
    {
        Instance = this;
    }

    struct Team
    {
        string Name;
        float Aggressiveness;
        List<byte> Friends;
    }

    List<Team> Teams;
}
