using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxLoadscreen : MonoBehaviour
{
    enum LSState
    {
        FadeIn,
        Steady,
        FadeOut
    }

    public CanvasGroup CVGroup;
    public RawImage LoadImage;
    public Image LoadIcon;
    bool ImageSet;
    float Percentage;
    float PercentageSpeed = 1.2f;
    float FadeScreenDuration = 0.5f;
    float FadeImageDuration = 0.5f;
    float FadeImagePlayback;
    float FadeScreenPlayback;
    LSState State;


    public void SetLoadImage(Texture2D image)
    {
        LoadImage.texture = image;
        ImageSet = true;
    }

    public void FadeOut()
    {
        FadeScreenPlayback = FadeScreenDuration;
        State = LSState.FadeOut;
        StartCoroutine(DelayedDestroy());
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(CVGroup != null);
        Debug.Assert(LoadImage != null);
        Debug.Assert(LoadIcon != null);
        State = LSState.FadeIn;
        FadeScreenPlayback = 0.0f;
        FadeScreenPlayback = 0.0f;
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(FadeScreenDuration);
        LoadIcon.material.SetFloat("_Percent", 0.0f);
        DestroyImmediate(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        PhxEnvironment rt = PhxGame.GetEnvironment();
        if (rt != null)
        {
            Percentage = Mathf.Lerp(Percentage, rt.GetLoadingProgress(), Time.deltaTime * PercentageSpeed);
            LoadIcon.material.SetFloat("_Percent", Percentage);
        }

        if (ImageSet)
        {
            if (FadeImagePlayback < FadeImageDuration)
            {
                FadeImagePlayback += Time.deltaTime;
            }

            LoadImage.color = Color.Lerp(Color.black, Color.white, FadeImagePlayback / FadeImageDuration);
        }

        if (State == LSState.FadeIn && FadeScreenPlayback < FadeScreenDuration)
        {
            FadeScreenPlayback += Time.deltaTime;
        }
        else if (State == LSState.FadeOut && FadeScreenPlayback > 0.0)
        {
            FadeScreenPlayback -= Time.deltaTime;
        }

        CVGroup.alpha = FadeScreenPlayback / FadeScreenDuration;
    }
}
