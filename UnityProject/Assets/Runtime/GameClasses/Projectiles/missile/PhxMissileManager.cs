using System.Collections.Generic;
using UnityEngine;

/*

public class PhxMissileManager : MonoBehaviour
{
    PhxGameRuntime Game => PhxGameRuntime.Instance;

    List<PhxMissile> Missiles = new List<PhxMissile>();
    Dictionary<PhxMissileClass, List<int>> MissileClassIndices = new Dictionary<PhxMissileClass, List<int>>();


    PhxMissile GetMissile(PhxMissileClass MissileClass)
    {
        if (!MissileClassIndices.TryGetValue(MissileClass, out List<int> Indices))
        {
            Indices = new List<int>();
            MissileClassIndices[MissileClass] = Indices;
        } 
        else 
        {
            foreach (int i in Indices)
            {
                if (Missiles[i].IsActive == false)
                {
                    return Missiles[i];
                }
            }
        }

        int MissileIndex = Missiles.Count;

        GameObject MissileObject = ModelLoader.Instance.GetGameObjectFromModel(MissileClass.GeometryName.Get(), null);
        MissileObject.transform.SetParent(transform);

        PhxMissile NewMissile = MissileObject.AddComponent<PhxMissile>();
        NewMissile.Setup(MissileClass);

        Missiles.Add(NewMissile);
        Indices.Add(MissileIndex);
        
        return NewMissile;
    }



    void Update()
    {
        foreach (PhxMissile Missile in Missiles)
        {
            if (Missile.IsActive)
            {
                Missile.Tick(Time.deltaTime);
            }
        }
    }



    public void FireMissile(PhxPawnController owner, 
                            Vector3 pos, Quaternion rot, 
                            PhxMissileClass MissileClass, 
                            List<Collider> IgnoredColliders = null)
    {
        PhxMissile Missile = GetMissile(MissileClass);
        Missile.Spawn(owner, pos, rot);

    }
}
*/