using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCameraBattler : MonoBehaviour
{
    Vector3 turnPosition;
    public Vector3 fightPosition;
    public float cameraSpeed = 10;
    // Start is called before the first frame update
    void Awake()
    {
        turnPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.isOnTurn)
        {
            if (transform.position != new Vector3(turnPosition.x, turnPosition.y, -10))
            {
                transform.position = Vector3.Lerp(transform.position, turnPosition, cameraSpeed * Time.deltaTime);
            }
        }
        else if (!GameManagerScript.isOnTurn)
        {
            if (transform.position != new Vector3(fightPosition.x, fightPosition.y, -10))
            {
                transform.position = Vector3.Lerp(transform.position, fightPosition, cameraSpeed * Time.deltaTime);
            }
        }
    }
}
