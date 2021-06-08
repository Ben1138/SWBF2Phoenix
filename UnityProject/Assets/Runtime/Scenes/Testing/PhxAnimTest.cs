using UnityEngine;

public class PhxAnimTest : PhxUnityScene
{
    static PhxGameMatch MATCH => PhxGameRuntime.GetMatch();

    public int Width;
    public int Height;
    public float Padding;

    public bool SpawnPlayer;
    public bool SpawnMobs;
    public bool EnableReload = true;
    public float ForwardMultiplier;

    int NameCounter;

    IPhxControlableInstance[] Instances;


    void Awake()
    {
        ScheduleLVLs = new string[]
        {
            "side/rep.lvl"
        };

        PhxGameRuntime.Instance.OnMapLoaded += Loaded;
    }

    void Loaded()
    {
        Debug.Log("Loaded.");

        string[] classNames =
        {
            "rep_inf_ep2_rifleman",
            "rep_inf_ep2_rocketeer",
            "rep_inf_ep2_sniper",
            "rep_inf_ep2_engineer",
            "rep_inf_ep2_jettrooper",
            "rep_inf_ep3_rifleman",
            "rep_inf_ep3_rocketeer",
            "rep_inf_ep3_sniper",
            "rep_inf_ep3_sniper_felucia",
            "rep_inf_ep3_engineer",
            "rep_inf_ep3_officer",
            "rep_inf_ep3_jettrooper",
        };

        PhxRuntimeScene scene = PhxGameRuntime.GetScene();
        PhxClass[] classes = new PhxClass[classNames.Length];

        for (int i = 0; i < classes.Length; ++i)
        {
            classes[i] = scene.GetClass(classNames[i]);
            if (classes[i] == null)
            {
                Debug.LogError($"Cannot find class '{classNames[i]}'!");
                return;
            }
        }

        if (SpawnMobs)
        {
            Instances = new PhxSoldier[Width * Height];
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Vector3 pos = new Vector3(x * -Padding, 0f, y * -Padding);
                    pos.x += Random.Range(-0.2f, 0.2f);
                    pos.y += Random.Range(-0.2f, 0.2f);

                    PhxClass spawnClass = classes[Random.Range(0, classes.Length)];

                    int idx = x + (Width * y);
                    Instances[idx] = MATCH.SpawnAI<PhxAnimTestController>(spawnClass, pos, Quaternion.identity);
                    Instances[idx].Fixate();
                }
            }
        }

        if (SpawnPlayer)
        {
            MATCH.SpawnPlayer(classes[5], transform.position, Quaternion.identity);
        }
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
          
    }
}

