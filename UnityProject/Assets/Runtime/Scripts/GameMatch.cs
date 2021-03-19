using System;
using System.Collections.Generic;
using UnityEngine;

public class GameMatch
{
    static RuntimeEnvironment ENV => GameRuntime.GetEnvironment();
    static RuntimeScene RTS => GameRuntime.GetScene();


    public Color NeutralColor  { get; private set; } = new Color(1.0f, 1.0f, 1.0f);
    public Color FriendlyColor { get; private set; } = new Color(0.0f, 0.0f, 1.0f);
    public Color EnemyColorr   { get; private set; } = new Color(1.0f, 0.0f, 0.0f);
    public Color LocalsColorr  { get; private set; } = new Color(1.0f, 1.0f, 0.0f);

    

    public class UnitClass
    {
        public ISWBFClass Unit = null;
        public int Count = 0;

        public override int GetHashCode()
        {
            return Unit.GetHashCode();
        }
    }

    // TODO: verify if enough
    public const int MAX_TEAMS = 20;

    public class Team
    {
        public string Name = "UNKNOWN TEAM";
        public float Aggressiveness = 1.0f;
        public Texture2D Icon = null;
        public int UnitCount = 0;
        public int ReinforcementCount = 0; // -1 == infinite
        public float SpawnDelay = 1.0f;
        public HashSet<UnitClass> UnitClasses = new HashSet<UnitClass>();
        public ISWBFClass HeroClass = null;
        public bool[] Friends = new bool[MAX_TEAMS];
    }

    public Team[] Teams { get; private set; } = new Team[MAX_TEAMS];


    public GameMatch()
    {
        for (int i = 0; i < MAX_TEAMS; ++i)
        {
            Teams[i] = new Team();
        }
    }


    // ====================================================================================
    // Lua API start
    // ====================================================================================

    public void SetTeamName(int teamIdx, string name)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].Name = name;
    }

    public void SetTeamIcon(int teamIdx, string iconName)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].Icon = TextureLoader.Instance.ImportUITexture(iconName);
    }

    public void SetUnitCount(int teamIdx, int unitCount)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].UnitCount = unitCount;
    }

    public void SetReinforcementCount(int teamIdx, int reinfCount)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].ReinforcementCount = reinfCount;
    }

    public int GetReinforcementCount(int teamIdx)
    {
        if (!CheckTeamIdx(--teamIdx)) return 0;
        return Teams[teamIdx].ReinforcementCount;
    }

    public void AddReinforcements(int teamIdx, int reinfCount)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].ReinforcementCount += reinfCount;
    }

    public void AddUnitClass(int teamIdx, string className, int unitCount)
    {
        if (!CheckTeamIdx(--teamIdx)) return;

        ISWBFClass odf = RTS.GetClass(className);
        if (odf != null)
        {
            Teams[teamIdx].UnitClasses.Add(new UnitClass { Unit = odf, Count = unitCount });
        }
    }

    public void SetHeroClass(int teamIdx, string className)
    {
        if (!CheckTeamIdx(--teamIdx)) return;

        ISWBFClass odf = RTS.GetClass(className);
        if (odf != null)
        {
            Teams[teamIdx].HeroClass = odf;
        }
    }

    public void SetTeamAsEnemy(int teamIdx1, int teamIdx2)
    {
        if (!CheckTeamIdx(--teamIdx1, --teamIdx2)) return;
        Teams[teamIdx1].Friends[teamIdx2] = false;
    }

    public void SetTeamAsFriend(int teamIdx1, int teamIdx2)
    {
        if (!CheckTeamIdx(--teamIdx1, --teamIdx2)) return;
        Teams[teamIdx1].Friends[teamIdx2] = true;
    }

    public bool IsFriend(int teamIdx1, int teamIdx2)
    {
        if (!CheckTeamIdx(--teamIdx1, --teamIdx2)) return false;
        return Teams[teamIdx1].Friends[teamIdx2];
    }

    // ====================================================================================
    // Lua API end
    // ====================================================================================




    bool CheckTeamIdx(int teamIdx)
    {
        if (teamIdx < 0 || teamIdx >= MAX_TEAMS)
        {
            Debug.LogErrorFormat($"Team index '{teamIdx}' is out of range '{MAX_TEAMS}'!");
            return false;
        }
        return true;
    }

    bool CheckTeamIdx(int teamIdx1, int teamIdx2)
    {
        return CheckTeamIdx(teamIdx1) && CheckTeamIdx(teamIdx2);
    }
}
