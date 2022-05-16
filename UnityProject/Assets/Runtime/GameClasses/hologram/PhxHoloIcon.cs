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
    public Material IconMaterial;
    public GameObject obj;
    public float scale;
    public float counter;

    public override void Init()
    {
        IconMaterial = new Material(Shader.Find("Unlit/HologramShader"));
        IconMaterial.SetFloat("_Scale", 1.0f);
    }

    public override void Destroy()
    {

    }

    public void Tick(float deltaTime)
    {
        if (obj != null)
        {
            obj.transform.Rotate(new Vector3(0.0f, RotationSpeed * Time.deltaTime, 0.0f));

            // TODO: This should be in vertex shader
            counter += deltaTime;
            if (counter >= 5.0f)
            {
                scale = 1.8f;
                counter = 0.0f;
            }
            if (scale > 1.0f)
            {
                scale -= deltaTime;
                IconMaterial.SetFloat("_Scale", scale);
            }
        }
    }

    public void LoadIcon(GameObject icon, int team)
    {
        if (obj != null) { UnityEngine.Object.Destroy(obj); }
        if (icon == null) { return; }

        obj = Instantiate(icon, gameObject.transform.position,
                            gameObject.transform.rotation, gameObject.transform);

        obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

        ChangeColorIcon(team);
    }

    public void ChangeColorIcon(int team)
    {
        Renderer renderer = gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
        if (renderer == null) { return; }

        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            Color color = Match.GetTeamColor(team);
            color.a = 0.4f;
            IconMaterial.SetColor("_Color", color);

            mats[i] = IconMaterial;
        }
        renderer.materials = mats;
    }

    public void Hide()
    {
        if(obj!=null)
            obj.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (obj != null)
            obj.gameObject.SetActive(true);
    }
}