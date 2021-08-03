using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherit from this class, implement the methods below as you like, and put it into your scene.
/// There must only exist ONE instance of this in the entire scene!
/// </summary>
public abstract class PhxUnityScript : MonoBehaviour
{
    public abstract void ScriptInit();
    public abstract void ScriptPostLoad();
}
