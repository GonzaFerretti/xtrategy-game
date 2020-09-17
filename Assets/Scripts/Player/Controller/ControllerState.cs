using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerState : ScriptableObject
{
    BaseController controller;

    public virtual void OnUpdate()
    {

    }

    public void Init(BaseController controller)
    {
        this.controller = controller;
    }
}
