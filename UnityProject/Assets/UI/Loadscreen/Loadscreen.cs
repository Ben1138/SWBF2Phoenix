using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loadscreen : MonoBehaviour
{
    enum LSState
    {
        FadeIn,
        Steady,
        FadeOut
    }

    public CanvasGroup CVGroup;
    public RawImage LoadImage;
    public Text ProgressText;
    bool ImageSet;
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
        Debug.Assert(ProgressText != null);
        State = LSState.FadeIn;
        FadeScreenPlayback = 0.0f;
        FadeScreenPlayback = 0.0f;
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(FadeScreenDuration);
        DestroyImmediate(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        RuntimeEnvironment rt = GameRuntime.GetEnvironment();
        if (rt != null)
        {
            ProgressText.text = string.Format("{0:0.} %", rt.GetLoadingProgress() * 100.0f);
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
