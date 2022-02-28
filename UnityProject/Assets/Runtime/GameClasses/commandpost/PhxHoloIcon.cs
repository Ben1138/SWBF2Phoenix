using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxHoloIcon : PhxInstance<PhxHoloIcon.ClassProperties>, IPhxTickable
{
    PhxMatch Match => PhxGame.GetMatch();
    PhxScene Scene => PhxGame.GetScene();

    public class ClassProperties : PhxClass
    {
        public PhxProp<float> HoloSize = new PhxProp<float>(1.0f);
    }

    public static float RotationSpeed = 30.0f;
    public static UnityEngine.Material mat;
    public static UnityEngine.Material matEnemy;
    public GameObject obj;
    public float scale;
    public float counter;

    public override void Init()
    {
        if (mat == null || matEnemy == null)
        {
            mat = Resources.Load<UnityEngine.Material>("CPHoloIcon");
            matEnemy = Resources.Load<UnityEngine.Material>("CPHoloIconEnemy");
        }
    }

    public override void Destroy()
    {

    }

    public void Tick(float deltaTime)
    {

    }

    public void LoadIcon(GameObject icon, bool userTeam)
    {
        if (obj != null) { UnityEngine.Object.Destroy(obj); }
        if (icon == null) { return; }

        obj = Instantiate(icon, gameObject.transform.position,
                            gameObject.transform.rotation, gameObject.transform);

        obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

        Renderer renderer = gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (userTeam)
            {
                mats[i] = mat;
            }
            else
            {
                mats[i] = matEnemy;
            }
        }
        renderer.materials = mats;
    }

    public void ChangeColorIcon(bool userTeam)
    {
        Renderer renderer = gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
        if (renderer == null) { return; }

        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (userTeam)
            {
                mats[i] = mat;
            }
            else
            {
                mats[i] = matEnemy;
            }
        }
        renderer.materials = mats;
    }

    void Update()
    {
        if (obj != null)
        {
            obj.transform.Rotate(new Vector3(0.0f, RotationSpeed * Time.deltaTime, 0.0f));

            counter += Time.deltaTime;
            if (counter >= 5.0f)
            {
                scale = 1.8f;
                counter = 0.0f;
            }
            if (scale > 1.0f)
            {
                scale -= Time.deltaTime;
                mat.SetFloat("_Scale", scale);
                matEnemy.SetFloat("_Scale", scale);
            }
        }
    }
}