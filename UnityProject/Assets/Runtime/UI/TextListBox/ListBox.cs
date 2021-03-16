using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
    [Header("References")]
    public GameObject ItemPrefab;
    public Transform Content;

    [Header("Settings")]
    public bool CheckList = false;
    public float Spacing = 10.0f;

    public Action<int> OnSelect;
    public int CurrentSelection { get; private set; } = -1;

    List<ListBoxItem> Items = new List<ListBoxItem>();

    public ListBoxItem AddItem(string itemStr, bool bSpecialItem=false)
    {
        GameObject itemInst = Instantiate(ItemPrefab, Content);
        RectTransform itemTrans = itemInst.transform as RectTransform;
        RectTransform conTrans = Content.transform as RectTransform;

        float yPos = -itemTrans.sizeDelta.y * Items.Count - Spacing * Items.Count;
        itemTrans.anchoredPosition = new Vector2(0.0f, yPos);

        ListBoxItem item  = itemInst.GetComponent<ListBoxItem>();
        int idx = Items.Count;
        item.OnClick += (PointerEventData eventData) => 
        {
            Select(idx);
        };
        item.SetIsCheckable(CheckList);
        item.IsSpecialItem = bSpecialItem;
        item.SetText(itemStr);
        item.SetSelected(false);

        yPos -= itemTrans.sizeDelta.y;
        conTrans.sizeDelta = new Vector2(conTrans.sizeDelta.x, -yPos);

        Items.Add(item);
        return item;
    }

    public void Select(int idx)
    {
        if (idx == CurrentSelection) return;

        if (idx < 0 || idx >= Items.Count)
        {
            Debug.LogError($"List box item index '{idx}' is out of range '{Items.Count}'!");
            return;
        }

        if (CurrentSelection >= 0)
        {
            Items[CurrentSelection].SetSelected(false);
        }
        CurrentSelection = idx;
        Items[CurrentSelection].SetSelected(true);

        OnSelect?.Invoke(CurrentSelection);
    }

    public int[] GetCheckedIndices()
    {
        List<int> check = new List<int>();
        for (int i = 0; i < Items.Count; ++i)
        {
            if (Items[i].IsChecked)
            {
                check.Add(i);
            }
        }
        return check.ToArray();
    }

    public void Clear()
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            Destroy(Items[i].gameObject);
        }
        Items.Clear();
        CurrentSelection = -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ItemPrefab != null);
        Debug.Assert(Content    != null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
