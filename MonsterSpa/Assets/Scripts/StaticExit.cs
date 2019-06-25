using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StaticExit : MonoBehaviour
{
    public static float3 exitPos;
    // Start is called before the first frame update
    
    void Start()
    {
        exitPos = transform.position; //position of the exit object
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
