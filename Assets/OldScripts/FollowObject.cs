using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public string followedName;
    public bool isInstant = true;
    public float followSpeed = 0;
    public float minFollowRange = 0;
    public bool fasterTravelWF = false;
    GameObject target;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public bool isPal = false;
    GameObject player;
    Vector2 velocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find(followedName);
        if (isPal == true)
        {
            player = GameObject.Find("Player");
        }
    }

    void facePlayer()
    {
        Vector3 dir = target.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0)
        {
            angle = 360 + angle;
        }
        if (angle > 90 && angle <= 270)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (isInstant == true)
            {
                transform.position = target.transform.position;
            }
            else
            {
                facePlayer();
                if (Vector2.Distance(transform.position, target.transform.position) > minFollowRange)
                {
                    if (anim != null && isPal == false)
                    {
                        anim.Play("PlayerWalk");
                    }
                    if (fasterTravelWF == false)
                    {

                    }
                    else
                    {
                        float distanceModifier = Vector2.Distance(transform.position, target.transform.position) / 1.5f;
                        transform.position = Vector2.Lerp(transform.position, target.transform.position, followSpeed /** Time.deltaTime*/);
                        //transform.position = Vector.SmoothDamp(transform.position, target.transform.position, ref velocity, followSpeed);
                    }
                    
                }
                else
                {
                    if (anim != null && isPal == false)
                    {
                        anim.Play("PlayerIdle");
                    }
                }
            }
        }
    }
}
