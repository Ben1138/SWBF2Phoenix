using UnityEngine;

public class PhxAnimTest : PhxUnityScript
{
    static PhxMatch Match => PhxGame.GetMatch();

    public Transform AITarget;
    public int Width;
    public int Height;
    public float Padding;

    public bool SpawnPlayer;
    public bool SpawnMobs;
    public bool EnableReload = true;
    public float ForwardMultiplier;

    int NameCounter;

    IPhxControlableInstance[] Instances;


    public override void ScriptInit()
    {
        PhxLuaAPI.ReadDataFile("ingame.lvl");
        PhxLuaAPI.ReadDataFile("side/rep.lvl",
            //"rep_inf_ep2_rifleman",
            //"rep_inf_ep2_rocketeer",
            //"rep_inf_ep2_sniper",
            //"rep_inf_ep2_engineer",
            //"rep_inf_ep2_jettrooper",
            //"rep_inf_ep3_rifleman",
            //"rep_inf_ep3_rocketeer",
            //"rep_inf_ep3_sniper",
            //"rep_inf_ep3_sniper_felucia",
            //"rep_inf_ep3_engineer",
            //"rep_inf_ep3_officer",
            //"rep_inf_ep3_jettrooper",
            "rep_hero_obiwan"
        );

        //PhxLuaAPI.ReadDataFile("side/cis.lvl",
        //    "cis_inf_rifleman",
        //    "cis_inf_rocketeer",
        //    "cis_inf_sniper",
        //    "cis_inf_marine"
        //);
    }

    public override void ScriptPostLoad()
    {
        Debug.Log("Loaded.");
        PhxScene scene = PhxGame.GetScene();

        //string[] classNames =
        //{
        //    //"rep_inf_ep2_rifleman",
        //    //"rep_inf_ep2_rocketeer",
        //    //"rep_inf_ep2_sniper",
        //    //"rep_inf_ep2_engineer",
        //    //"rep_inf_ep2_jettrooper",
        //    //"rep_inf_ep3_rifleman",
        //    "rep_inf_ep3_rocketeer",
        //    //"rep_inf_ep3_sniper",
        //    //"rep_inf_ep3_sniper_felucia",
        //    //"rep_inf_ep3_engineer",
        //    //"rep_inf_ep3_officer",
        //    //"rep_inf_ep3_jettrooper",
        //};

        //PhxClass[] classes = new PhxClass[classNames.Length];

        //for (int i = 0; i < classes.Length; ++i)
        //{
        //    classes[i] = scene.GetClass(classNames[i]);
        //    if (classes[i] == null)
        //    {
        //        Debug.LogError($"Cannot find class '{classNames[i]}'!");
        //        return;
        //    }
        //}

        //if (SpawnMobs)
        //{
        //    Instances = new PhxSoldier[Width * Height];
        //    for (int x = 0; x < Width; ++x)
        //    {
        //        for (int y = 0; y < Height; ++y)
        //        {
        //            Vector3 pos = new Vector3(x * -Padding, 0f, y * -Padding);
        //            pos.x += Random.Range(-0.2f, 0.2f);
        //            pos.z += Random.Range(-0.2f, 0.2f);

        //            PhxClass spawnClass = classes[Random.Range(0, classes.Length)];

        //            int idx = x + (Width * y);
        //            Instances[idx] = Match.SpawnAI<PhxAnimTestController>(spawnClass, pos, Quaternion.identity);
        //            Instances[idx].Fixate();
        //            PhxAnimTestController ai = Instances[idx].GetController() as PhxAnimTestController;
        //            ai.TestAim = AITarget;
        //        }
        //    }
        //}

        if (SpawnPlayer)
        {
            Match.SpawnPlayer(scene.GetClass("rep_hero_obiwan"), transform.position, Quaternion.identity);
        }
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
          
    }
}

