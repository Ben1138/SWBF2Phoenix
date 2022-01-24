using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhxListBoxItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    public RawImage ImageCheck;
    public RawImage Icon1;
    public RawImage Icon2;
    public Text Txt;

    [Header("Settings")]
    public Color ItemColor;
    public Color SpecialItemColor;
    public Color SelectedColor;
    public bool IsSpecialItem;
    public float FlickerSpeed = 40.0f;

    public bool IsChecked { get; private set; }

    public Action<PointerEventData> OnClick;
    public Action<bool> OnCheckChanged;

    Texture2D TexChecked;
    Texture2D TexUnchecked;
    Image Sprite;
    AudioClip HoverSound;
    bool IsCheckable;
    bool IsHovering;

    public void SetSelected(bool bSelected)
    {
        if (!IsCheckable)
        {
            Txt.color = bSelected ? SelectedColor : (IsSpecialItem ? SpecialItemColor : ItemColor);
        }
        PhxGame.Instance.PlayUISound(HoverSound, 1.4f);
    }

    public void SetText(string txt)
    {
        Txt.text = txt;
    }

    public void SetIsCheckable(bool bCheckable)
    {
        IsCheckable = bCheckable;
        IsChecked = false;

        if (IsCheckable)
        {
            ImageCheck.gameObject.SetActive(true);
            Txt.rectTransform.sizeDelta = new Vector2(50, 0);
            TexChecked = TextureLoader.Instance.ImportUITexture("check_yes");
            TexUnchecked = TextureLoader.Instance.ImportUITexture("check_no");
            ImageCheck.texture = TexUnchecked;
        }
        else
        {
            ImageCheck.gameObject.SetActive(false);
            Txt.rectTransform.sizeDelta = new Vector2(10, 0);
        }
    }

    public void SetIcon(Texture2D icon)
    {
        Icon1.gameObject.SetActive(true);
        Icon1.texture = icon;
    }

    public void SetIcon2(Texture2D icon)
    {
        Icon2.gameObject.SetActive(true);
        Icon2.texture = icon;
    }

    public void SetChecked(bool bChecked)
    {
        IsChecked = bChecked;
        ImageCheck.texture = IsChecked ? TexChecked : TexUnchecked;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsCheckable)
        {
            Sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        PhxGame.Instance.PlayUISound(HoverSound, 1.4f);
        IsHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsCheckable)
        {
            ImageCheck.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            Sprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }

        Color col = Txt.color;
        col.a = 1.0f;
        Txt.color = col;

        IsHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsCheckable)
        {
            SetChecked(!IsChecked);
            OnCheckChanged?.Invoke(IsChecked);
        }
        PhxGame.Instance.PlayUISound(HoverSound, 1.4f);
        OnClick?.Invoke(eventData);
    }

    void Start()
    {
        Sprite = GetComponent<Image>();
        Debug.Assert(ImageCheck != null);
        Debug.Assert(Icon1 != null);
        Debug.Assert(Icon2 != null);
        Debug.Assert(Txt != null);

        HoverSound = SoundLoader.LoadSound("ui_menumove");
    }

    void Update()
    {
        if (IsHovering)
        {
            // TODO: how about doing this in a shader?
            float alpha = (Mathf.Sin(Time.timeSinceLevelLoad * FlickerSpeed) + 1.5f) / 2.5f;

            Color col = Txt.color;
            col.a = alpha;
            Txt.color = col;

            if (IsCheckable)
            {
                col = ImageCheck.color;
                col.a = alpha;
                ImageCheck.color = col;
            }
            else
            {
                col = Sprite.color;
                col.a = alpha;
                Sprite.color = col;
            }
        }
    }
}
