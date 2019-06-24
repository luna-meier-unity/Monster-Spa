using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public static Constants g = null;

    public float HungerMin;
    public float HungerMax;
    public float HungerLossPerSecond;

    public float TemperatureMin;
    public float TemperatureMax;

    /// <summary>
    /// Rate at which the temperature tends towards the default.
    /// </summary>
    public float TemperatureNormalizeRate;
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("Multiple constant gameobject components in scene, please only have one constant gameobject in the scene.");

        g = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
