using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaverPitch : MonoBehaviour
{
    public float MinimumPitch;
    public float MaximumPitch;

    private AudioSource audioSrc;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
       audioSrc.pitch = Mathf.Lerp(MinimumPitch, MaximumPitch, Mathf.PerlinNoise(Time.time/3.0f, 0));
    }
}
