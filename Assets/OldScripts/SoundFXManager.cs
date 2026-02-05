using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private GameObject soundFXObject;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume, float pitchMin, float pitchMax, bool affectedByDistance) //later add stuff for pitch
    {
        GameObject audioObj = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        AudioSource audioSource = audioObj.GetComponent<AudioSource>();

        audioSource.clip = audioClip;

        GameObject audioListener = GameObject.Find("Player");
        float distanceMultiplier = 1;
        float minDistance = 3;
        float maxDistance = 15;
        float distance = Vector2.Distance(audioSource.transform.position, audioListener.transform.position);

        if (distance < minDistance )
        {
            //donothingwow
        }
        else if (distance > maxDistance)
        {
            distanceMultiplier = 0;
        }
        else
        {
            distanceMultiplier = 1 - ((distance - minDistance) / (maxDistance - minDistance));
        }

        if (affectedByDistance == false)
        {
            distanceMultiplier = 1;
        }

        audioSource.volume = volume * distanceMultiplier;
        float random = Random.Range(pitchMin, pitchMax); //base value is 1 (atleast according to internet)
        audioSource.pitch = random;

        audioSource.Play();
        
        float clipLength = audioSource.clip.length;

        Destroy(audioObj, clipLength);
    }
}
