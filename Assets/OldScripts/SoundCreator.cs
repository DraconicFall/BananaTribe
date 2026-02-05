using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCreator : MonoBehaviour
{
    //public float soundDelay = 0;
    public float pitchMin = 1;
    public float pitchMax = 1;
    public float volume = 1;
    public bool affectedByDistance = true;
    public AudioClip soundFX;
    // Start is called before the first frame update
    void Awake()
    {
        playSound();
    }

    void playSound()
    {
        SoundFXManager.instance.PlaySoundFXClip(soundFX, transform, volume, pitchMin, pitchMax, affectedByDistance);
    }
}
