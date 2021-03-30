using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : InstanceController
{
    public bool CancelPressed { get; private set; }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        State state = ControlState;

        ref Vector2 walk = ref state.WalkDirection;
        walk.x = Input.GetAxis("Horizontal");
        walk.y = Input.GetAxis("Vertical");

        ControlState = state;

        CancelPressed = Input.GetButtonDown("Cancel");
    }
}
