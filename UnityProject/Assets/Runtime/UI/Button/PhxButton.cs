using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhxButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    static PhxEnvironment ENV { get { return PhxGame.GetEnvironment(); } }

    public string LocalizePath;

    [Header("References")]
    public Text Text;
    public RawImage Left;
    public RawImage Center;
    public RawImage Right;

    bool bIsHovering;
    AudioClip HoverSound;
    AudioClip ClickSound;


    void Start()
    {
        Debug.Assert(Left   != null);
        Debug.Assert(Center != null);
        Debug.Assert(Right  != null);

        if (!string.IsNullOrEmpty(LocalizePath))
        {
            Text.text = ENV.GetLocalized(LocalizePath);
        }

        Left.texture   = TextureLoader.Instance.ImportUITexture("bf2_buttons_botleft");
        Center.texture = TextureLoader.Instance.ImportUITexture("bf2_buttons_items_center");
        Right.texture  = TextureLoader.Instance.ImportUITexture("bf2_buttons_botright");

        HoverSound = SoundLoader.LoadSound("ui_menumove");
        ClickSound = SoundLoader.LoadSound("ui_planetzoom");
    }

    void Update()
    {
        if (bIsHovering)
        {
            float intensity = Mathf.Sin(Time.timeSinceLevelLoad * PhxUISettings.FlickerSpeed) * PhxUISettings.FlickerIntensity + (1.0f - PhxUISettings.FlickerIntensity);
            Color col = new Color(intensity, intensity, intensity, 1.0f);
            Left.color   = col;
            Center.color = col;
            Right.color  = col;
        }
        else
        {
            Left.color   = Color.white;
            Center.color = Color.white;
            Right.color  = Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bIsHovering = true;
        PhxGame.Instance.PlayUISound(HoverSound, 1.4f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bIsHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PhxGame.Instance.PlayUISound(ClickSound, 1.1f);
    }
}
