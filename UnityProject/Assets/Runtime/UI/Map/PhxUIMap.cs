using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PhxUIMap : MonoBehaviour
{
    PhxGameRuntime GAME => PhxGameRuntime.Instance;
    PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    PhxGameMatch MATCH => PhxGameRuntime.GetMatch();


    [Header("References")]
    public Button CPButtonPrefab;

    [Header("Settings")]
    public bool ClipableCPs = false;
    public float CPUpdateFreq = 10f;
    public float CPSelectAnimSpeed = 1f;

    public Action<PhxCommandpost> OnCPSelect;

    Material MapMat;
    float RefreshTimer;

    PhxCommandpost[] CommandPosts;
    Vector4[] CPPositions = new Vector4[32];
    Color[] CPColors = new Color[64];
    bool[] CPSelected = new bool[64];
    float[] CPSelectAnim = new float[64];
    int CPCount = 0;

    AudioClip CPSelectSound;
    Button[] CPButtons = new Button[64];


    public void SelectCP(PhxCommandpost cp)
    {
        int idx = Array.IndexOf(CommandPosts, cp);
        if (idx >= 0)
        {
            SelectCP(idx);
        }
    }

    public void SelectCP(int idx)
    {
        Debug.Assert(idx >= 0 && idx < CommandPosts.Length);
        Array.Clear(CPSelected, 0, CPSelected.Length);
        CPSelected[idx] = true;
        OnCPSelect?.Invoke(CommandPosts[idx]);

        GAME.PlayUISound(CPSelectSound, 1.4f);
    }

    void Awake()
    {
        CommandPosts = SCENE.GetCommandPosts();
        RefreshTimer = CPUpdateFreq;
    }

    void Start()
    {
        RawImage image = GetComponent<RawImage>();
        Debug.Assert(image != null);

        MapMat = image.materialForRendering;
        Debug.Assert(MapMat != null);

        CPSelectSound = SoundLoader.LoadSound("ui_menumove");

        // TODO: Load texture from "MapTexture" property specified in PhxCommandpost class
        Texture2D cpTexture = TextureLoader.Instance.ImportUITexture("hud_flag_icon");
        MapMat.SetTexture("_CPTex", cpTexture);
        MapMat.SetTexture("_MapTex", SCENE.MapTexture);

        if (ClipableCPs)
        {
            for (int i = 0; i < CPButtons.Length; ++i)
            {
                CPButtons[i] = Instantiate(CPButtonPrefab, transform);
                CPButtons[i].gameObject.SetActive(false);

                RectTransform t = CPButtons[i].transform as RectTransform;
                t.sizeDelta = new Vector2(cpTexture.width, cpTexture.height);

                int idx = i;
                Button btn = CPButtons[i].GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    PhxCommandpost cp = CommandPosts[idx];
                    if (cp.Team == MATCH.Player.Team)
                    {
                        SelectCP(idx);
                    }
                });
            }
        }
    }

    void Update()
    {
        RefreshTimer += Time.deltaTime;
        if (CPUpdateFreq != 0f && RefreshTimer > CPUpdateFreq)
        {
            if (ClipableCPs)
            {
                for (int i = 0; i < CPCount; ++i)
                {
                    CPButtons[i].gameObject.SetActive(false);
                }
            }

            CommandPosts = SCENE.GetCommandPosts();
            CPCount = CommandPosts.Length;

            Vector2 positionSum = Vector2.zero;
            Vector2 posMin = Vector2.positiveInfinity;
            Vector2 posMax = Vector2.negativeInfinity;
            for (int i = 0; i < CPCount; ++i)
            {
                int cpIdx = i / 2;
                int cpVecIdx = (i % 2) * 2;

                Vector2 pos = new Vector2(CommandPosts[i].transform.position.x, CommandPosts[i].transform.position.z);
                positionSum += pos;

                posMin.x = Mathf.Min(posMin.x, pos.x);
                posMin.y = Mathf.Min(posMin.y, pos.y);
                posMax.x = Mathf.Max(posMax.x, pos.x);
                posMax.y = Mathf.Max(posMax.y, pos.y);

                CPPositions[cpIdx][cpVecIdx + 0] = pos.x;
                CPPositions[cpIdx][cpVecIdx + 1] = pos.y;
            }

            // zoom is in world units per pixel
            float zoom = (posMax - posMin).magnitude * 1.25f;

            // Take the of all cp positions mean and flip the axis
            Vector2 mapOffset = positionSum  / - CPCount;

            if (ClipableCPs)
            {
                RectTransform rectT = transform as RectTransform;

                for (int i = 0; i < CPCount; ++i)
                {
                    int cpIdx = i / 2;
                    int cpVecIdx = (i % 2) * 2;

                    Vector2 cpWorldPos = new Vector2(CPPositions[cpIdx][cpVecIdx], CPPositions[cpIdx][cpVecIdx + 1]);
                    Vector2 cpMapPos = ((cpWorldPos + mapOffset) / zoom) + new Vector2(0.5f, 0.5f);

                    CPButtons[i].gameObject.SetActive(true);
                    RectTransform t = CPButtons[i].transform as RectTransform;
                    t.anchoredPosition = new Vector3(cpMapPos.x * rectT.rect.width, cpMapPos.y * rectT.rect.height, 0f);
                }
            }
            

            MapMat.SetFloat("_Zoom", zoom);

            MapMat.SetFloat("_MapOffsetX", mapOffset.x);
            MapMat.SetFloat("_MapOffsetY", mapOffset.y);

            MapMat.SetVectorArray("_CPPositions", CPPositions);
            MapMat.SetFloat("_CPCount", CPCount);

            RefreshTimer = 0f;
        }

        for (int i = 0; i < CPCount; ++i)
        {
            CPColors[i] = MATCH.GetTeamColor(CommandPosts[i].Team);

            if (CPSelected[i])
            {
                CPSelectAnim[i] = Mathf.Clamp01(CPSelectAnim[i] + Time.deltaTime / CPSelectAnimSpeed);
            }
            else
            {
                CPSelectAnim[i] = Mathf.Clamp01(CPSelectAnim[i] - Time.deltaTime / CPSelectAnimSpeed);
            }
        }

        MapMat.SetColorArray("_CPColors", CPColors);
        MapMat.SetFloatArray("_CPSelected", CPSelectAnim);
    }
}
