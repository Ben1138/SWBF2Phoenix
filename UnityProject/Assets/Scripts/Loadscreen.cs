using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loadscreen : MonoBehaviour
{
    public Image LoadImage;
    public Text ProgressText;


    public void SetLoadImage(Texture2D image)
    {
        LoadImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(LoadImage != null);
        Debug.Assert(ProgressText != null);
    }

    // Update is called once per frame
    void Update()
    {
        RuntimeEnvironment rt = GameRuntime.GetEnvironment();
        if (rt != null)
        {
            ProgressText.text = string.Format("{0:0.} %", rt.GetLoadingProgress() * 100.0f);
        }
    }
}
