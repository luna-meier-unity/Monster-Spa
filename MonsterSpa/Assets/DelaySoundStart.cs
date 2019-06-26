using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySoundStart : MonoBehaviour
{
    private AudioSource audioSrc;
    public float delay;

    private void OnEnable()
    {
        if(audioSrc == null)
            audioSrc = GetComponent<AudioSource>();
        
        audioSrc.PlayDelayed(delay);
    }
}
