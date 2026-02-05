using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    public GameObject spawnedEffect;
    float screenShakeStrength = 0;
    float screenShakeLength = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnedEffect != null)
        {
            Instantiate(spawnedEffect, transform.position, Quaternion.identity);
        }
        if (screenShakeStrength > 0)
        {
            CameraPosScript mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraPosScript>();
            StartCoroutine(mainCam.shakeTheScreen(screenShakeStrength, screenShakeLength));
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
