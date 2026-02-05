using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScript : MonoBehaviour
{
    public float time;
    public GameObject deathEffect;
    bool calledDeathScript = false;
    public bool destroyWhenEIsPressed = false;
    public bool ifChildIsDestroyed = false;
    public int amountOfChild = 0;
    // Start is called before the first frame update
    void Start()
    {
        amountOfChild = transform.childCount;
    }

    private void Update()
    {
        amountOfChild = transform.childCount;
        if (ifChildIsDestroyed == true && amountOfChild <= 0)
        {
            Destroy(gameObject);
        }
        if (destroyWhenEIsPressed == true && Input.GetKey(KeyCode.E))
        {
            Destroy(gameObject);
        }
        if (calledDeathScript == false)
        {
            calledDeathScript = true;
            StartCoroutine(death());
        }
    }
    // Update is called once per frame
    IEnumerator death()
    {
        yield return new WaitForSeconds(time);
        if (deathEffect != null )
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
