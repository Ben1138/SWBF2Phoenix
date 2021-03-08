using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SWBFButton : MonoBehaviour
{
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
            float intensity = Mathf.Sin(Time.timeSinceLevelLoad * UISettings.FlickerSpeed) * UISettings.FlickerIntensity + (1.0f - UISettings.FlickerIntensity);
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

    public void OnPointerEnter()
    {
        bIsHovering = true;
        GameRuntime.Instance.PlayUISound(HoverSound, 1.4f);
    }

    public void OnPointerExit()
    {
        bIsHovering = false;
    }

    public void OnClick()
    {
        GameRuntime.Instance.PlayUISound(ClickSound, 1.1f);
    }
}
