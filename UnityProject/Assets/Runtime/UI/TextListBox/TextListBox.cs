using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextListBox : MonoBehaviour
{
    [Header("References")]
    public GameObject ItemPrefab;
    public Transform Content;

    [Header("Settings")]
    public float Spacing = 10.0f;

    public int CurrentSelection { get; private set; } = -1;

    List<TextListBoxItem> Items = new List<TextListBoxItem>();

    public int AddItem(string itemStr, bool bSpecialItem=false)
    {
        GameObject itemInst = Instantiate(ItemPrefab, Content);
        RectTransform itemTrans = itemInst.transform as RectTransform;
        RectTransform conTrans = Content.transform as RectTransform;

        float yPos = -itemTrans.sizeDelta.y * Items.Count - Spacing * Items.Count;
        itemTrans.anchoredPosition = new Vector2(0.0f, yPos);

        TextListBoxItem item  = itemInst.GetComponent<TextListBoxItem>();
        int idx = Items.Count;
        item.OnClick += (PointerEventData eventData) => 
        {
            Select(idx);
        };
        item.IsSpecialItem = bSpecialItem;
        item.SetText(itemStr);
        item.SetSelected(false);

        yPos -= itemTrans.sizeDelta.y;
        conTrans.sizeDelta = new Vector2(conTrans.sizeDelta.x, -yPos);

        Items.Add(item);
        return Items.Count - 1;
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
    }

    public void Clear()
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            Destroy(Items[i].gameObject);
        }
        Items.Clear();
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
