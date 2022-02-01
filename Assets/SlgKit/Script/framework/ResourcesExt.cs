using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesExt
{

    public static GameObject Load(string path)
    {

        return Resources.Load<GameObject>(path);
    }
}

