using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LibSWBF2.Enums;

public class PhxCharacterSelect : PhxMenuInterface
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeScene RTS => PhxGameRuntime.GetScene();
    static PhxGameMatch MTC => PhxGameRuntime.GetMatch();
    static PhxCamera CAM => PhxGameRuntime.GetCamera();

    [Header("References")]
    public PhxUIMap Map;
    public PhxCharacterItem ItemPrefab;
    public RectTransform ListContents;
    public Button BtnSpawn;
    public Button BtnNextCamera;

    [Header("Settings")]
    public float ItemSpace = 5.0f;
    public float MaxItemHeight = 200f;

    List<PhxCharacterItem> Items   = new List<PhxCharacterItem>();
    PhxClass CurrentSelection = null;
    List<IPhxControlableInstance> UnitPreviews = new List<IPhxControlableInstance>();
    PhxCommandpost SpawnCP;

    static int nameCounter = 0;


    public override void Clear()
    {
        for (int i = 0; i < UnitPreviews.Count; ++i)
        {
            Destroy(UnitPreviews[i].GetInstance().gameObject);
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
        IPhxControlableInstance preview = RTS.CreateInstance(cl, cl.Name+"_CSP" + nameCounter++, Vector3.zero, Quaternion.identity, false, GAME.CharSelectTransform) as IPhxControlableInstance;
        preview.Fixate();
        UnitPreviews.Add(preview);

        PhxCharacterItem item = Instantiate(ItemPrefab, ListContents);
        item.OnClicked += () =>
        {
            SetActive(item, cl, preview);
        };

        item.SetHeaderText(cl.LocalizedName);

        string detailText = "";
        //PhxMultiProp weapons = cl.P.Get<PhxMultiProp>("WeaponName");
        //foreach (object[] weap in weapons.Values)
        //{
        //    PhxClass weapClass = RTS.GetClass(weap[0] as string);
        //    if (weapClass != null)
        //    {
        //        detailText += weapClass.LocalizedName + '\n';
        //    }
        //}
        item.SetDetailText(detailText);

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
        Debug.Assert(BtnNextCamera != null);
        Debug.Assert(Map           != null);

        // For soem reason, we have to trigger the volume in order
        // for it to be actually active...
        GAME.CharSelectPPVolume.gameObject.SetActive(false);
        GAME.CharSelectPPVolume.gameObject.SetActive(true);

        BtnSpawn.onClick.AddListener(SpawnClicked);
        BtnNextCamera.onClick.AddListener(NextCameraClicked);
        Map.OnCPSelect += OnCPSelected;

        PhxCommandpost[] cps = RTS.GetCommandPosts();
        for (int i = 0; i < cps.Length; ++i)
        {
            if (cps[i].Team == MTC.Player.Team)
            {
                Map.SelectCP(cps[i]);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCPSelected(PhxCommandpost cp)
    {
        //Debug.Log($"Selected CP '{cp.name}'");
        SpawnCP = cp;
    }
    
    void SpawnClicked()
    {
        if (SpawnCP != null)
        {
            MTC.SpawnPlayer(CurrentSelection, SpawnCP);
        }
    }

    void NextCameraClicked()
    {
        CAM.Fixed(RTS.GetNextCameraShot());
    }
}
