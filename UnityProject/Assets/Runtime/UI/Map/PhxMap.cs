using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PhxMap : MonoBehaviour
{
    public bool UpdateCPs = true;

    PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    PhxGameMatch MATCH => PhxGameRuntime.GetMatch();

    Material MapMat;
    const float RefreshTime = 10f;
    float RefreshTimer = RefreshTime;

    PhxCommandpost[] CommandPosts;
    Vector4[] CPPositions = new Vector4[32];
    Color[] CPColors = new Color[64];
    int CPCount = 0;


    void Start()
    {
        RawImage image = GetComponent<RawImage>();
        Debug.Assert(image != null);

        MapMat = image.materialForRendering;
        Debug.Assert(MapMat != null);

        // TODO: Load texture from "MapTexture" property specified in PhxCommandpost class
        MapMat.SetTexture("_CPTex", TextureLoader.Instance.ImportUITexture("hud_flag_icon"));
        MapMat.SetTexture("_MapTex", SCENE.MapTexture);
    }

    void Update()
    {
        RefreshTimer += Time.deltaTime;
        if (UpdateCPs && RefreshTimer > RefreshTime)
        {
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

                CPColors[i] = MATCH.GetTeamColor(CommandPosts[i].Team);
            }

            float zoom = (posMax - posMin).magnitude;
            MapMat.SetFloat("_Zoom", zoom);

            // Take the of all cp positions mean and flip the axis
            positionSum /= -CPCount;

            MapMat.SetFloat("_MapOffsetX", positionSum.x);
            MapMat.SetFloat("_MapOffsetY", positionSum.y);

            MapMat.SetVectorArray("_CPPositions", CPPositions);
            MapMat.SetColorArray("_CPColors", CPColors);
            MapMat.SetFloat("_CPCount", CPCount);

            RefreshTimer = 0f;
        }
        else
        {
            for (int i = 0; i < CPCount; ++i)
            {
                CPColors[i] = MATCH.GetTeamColor(CommandPosts[i].Team);
            }
            MapMat.SetColorArray("_CPColors", CPColors);
        }
    }
}
