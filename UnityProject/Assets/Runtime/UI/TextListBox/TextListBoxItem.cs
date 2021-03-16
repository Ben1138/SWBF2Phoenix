using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextListBoxItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Settings")]
    public Color ItemColor;
    public Color SpecialItemColor;
    public Color SelectedColor;
    public bool IsSpecialItem;

    public Action<PointerEventData> OnClick;

    Image Sprite;
    Text Text;
    AudioClip HoverSound;

    public void SetSelected(bool bSelected)
    {
        if (Text == null)
        {
            Text = GetComponentInChildren<Text>();
        }
        Text.color = bSelected ? SelectedColor : (IsSpecialItem ? SpecialItemColor : ItemColor);
        GameRuntime.Instance.PlayUISound(HoverSound, 1.4f);
    }

    public void SetText(string txt)
    {
        if (Text == null)
        {
            Text = GetComponentInChildren<Text>();
        }
        Text.text = txt;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        GameRuntime.Instance.PlayUISound(HoverSound, 1.4f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Sprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(eventData);
    }

    void Start()
    {
        Sprite = GetComponent<Image>();
        Text = GetComponentInChildren<Text>();
        Debug.Assert(Text != null);

        HoverSound = SoundLoader.LoadSound("ui_menumove");
    }

    void Update()
    {
        
    }
}
