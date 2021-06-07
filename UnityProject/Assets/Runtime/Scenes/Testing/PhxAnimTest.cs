using UnityEngine;

public class PhxAnimTest : MonoBehaviour
{
    public PhxCamera Cam;

    public int Width;
    public int Height;
    public float Padding;

    public bool SpawnPlayer;
    public bool SpawnMobs;
    public bool EnableReload = true;
    public float ForwardMultiplier;

    PhxRuntimeEnvironment Env;
    int NameCounter;

    PhxSoldier[] Instances;
    float[] ReloadTimers;
    float[] ForwardOffset;

    void Start()
    {
        WorldLoader.UseHDRP = true;
        MaterialLoader.UseHDRP = true;

        Env = PhxRuntimeEnvironment.Create(PhxGameRuntime.GamePath / "GameData/data/_lvl_pc", null, false);
        Env.ScheduleLVLRel("load/common.lvl");
        Env.ScheduleLVLRel("side/rep.lvl");
        Env.ScheduleLVLRel("ingame.lvl");
        Env.OnLoaded += Loaded;
        Env.Run(null);
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

        PhxRuntimeScene scene = Env.GetScene();
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
            ReloadTimers = new float[Width * Height];
            ForwardOffset = new float[Width * Height];

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Vector3 pos = new Vector3(x * -Padding, 0f, y * -Padding);
                    pos.x += Random.Range(-0.2f, 0.2f);
                    pos.y += Random.Range(-0.2f, 0.2f);

                    PhxClass spawnClass = classes[Random.Range(0, classes.Length)];

                    int idx = x + (Width * y);
                    Instances[idx] = scene.CreateInstance(spawnClass, "instance" + NameCounter++, pos, Quaternion.identity) as PhxSoldier;
                    ReloadTimers[idx] = Random.Range(1.0f, 10f);
                    ForwardOffset[idx] = Random.Range(0f, 3.1416f * 2f);
                }
            }
        }

        if (SpawnPlayer)
        {
            PhxSoldier pawn = Env.GetScene().CreateInstance(classes[5], "player" + NameCounter++, transform.position, Quaternion.identity) as PhxSoldier;
            if (pawn == null)
            {
                Debug.LogError($"Given spawn class '{classes[5].Name}' is not a soldier!");
                return;
            }

            pawn.gameObject.layer = 3;
            pawn.Controller = new PhxPlayerController();
            pawn.Controller.Pawn = pawn;
            Cam.Follow(pawn);
        }
    }

    void Update()
    {
        Env.Update();

        if (Instances != null)
        {
            for (int i = 0; i < Instances.Length; ++i)
            {
                if (EnableReload)
                {
                    ReloadTimers[i] -= Time.deltaTime;
                    if (ReloadTimers[i] < 0f)
                    {
                        ReloadTimers[i] = Random.Range(0.5f, 5f);
                        Instances[i].Animator.SetState(1, 0);
                    }
                }
                Instances[i].Animator.Forward = Mathf.Sin(Time.timeSinceLevelLoad * ForwardMultiplier + ForwardOffset[i]);
            }
        }

        PhxPlayerController.Instance?.Update(Time.deltaTime);
    }

    void FixedUpdate()
    {
        Env.FixedUpdate(Time.fixedDeltaTime);        
    }
}

