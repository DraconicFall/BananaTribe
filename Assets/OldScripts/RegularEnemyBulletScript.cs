using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class RegularEnemyBulletScript : MonoBehaviour
{
    public enum typeOfAtk
    {
        bullet,
        field,
        chainLightning,
        mortarShell
    }
    public enum typeOfShell //only if is mortarShell duhh...
    {
        indicatorCreatesAttack, //uses atkWayDeathOr
        attackSpawnIndicator
    }
    public enum spawnLocation //only if is mortarShell duhh...
    {
        normal, //comes from source
        aroundTheArena, //from a location from the arena.
        atTheTarget
    }

    [Header("Type of attack")]
    public typeOfAtk typeOfBullet;
    public typeOfShell typeOfMShell;
    public List<GameObject> enemiesHit;
    public int lowerHitDamageAmount = 0;
    [Header("Spawn Type")]
    public spawnLocation spawnType;
    public Vector2 centerOfArena;
    public Vector2 changesOfSpace; //only if spawnLocation is aroundTheArena
    [Header("Movement")]
    public float knockbackStrength = 0;
    public float yDisplacement = 0;
    public float moveAheadSpeed; //how much bullet moves forward when it is created.
    public float moveSpeedMin;
    public float moveSpeedMax;
    float moveSpeed;
    public bool rotateWhenMoving = false;
    public float rotateSpeed = 0;
    int whichSideV = 0; //0 = up, 1 = down
    int whichSideH = 0; //0 = right, 1 = left
    public int amountOfPiercing = 0;
    int amountOfEnemiesHit = 0;
    public bool homing = false;
    public float homingSpeed = 0;
    public string homingTag;
    float distancetoenemy;
    public Transform target;
    public ContactFilter2D contactFilter;

    public int teamNumber = 0;

    public Vector2 moveDirection;

    [Header("Animation")]
    public bool usesAnimation = false;
    public string animationPlayedOnRepeat;
    Animator animator;


    [Header("OnlyHitsOneTarget")]
    public bool OnlyHitsTarget = false;
    private List<EffectCreate> effectsToApply;

    [Header("Other")]
    public bool ignoreDashing = false;
    public bool breakableForEBullets = true;
    public bool alwaysAt0Degrees = false;
    public float damage;
    public float screenShakeStrength;
    public float screenShakeLength;
    public bool hitsWall = true;
    public bool notEnemyBullet = false;
    public float damageMultiplier = 1;

    [Header("Death")]
    public float lengthToWaitBFDestroy = 0;
    public GameObject deathEffect;
    public float maxDistance = 1;
    float distanceTraveled;

    [Header("For Just Fields")]
    public float damageCooldown = 0;
    bool canDoDmg = true;

    [Header("For Just Shells")]
    public Vector2 expanderSize;
    public float expansionTimeMin = 1; //means will take X seconds until the attack is played.
    public float expansionTimeMax = 1;
    public bool destroyAfterExpand = true;
    float expansionTime;
    public Transform expander;
    // Start is called before the first frame update

    private void Awake()
    {
        if (usesAnimation == true)
        {
            animator = GetComponent<Animator>();
        }
        if (alwaysAt0Degrees == true)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        if (typeOfBullet == typeOfAtk.mortarShell && expander == null)
        {
            expander = transform.Find("Expander");
        }
        moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
        expansionTime = Random.Range(expansionTimeMin, expansionTimeMax);
        Collider2D collider = transform.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        if (usesAnimation == true)
        {
            animator.Play(animationPlayedOnRepeat);
        }
    }
    void Start()
    {
        if (alwaysAt0Degrees == true)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        transform.position = new Vector2(transform.position.x, transform.position.y + yDisplacement);
        if (spawnType == spawnLocation.aroundTheArena)
        {
            float randomX = Random.Range(-changesOfSpace.x, changesOfSpace.x);
            float randomY = Random.Range(-changesOfSpace.y, changesOfSpace.y);

            transform.position = new Vector2(centerOfArena.x + randomX, centerOfArena.y + randomY);
        }
        else if (spawnType == spawnLocation.atTheTarget)
        {
            UpdateTarget();
            transform.position = new Vector2(target.position.x, target.position.y);
        }
        if (typeOfBullet == typeOfAtk.mortarShell)
        {
            if (typeOfMShell == typeOfShell.indicatorCreatesAttack)
            {
                StartCoroutine(mortorShellExpand(expansionTime, true));
            }
        }
        if (moveAheadSpeed > 0)
        {
            transform.Translate(new Vector2(1, 0) * moveAheadSpeed * Time.fixedDeltaTime); //new Vector used to be moveDirection
        }
        if (!OnlyHitsTarget && homing == true)
        {
            UpdateTarget();
        }
        foreach (Transform child in transform)
        {
            RegularEnemyBulletScript childEBScript = child.GetComponent<RegularEnemyBulletScript>();
            if (childEBScript != null)
            {
                childEBScript.damageMultiplier = damageMultiplier;
            }
        }

        if (lengthToWaitBFDestroy > 0)
        {
            StartCoroutine(Death());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.isOnTurn) //temporary solution to pals taking damage after fight has ended
        {
            Destroy(gameObject);
        }
        distanceTraveled += moveSpeed * Time.deltaTime;
        if (maxDistance + 0.5f <= distanceTraveled)
        {
            StartCoroutine(Death());
        }
        if (alwaysAt0Degrees == true)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        transform.Translate(new Vector2(1, 0) * moveSpeed * Time.deltaTime);
        if (rotateWhenMoving == true)
        {
            transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
            float radians = transform.rotation.z * (Mathf.PI / 180);
            moveDirection = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
            //print(moveDirection);
        }
        if (homing == true)
        {
            if (!OnlyHitsTarget)
            {
                UpdateTarget();
            }
            if (target != null)
            {
                Vector2 dir = target.position - transform.position;
                moveDirection = dir.normalized;
                float rotateAmount = Vector3.Cross(moveDirection, transform.right).z;
                //Debug.Log(rotateAmount);
                transform.Rotate(new Vector3(0, 0, -rotateAmount * homingSpeed * Time.deltaTime));
            }
        }
        if (OnlyHitsTarget)
        {
            Vector2 dir = target.position - transform.position;
            transform.Translate(dir.normalized * 35 * Time.deltaTime);
            if (target == null)
            {
                StartCoroutine(Death());
            }
            if (target != null && Vector2.Distance(transform.position, target.position) < 1.2f)
            {
                SUnitScript eS = target.GetComponent<SUnitScript>();
                if (eS != null)
                {
                    eS.LoseHealth(Mathf.RoundToInt(damage));
                    StartCoroutine(Death());
                }
            }
        }
        //if (amountOfBounces > 0)
        //{
        //    RaycastHit2D rayUp = Physics2D.Raycast(transform.position, new Vector2(0, 1), colRadius * 2.5f, contactFilter.layerMask);
        //    RaycastHit2D rayRight = Physics2D.Raycast(transform.position, new Vector2(1, 0), colRadius * 2.5f, contactFilter.layerMask);
        //    RaycastHit2D rayLeft = Physics2D.Raycast(transform.position, new Vector2(-1, 0), colRadius * 2.5f, contactFilter.layerMask);
        //    RaycastHit2D rayDown = Physics2D.Raycast(transform.position, new Vector2(0, -1), colRadius * 2.5f, contactFilter.layerMask);
        //    if (rayUp.collider != null) //0 = up, 1 = right, 2 = down, 3= left
        //    {
        //        whichSide = 0;
        //    }
        //    if (rayRight.collider != null)
        //    {
        //        whichSide = 1;

        //    }
        //    if (rayLeft.collider != null)
        //    {
        //        whichSide = 3;
        //    }
        //    if (rayDown.collider != null)
        //    {
        //        whichSide = 2;
        //    }
        //}
        //rb.velocity = moveDirection * moveSpeed
    }

    void UpdateTarget()
    {
        Debug.Log("targetUpdated");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(homingTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                distancetoenemy = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null && shortestDistance <= 999999)
        {
            target = nearestEnemy.transform;

        }
        else
        {
            target = null;
        }
    }

    void UpdateTargetChainLightning()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(homingTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            Collider2D collider = enemy.GetComponent<Collider2D>();
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && collider.enabled == true)
            {
                int count = 0;
                for (int i = 0; i < enemiesHit.Count; i++)
                {
                    if (enemy == enemiesHit[i])
                    {
                        count = count + 1;
                    }
                }
                if (count == 0)
                {
                    shortestDistance = distanceToEnemy;
                    distancetoenemy = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }
        }
        if (nearestEnemy != null && shortestDistance <= 999999)
        {
            //enemiesHit.Add(nearestEnemy);
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    IEnumerator mortorShellExpand(float time, bool alsoDoAttack)
    {
        int repeatAmount = (int)(time * 100);
        float eIncreaseAmntX = expanderSize.x / repeatAmount;
        float eIncreaseAmntY = expanderSize.y / repeatAmount;
        expander.localScale = new Vector3(0, 0, 1);
        for(int i = 0; i < repeatAmount; i++)
        {
            expander.localScale = new Vector3(expander.localScale.x + eIncreaseAmntX, expander.localScale.y + eIncreaseAmntY, 1);
            yield return new WaitForSeconds(0.01f);
        }
        if (destroyAfterExpand == true)
        {
            if (alsoDoAttack == true)
            {
                StartCoroutine(Death());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetMoveDirection(Vector2 dir, float angle, int teamNum, int bulDamage, float range, int piercing) //when add angles do a , float angle or something
    {
        amountOfPiercing = piercing;
        maxDistance = range;
        damage = bulDamage; 
        teamNumber = teamNum;
        //transform.position = new Vector2(transform.position.x, transform.position.y + yDisplacement);
        if (alwaysAt0Degrees != true)
        {
            transform.Rotate(new Vector3(0, 0, angle));
        }
        moveDirection = dir;
        Collider2D collider = transform.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        //moveDirection.Normalize();
    }

    public void SetMoveDirectionAndTarget(Vector2 dir, float angle, int teamNum, int bulDamage, float range, int piercing, Transform target1) //when add angles do a , float angle or something
    {
        target = target1;
        amountOfPiercing = piercing;
        maxDistance = range;
        damage = bulDamage;
        teamNumber = teamNum;
        //transform.position = new Vector2(transform.position.x, transform.position.y + yDisplacement);
        if (alwaysAt0Degrees != true)
        {
            transform.Rotate(new Vector3(0, 0, angle));
        }
        moveDirection = dir;
        Collider2D collider = transform.GetComponent<Collider2D>();
        collider.enabled = false;
        //moveDirection.Normalize();
    }

    public void activateCollider()
    {
        Collider2D collider = transform.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (typeOfBullet == typeOfAtk.bullet)
        {
            SUnitScript healthObj = collision.gameObject.GetComponent<SUnitScript>();
            if (collision.gameObject.tag == "Wall" && hitsWall == true)
            {
                StartCoroutine(Death());
            }
            if (collision.gameObject.tag == "HealthObj" && teamNumber != healthObj.teamNumber)
            {
                //Debug.Log("hit");
                GameObject enemy = collision.gameObject;
                SUnitScript eS = enemy.GetComponent<SUnitScript>();

                if (screenShakeStrength > 0)
                {
                    CameraPosScript mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraPosScript>();
                    StartCoroutine(mainCam.shakeTheScreen(screenShakeStrength, screenShakeLength));
                }
                if (eS != null)
                {
                    if (lowerHitDamageAmount > 0 && eS.damage > 0)
                    {
                        eS.damage -= lowerHitDamageAmount;
                        if (eS.damage < 0)
                        {
                            eS.damage = 0;
                        }
                    }
                    eS.LoseHealth(Mathf.RoundToInt(damage));
                    //eS.knockback(gameObject, knockbackStrength, 0.25f);
                }
                if (amountOfPiercing <= 0)
                {
                    gameObject.GetComponent<Collider2D>().enabled = false;
                    StartCoroutine(Death());
                }
                amountOfPiercing = amountOfPiercing - 1;
            }
        }
        else if (typeOfBullet == typeOfAtk.chainLightning)
        {
            if (collision.gameObject.tag == "Enemies")
            {
                //Debug.Log("hit");
                GameObject enemy = collision.gameObject;
                //EnemyScript eS = enemy.GetComponent<EnemyScript>();

                for (int i = 0; i < effectsToApply.Count; i++)
                {
                    //eS.startEffect(effectsToApply[i]);
                }
                if (deathEffect != null)
                {
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                }

                enemiesHit.Add(enemy);
                //eS.LoseHealth(damage * damageMultiplier);

                UpdateTargetChainLightning();
                if (target == null)
                {
                    StartCoroutine(Death());
                }
                else
                {
                    Vector2 dir = target.position - transform.position;
                    moveDirection = dir.normalized;
                    float radians = Mathf.Atan2(moveDirection.y, moveDirection.x);
                    float angle = (radians * (180 / Mathf.PI));

                    transform.rotation = Quaternion.Euler(0, 0, angle);

                    if (amountOfPiercing <= 0)
                    {
                        StartCoroutine(Death());
                    }
                    damage = damage * 0.65f;
                    amountOfPiercing = amountOfPiercing - 1;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (typeOfBullet == typeOfAtk.field)
        {
            //if (notEnemyBullet == false)
            //{
            //    //if (collision.gameObject.tag == "Player")
            //    //{
            //    //    if (deathEffect != null)
            //    //    {
            //    //        Instantiate(deathEffect, transform.position, Quaternion.identity);
            //    //    }
            //    //    if (collision.GetComponent<PlayerScript>() != null && GameManagerScript.playerDashing == false && GameManagerScript.playerTookDmg == false && canDoDmg == true /*&& GameManagerScript.playerHasStartedWave == true*/)
            //    //    {
            //    //        Debug.Log("hit");
            //    //        GameManagerScript.playerHealth = GameManagerScript.playerHealth - damage;
            //    //        GameManagerScript.playerTookDmg = true;
            //    //        StartCoroutine(Cooldown(damageCooldown));
            //    //        canDoDmg = false;

            //    //    }
            //    //    else if (collision.GetComponent<EnemyScript>() != null)
            //    //    {
            //    //        if (canDoDmg == true)
            //    //        {
            //    //            EnemyScript eS = collision.GetComponent<EnemyScript>();
            //    //            eS.LoseHealth(damage * damageMultiplier);
            //    //            StartCoroutine(Cooldown(damageCooldown));
            //    //            canDoDmg = false;
            //    //        }
            //    //    }
            //    //}
            //    //else if (collision.gameObject.tag == "NonAttractingPlayerAllies")
            //    //{
            //    //    if (collision.GetComponent<EnemyScript>() != null)
            //    //    {
            //    //        if (canDoDmg == true)
            //    //        {
            //    //            EnemyScript eS = collision.GetComponent<EnemyScript>();
            //    //            eS.LoseHealth(damage * damageMultiplier);
            //    //            StartCoroutine(Cooldown(damageCooldown));
            //    //            canDoDmg = false;
            //    //        }
            //    //    }
            //    //}
            //}
            //else if (notEnemyBullet == true)
            //{
            //    //if (collision.gameObject.tag == "Enemies")
            //    //{
            //    //    //Debug.Log("hit");
            //    //    GameObject enemy = collision.gameObject;
            //    //    //EnemyScript eS = enemy.GetComponent<EnemyScript>();

            //    //    if (canDoDmg == true)
            //    //    {
            //    //        for (int i = 0; i < effectsToApply.Count; i++)
            //    //        {
            //    //            //eS.startEffect(effectsToApply[i]);
            //    //        }
            //    //        if (screenShakeStrength > 0)
            //    //        {
            //    //            CameraPosScript mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraPosScript>();
            //    //            StartCoroutine(mainCam.shakeTheScreen(screenShakeStrength, screenShakeLength));
            //    //        }
            //    //        //eS.LoseHealth(damage * damageMultiplier);
            //    //        StartCoroutine(Cooldown(damageCooldown));
            //    //        canDoDmg = false;
            //    //    }
            //    //}
            //}
        }
    }

    IEnumerator Cooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        canDoDmg = true;
    }

    IEnumerator Death()
    {
        moveSpeed = 0;    
        if (lengthToWaitBFDestroy > 0)
        {
            yield return new WaitForSeconds(0.1f);
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        if (deathEffect != null)
        {
            GameObject deathEf = Instantiate(deathEffect, transform.position, Quaternion.identity);
            RegularEnemyBulletScript deathEBScript = deathEf.GetComponent<RegularEnemyBulletScript>();
            if (deathEBScript != null)
            {
                deathEBScript.damageMultiplier = damageMultiplier;
            }
        }
        yield return new WaitForSeconds(lengthToWaitBFDestroy);
        Destroy(gameObject);
    }
}
