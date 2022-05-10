using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LibSWBF2.Enums;

public class PhxCharacterSelect : PhxMenuInterface
{
    static PhxGame Game => PhxGame.Instance;
    static PhxScene Scene => PhxGame.GetScene();
    static PhxMatch Match => PhxGame.GetMatch();
    static PhxCamera Camera => PhxGame.GetCamera();


    [Header("References")]
    public PhxUIMap Map;
    public PhxCharacterItem ItemPrefab;
    public RectTransform ListContents;
    public Button BtnSpawn;
    public Button BtnSwitchTeam;
    public Button BtnNextCamera;

    [Header("Settings")]
    public float ItemSpace = 5.0f;
    public float MaxItemHeight = 200f;

    List<PhxCharacterItem> Items = new List<PhxCharacterItem>();
    PhxClass CurrentSelection = null;
    List<IPhxControlableInstance> UnitPreviews = new List<IPhxControlableInstance>();
    PhxCommandpost SpawnCP;

    static int nameCounter = 0;

    public void UpdateCharacterList()
    {
        Clear();

        var team = Match.Teams[Match.Player.Team - 1];
        foreach (var cl in team.UnitClasses)
        {
            Add(cl.Unit);
        }
        Add(team.HeroClass);
    }

    public override void Clear()
    {
        CurrentSelection = null;

        // Destroy UI items
        foreach (var item in Items)
        {
            Destroy(item.gameObject);
        }
        Items.Clear();

        // Destroy unit preview instances
        for (int i = 0; i < UnitPreviews.Count; ++i)
        {
            Scene.DestroyInstance(UnitPreviews[i].GetInstance());
        }
        UnitPreviews.Clear();
    }

    public void Add(PhxClass cl)
    {
        if (cl.ClassType != EEntityClassType.GameObjectClass)
        {
            Debug.LogError($"Cannot add odf class '{cl.Name}' as item to character selection!");
            return;
        }
         
        // CSP = Char Select Preview
        IPhxControlableInstance preview = Scene.CreateInstance(cl, cl.Name+"_CSP" + nameCounter++, Vector3.zero, Quaternion.identity, false, Game.CharSelectTransform) as IPhxControlableInstance;
        preview.Fixate();
        UnitPreviews.Add(preview);

        PhxCharacterItem item = Instantiate(ItemPrefab, ListContents);
        item.OnClicked += () =>
        {
            SetActive(item, cl, preview);
        };

        item.SetHeaderText(cl.LocalizedName);

        string detailText = "";
        PhxSoldier.ClassProperties soldier = cl as PhxSoldier.ClassProperties;
        if (soldier != null)
        {
            foreach (Dictionary<string, IPhxPropRef> section in soldier.Weapons)
            {
                if (section.TryGetValue("WeaponName", out IPhxPropRef nameVal))
                {
                    PhxProp<string> weapName = (PhxProp<string>)nameVal;
                    PhxClass weapClass = Scene.GetClass(weapName);
                    if(weapClass != null)
                    {
                        PhxProp<int> medalProp = weapClass.P.Get<PhxProp<int>>("MedalsTypeToUnlock");
                        if (medalProp != null && medalProp != 0)
                        {
                            // Skip medal/award weapons for display
                            continue;
                        }
                        detailText += weapClass.LocalizedName + '\n';
                    }
                }
            }
            item.SetDetailText(detailText);
        }

        Items.Add(item);

        if (CurrentSelection == null)
        {
            SetActive(item, cl, preview);
        }
        else
        {
            item.SetActive(false);
            preview.GetInstance().gameObject.SetActive(false);
        }

        ReCalcItemSize();
    }

    void SetActive(PhxCharacterItem item, PhxClass cl, IPhxControlableInstance preview)
    {
        foreach (PhxCharacterItem it in Items)
        {
            it.SetActive(false);
        }
        item.SetActive(true);
        CurrentSelection = cl;

        foreach (PhxInstance inst in UnitPreviews)
        {
            inst.gameObject.SetActive(false);
        }
        preview.GetInstance().gameObject.SetActive(true);

        IPhxControlableInstance animPreview = preview as IPhxControlableInstance;
        if (animPreview != null)
        {
            animPreview.PlayIntroAnim();
        }
    }

    void ReCalcItemSize()
    {
        float availableHeight = ListContents.rect.height - (ItemSpace * (Items.Count - 1));
        float itemHeight = availableHeight / Items.Count;
        itemHeight = Mathf.Min(itemHeight, MaxItemHeight);

        for (int i = 0; i < Items.Count; ++i)
        {
            RectTransform trans = (RectTransform)Items[i].transform;
            trans.sizeDelta = new Vector2(ListContents.sizeDelta.x, itemHeight);
            trans.anchoredPosition = new Vector2(0f, i * -itemHeight + i * -ItemSpace);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ItemPrefab    != null);
        Debug.Assert(ListContents  != null);
        Debug.Assert(BtnSpawn      != null);
        Debug.Assert(BtnSwitchTeam != null);
        Debug.Assert(BtnNextCamera != null);
        Debug.Assert(Map           != null);

        // For some reason, we have to trigger the volume in order
        // for it to be actually active...
        Game.CharSelectPPVolume.gameObject.SetActive(false);
        Game.CharSelectPPVolume.gameObject.SetActive(true);

        BtnSpawn.onClick.AddListener(SpawnClicked);
        BtnSwitchTeam.onClick.AddListener(SwitchTeamClicked);
        BtnNextCamera.onClick.AddListener(NextCameraClicked);
        Map.OnCPSelect += OnCPSelected;

        PhxCommandpost[] cps = Scene.GetCommandPosts();
        for (int i = 0; i < cps.Length; ++i)
        {
            if (cps[i].Team == Match.Player.Team)
            {
                Map.SelectCP(cps[i]);
                break;
            }
        }

        UpdateCharacterList();
    }

    void OnCPSelected(PhxCommandpost cp)
    {
        if (Scene.GetCPCameraTransform(cp, out PhxTransform t))
        {
            Game.Camera.transform.position = t.Position;
            Game.Camera.transform.rotation = t.Rotation;
        }

        SpawnCP = cp;
    }
    
    void SpawnClicked()
    {
        if (CurrentSelection != null && SpawnCP != null)
        {
            Match.SpawnPlayer(CurrentSelection, SpawnCP);
        }
    }

    void SwitchTeamClicked()
    {
        Match.Player.Team = Match.Player.Team == 1 ? 2 : 1;
        UpdateCharacterList();

        PhxCommandpost cp = null;
        PhxCommandpost[] cps = Scene.GetCommandPosts();
        for (int i = 0; i < cps.Length; ++i)
        {
            cps[i].UpdateColor();
            cps[i].ChangeColorIcon();
            if (cps[i].Team == Match.Player.Team) { cp = cps[i]; }
        }

        if (cp != null)
        {
            Map.SelectCP(cp);
        } else
        {
            //Not sure what to do in this case
        }
    }

    void NextCameraClicked()
    {
        Camera.Fixed(Scene.GetNextCameraShot());
    }
}
