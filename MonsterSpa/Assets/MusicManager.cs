using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    public AudioSource[] Songs;

    public int currentSong = -1;
    public int previousSong = -1;
    public float timeToNextSong = 0;

    public float MiniminumSecondsBetweenSongs;
    public float MaximumSecondsBetweenSongs;

    
    
    // Start is called before the first frame update
    void Start()
    {
        //timeToNextSong = Random.Range(MiniminumSecondsBetweenSongs, MaximumSecondsBetweenSongs);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSong == -1)
        {
            timeToNextSong -= Time.deltaTime;

            if (!(timeToNextSong <= 0)) return;
            
            currentSong = Random.Range(0, Songs.Length);
            if (currentSong == previousSong)
                currentSong = (currentSong + 1) % Songs.Length;

            Songs[currentSong].Play();
        }
        else
        {
            if(Songs[currentSong].isPlaying)
                return;

            
            timeToNextSong = Random.Range(MiniminumSecondsBetweenSongs, MaximumSecondsBetweenSongs);
            previousSong = currentSong;
            
            currentSong = -1;

        }

    }
}
