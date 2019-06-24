using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public class GameMgr : MonoBehaviour
{
    public static GameMgr g;
    public NativeArray<Entity> rooms;
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("Multiple GameMgr gameobject components in scene, please only have one constant gameobject in the scene.");

        g = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
