using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxRuntimeMatch
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeEnvironment ENV => PhxGameRuntime.GetEnvironment();
    static PhxRuntimeScene RTS => PhxGameRuntime.GetScene();
    static PhxCamera CAM => PhxGameRuntime.GetCamera();
    static PhxTimerDB TDB => PhxGameRuntime.GetTimerDB();


    public static Color ColorNeutral   = Color.white;
    public static Color ColorEnemy     = Color.red;
    public static Color ColorFriendly  = Color.green;
    public static Color ColorLocals    = Color.yellow;

    public int? ShowTimer = null;

    public PhxPlayerController Player { get; private set; }

    List<PhxPawnController> AIControllers = new List<PhxPawnController>();


    public enum PhxPlayerState
    {
        CharacterSelection,
        Spawned,
        FreeCam
    }

    public PhxPlayerState PlayerST { get; private set; } = PhxPlayerState.CharacterSelection;

    bool LVLsLoaded = false;
    bool AvailablePauseMenu = false;
    int NameCounter;


    public class PhxUnitClass
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

    public class PhxTeam
    {
        public enum TimerDisplay
        {
            Victory, Defeat
        }

        public string Name = "UNKNOWN TEAM";
        public float Aggressiveness = 1.0f;
        public Texture2D Icon = null;
        public int UnitCount = 0;
        public int ReinforcementCount = -1; // default is infinite
        public float SpawnDelay = 1.0f;
        public HashSet<PhxUnitClass> UnitClasses = new HashSet<PhxUnitClass>();
        public PhxClass HeroClass = null;
        public bool[] Friends = new bool[MAX_TEAMS];

        public int? Timer = null;
        public TimerDisplay TimerMode = TimerDisplay.Defeat;
    }

    public PhxTeam[] Teams { get; private set; } = new PhxTeam[MAX_TEAMS];

    Queue<(int, string, int)> TeamUnits = new Queue<(int, string, int)>();
    Queue<(int, string)> TeamHeroClass = new Queue<(int, string)>();
    Queue<(int, string)> TeamIcon = new Queue<(int, string)>();

    AudioClip UIBack;


    public PhxRuntimeMatch()
    {
        for (int i = 0; i < MAX_TEAMS; ++i)
        {
            Teams[i] = new PhxTeam();

            // Everyone if befriended with themselfs
            Teams[i].Friends[i] = true;
        }

        GAME.OnRemoveMenu += OnRemoveMenu;

        Player = new PhxPlayerController();
    }

    public Color GetTeamColor(int teamId)
    {
        if (teamId == 0)
        {
            return ColorNeutral;
        }
        else if (teamId == Player.Team || IsFriend(Player.Team, teamId))
        {
            return ColorFriendly;
        }
        else if (teamId >= 3)
        {
            return ColorLocals;
        }
        return ColorEnemy;
    }

    public void SetPlayerState(PhxPlayerState st)
    {
        if (PlayerST == st) return;

        PlayerST = st;
        if (PlayerST == PhxPlayerState.CharacterSelection)
        {
            ShowCharacterSelection();
            CAM.Fixed(RTS.GetNextCameraShot());
            RemoveHUD();
        }
        else if (PlayerST == PhxPlayerState.Spawned)
        {
            GAME.RemoveMenu();
            CAM.Follow(Player.Pawn);
            ShowHUD();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (PlayerST == PhxPlayerState.FreeCam)
        {
            CAM.Free();
            GAME.RemoveMenu();
            RemoveHUD();
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
                Teams[addTeamUnit.Item1].UnitClasses.Add(new PhxUnitClass { Unit = odf, Count = addTeamUnit.Item3 });
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

    public void Destroy()
    {
        CAM.Fixed();

        GAME.OnMapLoaded -= StartMatch;
        GAME.OnRemoveMenu -= OnRemoveMenu;

        IPhxControlableInstance pawn = Player.Pawn;
        if (pawn != null)
        {
            pawn.UnAssign();
            RTS.DestroyInstance(pawn.GetInstance());
        }

        // TODO: kill AI, clear all pools
    }

    public void Tick(float deltaTime)
    {
        Player.Update(deltaTime);
        for (int i = 0; i < AIControllers.Count; ++i)
        {
            AIControllers[i].Update(deltaTime);
        }

        if (AvailablePauseMenu && Player.CancelPressed)
        {
            if (GAME.IsMenuActive(GAME.PauseMenuPrefab))
            {
                GAME.RemoveMenu();
            }
            else
            {
                ShowMenu(GAME.PauseMenuPrefab);
            }

            if (UIBack == null)
            {
                UIBack = SoundLoader.LoadSound("ui_menuBack");
            }
            GAME.PlayUISound(UIBack, 1.2f);
        }
    }

    public void KillPlayer()
    {
        CAM.Fixed();

        IPhxControlableInstance pawn = Player.Pawn;
        if (pawn != null)
        {
            pawn.UnAssign();
            RTS.DestroyInstance(pawn.GetInstance());
            SetPlayerState(PhxPlayerState.CharacterSelection);
        }
    }

    public IPhxControlableInstance SpawnPlayer(PhxClass cl, PhxCommandpost cp)
    {
        if (cp.SpawnPath.Get() == null)
        {
            Debug.LogWarning($"Cannot spawn on CP '{cp.name}', since it has no spawn path!");
            return null;
        }
        SWBFPath.Node spawnNode = cp.SpawnPath.Get().GetRandom();
        return SpawnPlayer(cl, spawnNode.Position, Quaternion.identity);
    }

    public IPhxControlableInstance SpawnPlayer(PhxClass cl, Vector3 position, Quaternion rotation)
    {
        if (Player.Pawn != null)
        {
            Debug.LogWarning("Player already spawned!");
            return null;
        }

        IPhxControlableInstance pawn = RTS.CreateInstance(cl, "player" + NameCounter++, position, rotation, false) as IPhxControlableInstance;
        if (pawn == null)
        {
            Debug.LogError($"Given spawn class '{cl.Name}' is not a IPhxControlableInstance!");
            return null;
        }

        pawn.GetInstance().gameObject.layer = 3;
        pawn.Assign(Player);
        SetPlayerState(PhxPlayerState.Spawned);

        Debug.Log($"Spawned player at pos: {position} - rot: {rotation}");

        return pawn;
    }

    public IPhxControlableInstance SpawnAI<T>(PhxClass cl, PhxCommandpost cp) where T : PhxPawnController, new()
    {
        if (cp.SpawnPath.Get() == null)
        {
            Debug.LogWarning($"Cannot spawn on CP '{cp.name}', since it has no spawn path!");
            return null;
        }
        SWBFPath.Node spawnNode = cp.SpawnPath.Get().GetRandom();
        return SpawnAI<T>(cl, spawnNode.Position, Quaternion.identity);
    }

    public IPhxControlableInstance SpawnAI<T>(PhxClass cl, Vector3 position, Quaternion rotation) where T : PhxPawnController, new()
    {
        IPhxControlableInstance player = RTS.CreateInstance(cl, "AI" + NameCounter++, position, rotation, false) as IPhxControlableInstance;
        if (player == null)
        {
            Debug.LogError($"Given spawn class '{cl.Name}' is not a IPhxControlableInstance!");
            return null;
        }

        PhxPawnController controller = new T();
        AIControllers.Add(controller);
        player.Assign(controller);
        return player;
    }

    public (int?, PhxTeam.TimerDisplay) GetTeamTimer(int teamIdx)
    {
        if (!CheckTeamIdx(--teamIdx) || !Teams[teamIdx].Timer.HasValue)
        {
            return (null, PhxTeam.TimerDisplay.Defeat);
        }

        TDB.GetTimer(Teams[teamIdx].Timer.Value, out PhxTimerDB.PhxTimer timer);
        if (!timer.InUse)
        {
            Teams[teamIdx].Timer = null;
            Teams[teamIdx].TimerMode = PhxTeam.TimerDisplay.Defeat;
        }

        return (Teams[teamIdx].Timer, Teams[teamIdx].TimerMode);
    }

    // ====================================================================================
    // Lua API start
    // ====================================================================================

    public void SetDefeatTimer(int? timerPtr, int teamIdx)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].Timer = timerPtr;
        Teams[teamIdx].TimerMode = PhxTeam.TimerDisplay.Defeat;
    }

    public void SetVictoryTimer(int? timerPtr, int teamIdx)
    {
        if (!CheckTeamIdx(--teamIdx)) return;
        Teams[teamIdx].Timer = timerPtr;
        Teams[teamIdx].TimerMode = PhxTeam.TimerDisplay.Victory;
    }

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
                Teams[teamIdx].UnitClasses.Add(new PhxUnitClass { Unit = odf, Count = unitCount });
            }

            if (PlayerST == PhxPlayerState.CharacterSelection)
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

            if (PlayerST == PhxPlayerState.CharacterSelection)
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

    // player get's to choose a side and character unit
    public void StartMatch()
    {
        AvailablePauseMenu = true;
        ShowCharacterSelection();
        CAM.Fixed(RTS.GetNextCameraShot());
    }

    T ShowMenu<T>(T menu) where T : PhxMenuInterface
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        return GAME.ShowMenu(menu);
    }

    void ShowCharacterSelection()
    {
        PhxCharacterSelect charSel = ShowMenu(GAME.CharacterSelectPrefab);
        foreach (PhxUnitClass cl in Teams[0].UnitClasses)
        {
            charSel.Add(cl.Unit);
        }
        //charSel.Add(Teams[0].HeroClass);
    }

    void ShowHUD()
    {
        if (GAME.IsMenuActive(GAME.HUDPrefab))
        {
            return;
        }
        ShowMenu(GAME.HUDPrefab);
    }

    void RemoveHUD()
    {
        if (GAME.IsMenuActive(GAME.HUDPrefab))
        {
            GAME.RemoveMenu();
        }
    }

    void OnRemoveMenu(Type menuType)
    {
        if (menuType == GAME.PauseMenuPrefab.GetType())
        {
            if (PlayerST == PhxPlayerState.CharacterSelection)
            {
                ShowCharacterSelection();
            }
            else if (PlayerST == PhxPlayerState.Spawned)
            {
                ShowHUD();

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
