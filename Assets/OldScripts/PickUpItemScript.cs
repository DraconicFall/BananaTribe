using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickUpItemScript : MonoBehaviour
{
    public enum TypeOfItem
    {
        regenOrb,
    }

    GameObject target;

    public TypeOfItem typeOfItem;
    public string targetName = "Player";
    public int amount = 0;

    float speed = 10;
    public float range = 5f;
    float normalRange;
    public AudioClip soundFX;
    public float volume;
    // Start is called before the first frame update
    void Start()
    {
        normalRange = range;
        speed = Random.Range(3f, 6f);
        target = GameObject.Find(targetName);
        if (target == null)
        {
            Destroy(gameObject);
        }
        transform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
        transform.Translate(new Vector2(1, 0) * speed * Time.deltaTime);
        if (speed > 0)
        {
            speed = speed - Time.deltaTime * 10;
        }
        if (speed < 0)
        {
            speed = 0;
        }
        if (Vector2.Distance(transform.position, target.transform.position) < 0.5f)
        {
            if (typeOfItem == TypeOfItem.regenOrb)
            {
                SoundFXManager.instance.PlaySoundFXClip(soundFX, transform, volume, 1, 1, true);
                GameManagerScript.playerMoney = GameManagerScript.playerMoney + amount;
            }
            Destroy(gameObject);
        }
        if (Vector2.Distance(transform.position, target.transform.position) < range)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            Vector2 moveDirection = target.transform.position - transform.position;
            moveDirection.Normalize();
            transform.Translate(moveDirection * 30 * Time.deltaTime);
        }
    }
}
