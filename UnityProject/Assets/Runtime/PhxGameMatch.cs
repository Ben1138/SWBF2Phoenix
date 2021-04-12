using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxGameMatch
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeEnvironment ENV => PhxGameRuntime.GetEnvironment();
    static PhxRuntimeScene RTS => PhxGameRuntime.GetScene();
    static PhxCamera Cam => PhxGameRuntime.GetCamera();


    public Color NeutralColor  { get; private set; } = new Color(1.0f, 1.0f, 1.0f);
    public Color FriendlyColor { get; private set; } = new Color(0.0f, 0.0f, 1.0f);
    public Color EnemyColor    { get; private set; } = new Color(1.0f, 0.0f, 0.0f);
    public Color LocalsColor   { get; private set; } = new Color(1.0f, 1.0f, 0.0f);

    public PhxPlayerController Player { get; private set; }


    public enum PlayerState
    {
        CharacterSelection,
        Spawned,
        FreeCam
    }

    public PlayerState PlayerST { get; private set; } = PlayerState.CharacterSelection;

    bool LVLsLoaded = false;
    bool AvailablePauseMenu = false;
    bool PauseMenuActive = false;
    int NameCounter;


    public class UnitClass
    {
        public PhxClass Unit = null;
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
        public int ReinforcementCount = -1; // default is infinite
        public float SpawnDelay = 1.0f;
        public HashSet<UnitClass> UnitClasses = new HashSet<UnitClass>();
        public PhxClass HeroClass = null;
        public bool[] Friends = new bool[MAX_TEAMS];
    }

    public Team[] Teams { get; private set; } = new Team[MAX_TEAMS];

    Queue<(int, string, int)> TeamUnits = new Queue<(int, string, int)>();
    Queue<(int, string)> TeamHeroClass = new Queue<(int, string)>();
    Queue<(int, string)> TeamIcon = new Queue<(int, string)>();

    AudioClip UIBack;
    PhxCamera.CamMode LastMode;


    public PhxGameMatch()
    {
        for (int i = 0; i < MAX_TEAMS; ++i)
        {
            Teams[i] = new Team();
        }

        GAME.OnMatchStart += StartMatch;
        GAME.OnRemoveMenu += OnRemoveMenu;

        Player = new PhxPlayerController();
    }

    public void SetPlayerState(PlayerState st)
    {
        if (PlayerST == st) return;

        PlayerST = st;
        if (PlayerST == PlayerState.CharacterSelection)
        {
            ShowCharacterSelection();
            Cam.Mode = PhxCamera.CamMode.Fixed;
        }
        else if (PlayerST == PlayerState.FreeCam)
        {
            Cam.Mode = PhxCamera.CamMode.Free;
            if (PauseMenuActive)
            {
                GAME.RemoveMenu();
                PauseMenuActive = false;
            }
        }
        else if (PlayerST == PlayerState.Spawned)
        {
            GAME.RemoveMenu();
            Cam.Mode = PhxCamera.CamMode.Control;
        }
        else
        {
            GAME.RemoveMenu();
            Cam.Mode = PhxCamera.CamMode.Fixed;
        }
    }

    // Since we're doing multithreaded loading, calling Lua stuff like "AddUnitClass" in "ScriptInit()"
    // will yield no results, since the .lvl files containing those classes might still load at that point.
    // Workaround: Queue those calls and apply them after all .lvl files have been loaded
    public void ApplySchedule()
    {
        while (TeamUnits.Count > 0)
        {
            (int, string, int) addTeamUnit = TeamUnits.Dequeue();
            PhxClass odf = RTS.GetClass(addTeamUnit.Item2);
            if (odf != null)
            {
                Teams[addTeamUnit.Item1].UnitClasses.Add(new UnitClass { Unit = odf, Count = addTeamUnit.Item3 });
            }
        }

        while (TeamHeroClass.Count > 0)
        {
            (int, string) setHeroClass = TeamHeroClass.Dequeue();
            PhxClass odf = RTS.GetClass(setHeroClass.Item2);
            if (odf != null)
            {
                Teams[setHeroClass.Item1].HeroClass = odf;
            }
        }

        while (TeamIcon.Count > 0)
        {
            (int, string) setTeamIcon = TeamIcon.Dequeue();
            Teams[setTeamIcon.Item1].Icon = TextureLoader.Instance.ImportUITexture(setTeamIcon.Item2);
        }

        LVLsLoaded = true;
    }

    public void Clear()
    {
        GAME.OnMatchStart -= StartMatch;
        GAME.OnRemoveMenu -= OnRemoveMenu;
    }

    public void Update()
    {
        Player.Update(Time.deltaTime);

        if (AvailablePauseMenu && Player.CancelPressed)
        {
            if (PauseMenuActive)
            {
                GAME.RemoveMenu();
            }
            else
            {
                GAME.ShowMenu(GAME.PauseMenuPrefab);
                PauseMenuActive = true;
                LastMode = Cam.Mode;
                Cam.Mode = PhxCamera.CamMode.Fixed;
            }

            if (UIBack == null)
            {
                UIBack = SoundLoader.LoadSound("ui_menuBack");
            }
            GAME.PlayUISound(UIBack, 1.2f);
        }
    }

    public void SpawnPlayer(PhxClass cl)
    {
        PhxCamera cam = PhxGameRuntime.GetCamera();
        Vector3 spawnPos = cam.transform.position + cam.transform.rotation * (cam.PositionOffset * 2f);

        PhxSoldier pawn = RTS.CreateInstance(cl, "player" + NameCounter++, spawnPos, Quaternion.identity) as PhxSoldier;
        if (pawn == null)
        {
            Debug.LogError($"Given spawn class '{cl.Name}' is not a soldier!");
            return;
        }

        pawn.gameObject.layer = 3;
        pawn.Controller = Player;
        Player.Pawn = pawn;
        SetPlayerState(PlayerState.Spawned);
    }

    public void SpawnAI(PhxClass cl)
    {
        PhxSoldier player = RTS.CreateInstance(cl, "AI" + NameCounter++, Camera.main.transform.position, Quaternion.identity) as PhxSoldier;
        if (player == null)
        {
            Debug.LogError($"Given spawn class '{cl.Name}' is not a soldier!");
            return;
        }

        player.Controller = new PhxAIController();
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
        if (!LVLsLoaded)
        {
            TeamIcon.Enqueue((teamIdx, iconName));
        }
        else
        {
            Teams[teamIdx].Icon = TextureLoader.Instance.ImportUITexture(iconName);
        }
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

        if (!LVLsLoaded)
        {
            TeamUnits.Enqueue((teamIdx, className, unitCount));
        }
        else
        {
            PhxClass odf = RTS.GetClass(className);
            if (odf != null)
            {
                Teams[teamIdx].UnitClasses.Add(new UnitClass { Unit = odf, Count = unitCount });
            }

            if (PlayerST == PlayerState.CharacterSelection)
            {
                // refresh
                ShowCharacterSelection();
            }
        }
    }

    public void SetHeroClass(int teamIdx, string className)
    {
        if (!CheckTeamIdx(--teamIdx)) return;

        if (!LVLsLoaded)
        {
            TeamHeroClass.Enqueue((teamIdx, className));
        }
        else
        {
            PhxClass odf = RTS.GetClass(className);
            if (odf != null)
            {
                Teams[teamIdx].HeroClass = odf;
            }

            if (PlayerST == PlayerState.CharacterSelection)
            {
                // refresh
                ShowCharacterSelection();
            }
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

    void ShowCharacterSelection()
    {
        PauseMenuActive = false;
        PhxCharacterSelect charSel = GAME.ShowMenu(GAME.CharacterSelectPrefab).GetComponent<PhxCharacterSelect>();
        foreach (UnitClass cl in Teams[0].UnitClasses)
        {
            charSel.Add(cl.Unit);
        }
        //charSel.Add(Teams[0].HeroClass);
    }

    void OnRemoveMenu()
    {
        PauseMenuActive = false;
        Cam.Mode = LastMode;
        if (PlayerST == PlayerState.CharacterSelection)
        {
            ShowCharacterSelection();
        }
    }

    // player get's to choose a side and character unit
    void StartMatch()
    {
        AvailablePauseMenu = true;
        ShowCharacterSelection();
    }
}
