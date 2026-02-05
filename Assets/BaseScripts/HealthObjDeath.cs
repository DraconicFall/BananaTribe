using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthObjDeath : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Vector2 travelDirection;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(travelDirection * speed * Time.deltaTime);
    }
}
