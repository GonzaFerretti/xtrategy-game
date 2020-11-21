using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMine : MonoBehaviour
{
    BaseController owner;

    public void SetOwner(BaseController controller)
    {
        owner = controller;
    }
}
