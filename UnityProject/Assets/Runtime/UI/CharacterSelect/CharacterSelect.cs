using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Enums;

public class CharacterSelect : MonoBehaviour
{
    static RuntimeScene RTS => GameRuntime.GetScene();

    [Header("References")]
    public CharacterItem ItemPrefab;
    public RectTransform ListContents;

    [Header("Settings")]
    public float ItemSpace = 5.0f;
    public float MaxItemHeight = 200f;

    List<CharacterItem> Items   = new List<CharacterItem>();
    ISWBFClass CurrentSelection = null;


    public void Add(ISWBFClass cl)
    {
        if (cl.ClassType != EEntityClassType.GameObjectClass)
        {
            Debug.LogError($"Cannot add odf class '{cl.Name}' as item in character selection!");
            return;
        }

        CharacterItem item = Instantiate(ItemPrefab, ListContents);
        item.OnClicked += () =>
        {
            foreach (CharacterItem it in Items)
            {
                it.SetActive(false);
            }
            item.SetActive(true);
            CurrentSelection = cl;
        };

        item.SetHeaderText(cl.LocalizedName);

        string detailText = "";
        MultiProp weapons = cl.P.Get<MultiProp>("WeaponName");
        foreach (object[] weap in weapons)
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
            CurrentSelection = cl;
            item.SetActive(true);
        }
        else
        {
            item.SetActive(false);
        }

        RefreshItems();
    }

    void RefreshItems()
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
