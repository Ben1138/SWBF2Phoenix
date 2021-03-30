using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LibSWBF2.Enums;

public class CharacterSelect : IMenu
{
    static GameRuntime GAME => GameRuntime.Instance;
    static RuntimeScene RTS => GameRuntime.GetScene();
    static GameMatch MTC => GameRuntime.GetMatch();

    [Header("References")]
    public CharacterItem ItemPrefab;
    public RectTransform ListContents;
    public Button BtnSpawn;
        
    [Header("Settings")]
    public float ItemSpace = 5.0f;
    public float MaxItemHeight = 200f;

    List<CharacterItem> Items   = new List<CharacterItem>();
    ISWBFClass CurrentSelection = null;
    List<ISWBFInstance> UnitPreviews = new List<ISWBFInstance>();


    public override void Clear()
    {
        for (int i = 0; i < UnitPreviews.Count; ++i)
        {
            UnitPreviews[i].gameObject.SetActive(false);
            Destroy(UnitPreviews[i]);
        }
        UnitPreviews.Clear();
    }

    public void Add(ISWBFClass cl)
    {
        if (cl.ClassType != EEntityClassType.GameObjectClass)
        {
            Debug.LogError($"Cannot add odf class '{cl.Name}' as item to character selection!");
            return;
        }

        // CSP = Char Select Preview
        ISWBFInstance preview = RTS.CreateInstance(cl, cl.Name+"_CSP" + nameCounter++, Vector3.zero, Quaternion.identity, GAME.CharSelectTransform);
        UnitPreviews.Add(preview);

        CharacterItem item = Instantiate(ItemPrefab, ListContents);
        item.OnClicked += () =>
        {
            SetActive(item, cl, preview);
        };

        item.SetHeaderText(cl.LocalizedName);

        string detailText = "";
        MultiProp weapons = cl.P.Get<MultiProp>("WeaponName");
        foreach (object[] weap in weapons.Values)
        {
            ISWBFClass weapClass = RTS.GetClass(weap[0] as string);
            if (weapClass != null)
            {
                detailText += weapClass.LocalizedName + '\n';
            }
        }
        item.SetDetailText(detailText);

        Items.Add(item);

        if (CurrentSelection == null)
        {
            SetActive(item, cl, preview);
        }
        else
        {
            item.SetActive(false);
            preview.gameObject.SetActive(false);
        }

        ReCalcItemSize();
    }

    void SetActive(CharacterItem item, ISWBFClass cl, ISWBFInstance preview)
    {
        foreach (CharacterItem it in Items)
        {
            it.SetActive(false);
        }
        item.SetActive(true);
        CurrentSelection = cl;

        foreach (ISWBFInstance inst in UnitPreviews)
        {
            inst.gameObject.SetActive(false);
        }
        preview.gameObject.SetActive(true);

        ISWBFSelectableCharacter animPreview = preview as ISWBFSelectableCharacter;
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
        Debug.Assert(ItemPrefab   != null);
        Debug.Assert(ListContents != null);
        Debug.Assert(BtnSpawn     != null);

        // For soem reason, we have to trigger the volume in order
        // for it to be actually active...
        GAME.CharSelectPPVolume.gameObject.SetActive(false);
        GAME.CharSelectPPVolume.gameObject.SetActive(true);

        BtnSpawn.onClick.AddListener(SpawnClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static int nameCounter = 0;
    void SpawnClicked()
    {
        MTC.SpawnPlayer(CurrentSelection);
    }
}
