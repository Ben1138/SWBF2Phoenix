using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISWBFAnimated
{
    public Action OnAnimEnd { get; set; }
    public void PlayAnimation(string animName, bool bLoop);
}
