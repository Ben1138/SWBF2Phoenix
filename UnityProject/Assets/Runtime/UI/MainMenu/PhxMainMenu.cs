using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxMainMenu : PhxMenuInterface
{
    static PhxEnvironment ENV { get { return PhxGame.GetEnvironment(); } }
    static PhxLuaRuntime RT { get { return PhxGame.GetLuaRuntime(); } }

    struct SubIcon
    {
        public string Sub;
        public Texture2D Icon;
    }

    [Header("References")]
    public PhxListBox LstMaps;
    public PhxListBox LstModes;
    public PhxListBox LstEras;
    public PhxListBox LstRotation;
    public Button BtnAdd;
    public Button BtnRemove;
    public Button BtnRemoveAll;
    public Button BtnStart;
    public Button BtnQuit;

    List<string>  MapLuaFiles = new List<string>();
    List<SubIcon> ModeSubs = new List<SubIcon>();
    List<SubIcon> EraSubs = new List<SubIcon>();
    List<string>  RotationLuaFiles = new List<string>();

    // These are just for convenience, so the user doesn't
    // have to re-check his last checked modes and eras
    HashSet<string> LastCheckedModes = new HashSet<string>();
    HashSet<string> LastCheckedEras = new HashSet<string>();


    public override void Clear()
    {

    }

    void OnMapSelectionChanged(int newIdx)
    {
        string mapluafile = MapLuaFiles[newIdx];

        LstModes.Clear();
        LstEras.Clear();
        ModeSubs.Clear();
        EraSubs.Clear();

        object[] res = RT.CallLuaFunction("missionlist_ExpandModelist", 1, mapluafile);
        PhxLuaRuntime.Table modes = res[0] as PhxLuaRuntime.Table;
        foreach (KeyValuePair<object, object> entry in modes)
        {
            PhxLuaRuntime.Table mode = entry.Value as PhxLuaRuntime.Table;
            string modeNamePath = mode.Get<string>("showstr");
            if (mode.Get("bIsWildcard") == null)
            {
                PhxListBoxItem item = LstModes.AddItem(ENV.GetLocalized(modeNamePath));
                Texture2D icon = TextureLoader.Instance.ImportUITexture(mode.Get<string>("icon"));
                item.SetIcon(icon);

                string key = mode.Get<string>("key");
                item.SetChecked(LastCheckedModes.Contains(key));
                item.OnCheckChanged += (bool check) =>
                {
                    if (check)
                    {
                        LastCheckedModes.Add(key);
                    }
                    else
                    {
                        LastCheckedModes.Remove(key);
                    }
                };

                ModeSubs.Add(new SubIcon { Sub = mode.Get<string>("subst"), Icon = icon });
            }
        }

        res = RT.CallLuaFunction("missionlist_ExpandEralist", 1, mapluafile);
        PhxLuaRuntime.Table eras = res[0] as PhxLuaRuntime.Table;
        foreach (KeyValuePair<object, object> entry in eras)
        {
            PhxLuaRuntime.Table era = entry.Value as PhxLuaRuntime.Table;
            string eraNamePath = era.Get<string>("showstr");
            if (era.Get("bIsWildcard") == null)
            {
                PhxListBoxItem item = LstEras.AddItem(ENV.GetLocalized(eraNamePath));
                Texture2D icon = TextureLoader.Instance.ImportUITexture(era.Get<string>("icon2"));
                item.SetIcon(icon);

                string key = era.Get<string>("key");
                item.SetChecked(LastCheckedEras.Contains(key));
                item.OnCheckChanged += (bool check) =>
                {
                    if (check)
                    {
                        LastCheckedEras.Add(key);
                    }
                    else
                    {
                        LastCheckedEras.Remove(key);
                    }
                };

                EraSubs.Add(new SubIcon { Sub = era.Get<string>("subst"), Icon = icon });
            }
        }
    }

    void AddMap()
    {
        if (LstMaps.CurrentSelection < 0)
        {
            return;
        }

        string mapluafile = MapLuaFiles[LstMaps.CurrentSelection];
        int[] modeIndices = LstModes.GetCheckedIndices();
        int[] eraIndices  = LstEras.GetCheckedIndices();

        for (int i = 0; i < modeIndices.Length; ++i)
        {
            SubIcon modeSub = ModeSubs[modeIndices[i]];

            for (int j = 0; j < eraIndices.Length; j++)
            {
                SubIcon eraSub = EraSubs[eraIndices[j]];

                string mapName = ENV.GetLocalizedMapName(mapluafile);
                PhxListBoxItem item = LstRotation.AddItem(mapName);
                item.SetIcon(modeSub.Icon);
                item.SetIcon2(eraSub.Icon);

                string mapScript = PhxHelpers.Format(mapluafile, eraSub.Sub, modeSub.Sub);
                RotationLuaFiles.Add(mapScript);
            }
        }
    }

    void RemoveSelectionFromRotation()
    {
        // TODO
    }

    void ClearRotation()
    {
        RotationLuaFiles.Clear();
        LstRotation.Clear();
    }

    void StartRotation()
    {
        PhxGame.Instance.AddToMapRotation(RotationLuaFiles);
        PhxGame.Instance.NextMap();
    }

    void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        Debug.Assert(LstMaps      != null);
        Debug.Assert(LstModes     != null);
        Debug.Assert(LstEras      != null);
        Debug.Assert(LstRotation  != null);
        Debug.Assert(BtnAdd       != null);
        Debug.Assert(BtnRemove    != null);
        Debug.Assert(BtnRemoveAll != null);
        Debug.Assert(BtnStart     != null);
        Debug.Assert(BtnQuit      != null);

        LstMaps.OnSelect += OnMapSelectionChanged;

        BtnAdd.onClick.AddListener(AddMap);
        BtnRemove.onClick.AddListener(RemoveSelectionFromRotation);
        BtnRemoveAll.onClick.AddListener(ClearRotation);
        BtnStart.onClick.AddListener(StartRotation);
        BtnQuit.onClick.AddListener(Quit);

        bool bForMP = false;
        RT.CallLuaFunction("missionlist_ExpandMaplist", 0, bForMP);
        PhxLuaRuntime.Table spMissions = RT.GetTable("missionselect_listbox_contents");

        foreach (KeyValuePair<object, object> entry in spMissions)
        {
            PhxLuaRuntime.Table map = entry.Value as PhxLuaRuntime.Table;
            string mapluafile = map.Get<string>("mapluafile");
            bool bIsModLevel  = map.Get<bool>("isModLevel");
            string mapName    = ENV.GetLocalizedMapName(mapluafile);

            LstMaps.AddItem(mapName, bIsModLevel);
            MapLuaFiles.Add(mapluafile);
        }
    }
}
