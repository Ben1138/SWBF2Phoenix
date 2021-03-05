using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class GC_commandpost : MonoBehaviour
{
    public static GameObject lol;
    public float NeutralizeTime = 1.0f;
    public float CaptureTime = 1.0f;

    Collision CaptureRegion;
    Collision ControlRegion;

    GameObject Holo;

    public void Init(Instance inst)
    {
        ClassLoader.Instance.AssignProp(inst, "NeutralizeTime", ref NeutralizeTime);
        ClassLoader.Instance.AssignProp(inst, "NeutralizeTime", ref CaptureTime);

        Transform hpHolo = transform.Find("com_bldg_controlzone/hp_hologram");
        if (hpHolo != null)
        {
            GameObject holoPrefab = Resources.Load<GameObject>("cp_holo");
            Holo = Instantiate(holoPrefab, hpHolo);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
