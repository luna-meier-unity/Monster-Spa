using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public float pitchMin;
    public float pitchMax;


    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
            Play();
    }

    public void Play()
    {
        audioSrc.pitch = Random.Range(pitchMin, pitchMax);
        audioSrc.Play();
    }
}
