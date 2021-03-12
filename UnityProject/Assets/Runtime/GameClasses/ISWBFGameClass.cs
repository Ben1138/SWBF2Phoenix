using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2;

public abstract class ISWBFGameClass : ISWBFClass
{
    public abstract void SetProperty(string propName, object propValue);
    public abstract void SetClassProperty(string propName, object propValue);
}
