using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SoundEffectType
{
    Bell,
    Buluboop,
    Chirp,
    Cockneye,
    LowVoice,
    Success,
    Tea,
    WaterBloop,
    Wind
}

public enum AmbientNoiseType
{
    Chirping,
    LowSimmer,
    Rain
}

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager g;
    
    public SoundEffect[] Effects;
    public AudioSource[] AmbientNoise;
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("Multiple Sound Effect Managers in scene, only one allowed.");
        g = this;
    }

    public void PlaySoundEffect(SoundEffectType effect)
    {
        Effects[(int)effect].Play();
    }

    public void StartAmbientNoise(AmbientNoiseType ambient)
    {
        if(!AmbientNoise[(int)ambient].isPlaying)
            AmbientNoise[(int)ambient].Play();
    }

    public void StopAmbientNoise(AmbientNoiseType ambient)
    {
        if(AmbientNoise[(int)ambient].isPlaying)
            AmbientNoise[(int)ambient].Stop();
    }
}
