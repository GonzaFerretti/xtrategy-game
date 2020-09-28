using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncAIActionResult
{
    public bool endedSuccesfully;
    public AIController controller;
    public int id;
    public AsyncAIActionResult(int id, AIController controller)
    {
        endedSuccesfully = false;
        this.controller = controller;
        this.id = id;
    }

    public void EndQuery()
    {
        controller.EndActionResult(this);
    }
}

