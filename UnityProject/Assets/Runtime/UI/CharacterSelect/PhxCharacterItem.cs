using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhxCharacterItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    static PhxGame GAME => PhxGame.Instance;

    [Header("References")]
    public RawImage TopBarLeft;
    public RawImage TopBarCenter;
    public RawImage TopBarRight;
    public Image DetailBox;
    public Text HeaderText;
    public Text DetailText;

    [Header("Settings")]
    public float InactiveIntensity = 0.6f;
    public float FlickerSpeed = 10.0f;

    public Action OnClicked;

    AudioClip Sound;
    bool IsActive = false;
    bool IsHovering = false;


    public void SetActive(bool bIsActive)
    {
        float factor = bIsActive ? 1.0f : InactiveIntensity;
        Color col = new Color(factor, factor, factor, 1.0f);
        TopBarLeft.color   = col;
        TopBarCenter.color = col;
        TopBarRight.color  = col;
        DetailBox.color    = col;
        IsActive = bIsActive;
    }

    public void SetHeaderText(string txt)
    {
        HeaderText.text = txt;
    }

    public void SetDetailText(string txt)
    {
        DetailText.text = txt;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke();
        GAME.PlayUISound(Sound, 1.4f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovering = false;
        SetActive(IsActive); // refresh alpha after exit
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovering = true;
        GAME.PlayUISound(Sound, 1.4f);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(TopBarLeft != null);
        Debug.Assert(TopBarCenter != null);
        Debug.Assert(TopBarRight != null);
        Debug.Assert(HeaderText != null);
        Debug.Assert(DetailBox != null);
        Debug.Assert(DetailText != null);

        TopBarLeft.texture   = TextureLoader.Instance.ImportUITexture("bf2_buttons_topleft");
        TopBarCenter.texture = TextureLoader.Instance.ImportUITexture("bf2_buttons_title_center");
        TopBarRight.texture  = TextureLoader.Instance.ImportUITexture("bf2_buttons_topright");

        // kinda hacky, since Unity doesn't allow to change a sprites texture on the fly, but well...
        Texture2D boxTexSrc = TextureLoader.Instance.ImportUITexture("border_3_pieces");
        Texture2D boxTexDst = DetailBox.sprite.texture;
        boxTexDst.SetPixels32(boxTexSrc.GetPixels32());
        boxTexDst.Apply();

        Sound = SoundLoader.LoadSound("ui_menumove");
    }

    void Update()
    {
        if (!IsActive && IsHovering)
        {
            float factor = (Mathf.Sin(Time.timeSinceLevelLoad * FlickerSpeed) + 10f) / 12f;
            Color col = new Color(factor, factor, factor, 1.0f);
            TopBarLeft.color = col;
            TopBarCenter.color = col;
            TopBarRight.color = col;
            DetailBox.color = col;
        }
    }
}
