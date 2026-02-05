using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audio1; //combat
    public AudioSource audio2; //shop

    public float baseVolume = 1.0f;
    // Update is called once per frame
    void Start()
    {
        //audio1.Play();
        //audio2.Play();
        //audio2.volume = 0;
        //audio1.volume = 0;
        StartCoroutine(FadeIn(audio1, .35f));
    }
    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {

        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
        audioSource.volume = 0.01f;
        audioSource.Stop();
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        if (audioSource.isPlaying == false)
        {
            audioSource.Play();
        }
        float startVolume = 0.01f;

        while (audioSource.volume < baseVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = baseVolume;
    }
}
