using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static EAttack;
//using static UnityEngine.GraphicsBuffer;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class SUnitScript : MonoBehaviour
{
    public enum attackType
    {
        Aimed,
        Constant
    }
    [Header("Base Stats")]
    public bool breakStatLimiter = false;
    public bool isSummon = false;
    public string palName;
    public int health = 1; //max should be 100
    public int firerate = 5; //firerate of 1 should be 1 attack in 3.2 seconds, and of 50 should be 1 attack in 0.2 seconds.
    public int speed = 1; //speed of 1 is base stat, max speed is 30
    public int damage = 1; //max should be 100
    public int range = 1;
    public int shield = 0;
    public int piercing = 0;
    public int xp = 0;
    public int level = 1; //3 is max the level
    public LayerMask summonLayerMask;
    public PalAttributes attribute;
    [Header("Stats That Show Up")]
    public TextMeshProUGUI levelText;
    public Slider xpBar;

    public TextMeshProUGUI healthNum;
    public TextMeshProUGUI firerateNum;
    public TextMeshProUGUI speedNum;
    public TextMeshProUGUI damageNum;

    public RectTransform healthRect;
    private Image healthRectImage;
    public RectTransform firerateRect;
    public RectTransform speedRect;
    public RectTransform damageRect;

    public Sprite healthIcon;
    public Sprite shieldIcon;

    [HideInInspector] public bool firstPlaceEffectDone = false;

    [Header("Connections")]
    public List<SUnitScript> connectionList;
    public List<GameObject> thingsToSummonIfConnected;
    int connectionAttackCounter = 0;

    [Header("Attacks")]
    int attackCounter = 0;
    bool canAttack = true;
    public attackType typeOfAttack;
    public Sprite attackSprite;
    Sprite idleSprite;
    Vector3 tPosNoUpdate = Vector3.zero;
    public Transform attackLocation;
    public GameObject bullet;
    public int BulletsAmounts = 1;
    public float startRegularAngle = 0.0f, endRegularAngle = 360.0f;
    public bool randomAngles = false;
    public int RepeatAmount = 1;
    public float RepeatRate = 0.0f;
    public int repeatAmount = 1;
    public float repeatRate = 0;
    public int teamNumber = 0; //0 is red, 1 is blue, 2 is green, 3 is yellow

    [Header("The Tower Specific")]
    public bool isAnTowerEnemy = false;


    [Header("Death")]
    public GameObject deathEffectPlayer;
    public GameObject deathEffectEnemy;
    public bool hasUniqueDeathEffect = false;

    [Header("Others")]
    public Color colorToSetWhenBuying; //used to make it so player can see pals more clearly.
    public LayerMask basicPathfindingContacts;
    private RaycastHit2D[] _obstacleCollisions;
    public float currentRotation = 0;
    private Vector2 _targetDirection = Vector2.zero;
    private float distanceOfCircleCast = 0.35f;
    private float _obstacleAvoidanceCooldown;
    private Vector2 _obstacleAvoidanceTargetDirection;

    private bool beganMoveSystem; //for moving pals around the field before fight has started
    Vector2 posBeforeMoveBegan = Vector2.zero;
    Camera mainCam;
    Collider2D palCollider;
    WorldInteracterUI wInteractive;

    public int startRow;
    public int startColumn;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    Transform target;
    Transform movementTarget;
    private float distancetoenemy;
    private float actualFirerate;
    private float actualSpeed;
    private float actualRange;
    private float numOfDFlashesActive = 0;
    private float timer;
    private bool startEffectAchieved = false;
    private bool startDirectionSet = false;
    [HideInInspector] public GameManagerScript gameManager; 
    PalPlacementSystem pSystem;

    private bool makeSureStatsCanBeSeen = false;
    private bool statIconsArePopping = false;
    public Canvas palCanvas;
    Canvas palXPBarCanvas;

    int timesFaintAttributeActivated = 0;

    private void Awake()
    {
        healthRectImage = healthRect.GetComponent<Image>();
        pSystem = GameObject.Find("PlacementSystem").GetComponent<PalPlacementSystem>();
        palCollider = gameObject.GetComponent<Collider2D>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _obstacleCollisions = new RaycastHit2D[10];
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        startEffectAchieved = false;
        actualFirerate = 3.2f - 0.06f * firerate;
        actualSpeed = 1 + (0.15f * speed);
        actualRange = range * 1.5f;
        idleSprite = spriteRenderer.sprite;
        if (attackLocation == null)
        {
            attackLocation = gameObject.transform;
        }

        firerateNum.text = "" + firerate;
        speedNum.text = "" + speed;
        damageNum.text = "" + damage;
        if (shield > 0)
        {
            healthRectImage.sprite = shieldIcon;
            healthNum.text = "" + shield;
        }
        else if (shield <= 0)
        {
            healthRectImage.sprite = healthIcon;
            healthNum.text = "" + health;
        }
    }

    private void Start()
    {
        if (xpBar != null)
        {
            palXPBarCanvas = xpBar.GetComponentInParent<Canvas>();
        }
        wInteractive = PalPlacementSystem.sellBox.GetComponent<WorldInteracterUI>();
        if (!firstPlaceEffectDone && attribute != null && GameManagerScript.isOnTurn && attribute.attributeType == PalAttributes.typeOfAttribute.OnFirstPlace)
        {
            if (attribute.attributeEffect == PalAttributes.typeOfEffect.PalsAround)
            {
                List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                for (int i = 0; i < palList.Count; i++)
                {
                    SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                    Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                    activateBaseAttribute(unitScript);
                }
            }
            firstPlaceEffectDone = true;
        }
        if (isAnTowerEnemy)
        {
            health += Mathf.RoundToInt(health * GameManagerScript.winAmount * 0.1f);
            damage += Mathf.RoundToInt(damage * GameManagerScript.winAmount * 0.1f);
        }

        if (isSummon)
        {
            timer = ((actualFirerate - 0.15f) / 2f);
        }

        anim.Play("PalSpawn");
    }

    public void displayStatUpEffect()
    {
        Instantiate(gameManager.sUnitXPUp, attackLocation.position, Quaternion.identity);
    }

    public void updateActualStats()
    {
        if (speed > 30)
        {
            speed = 30;
        }
        if (firerate > 50)
        {
            firerate = 50;
        }
        if (!breakStatLimiter || !isAnTowerEnemy)
        {
            if (health > 100)
            {
                health = 100;
            }
            if (damage > 100)
            {
                damage = 100;
            }
        }
        actualFirerate = 3.2f - 0.06f * firerate;
        actualSpeed = 1 + (0.25f * speed);
        actualRange = range * 1.5f;
    }
    void faceTarget()
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

    public bool DraggedXPUp()
    {
        if (level < 3)
        {
            anim.Play("PalXPUp");
            xp += 1;
            health += 1;
            damage += 1;
            firerate += 1;
            speed += 1;
            StartCoroutine(showStatUp(healthRect));
            StartCoroutine(showStatUp(firerateRect));
            StartCoroutine(showStatUp(speedRect));
            StartCoroutine(showStatUp(damageRect));
            updateActualStats();
            if (level == 1 && xp >= 2)
            {
                xp = 0;
                level++;

                pSystem.onlySetAttributeText(createAttributeDescription());

            }
            else if (level == 2 && xp >= 3)
            {
                xp = 3;
                level++;

                pSystem.onlySetAttributeText(createAttributeDescription());
            }

            levelText.text = "Lv" + level;
            if (level == 1)
            {
                xpBar.maxValue = 2;
                xpBar.value = xp;
            }
            else if (level == 2)
            {
                xpBar.maxValue = 3;
                xpBar.value = xp;
            }
            else if (level == 3)
            {
                xpBar.value = 3;
            }

            return true;
        }
        return false;
    }

    public bool XPUp()
    {
        if (level < 3)
        {
            anim.Play("PalXPUp");
            if (attribute != null && GameManagerScript.isOnTurn && attribute.attributeType == PalAttributes.typeOfAttribute.OnFirstPlace)
            {
                if (attribute.attributeEffect == PalAttributes.typeOfEffect.PalsAround)
                {
                    List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                    for (int i = 0; i < palList.Count; i++)
                    {
                        SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                        Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                        activateBaseAttribute(unitScript);
                    }
                }
            }
            Instantiate(gameManager.sUnitXPUp, attackLocation.position, Quaternion.identity);
            xp += 1;
            health += 1;
            damage += 1;
            firerate += 1;
            speed += 1;
            StartCoroutine(showStatUp(healthRect));
            StartCoroutine(showStatUp(firerateRect));
            StartCoroutine(showStatUp(speedRect));
            StartCoroutine(showStatUp(damageRect));

            updateActualStats();
            if (level == 1 && xp >= 2)
            {
                xp = 0;
                level++;

                pSystem.onlySetAttributeText(createAttributeDescription());

            }
            else if (level == 2 && xp >= 3)
            {
                xp = 3;
                level++;

                pSystem.onlySetAttributeText(createAttributeDescription());
            }

            levelText.text = "Lv" + level;
            if (level == 1)
            {
                xpBar.maxValue = 2;
                xpBar.value = xp;
            }
            else if (level == 2)
            {
                xpBar.maxValue = 3;
                xpBar.value = xp;
            }
            else if (level == 3)
            {
                xpBar.value = 3;
            }

            return true;
        }
        return false;
    }

    List<Transform> findFurthestEnemy(int amountOfEnemiesToFind)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("HealthObj");
        List<Transform> enemyList = new List<Transform>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].GetComponent<SUnitScript>().teamNumber != teamNumber)
            {
                enemyList.Add(enemies[i].transform);
            }
        }
        enemyList = enemyList.OrderByDescending(t => Vector2.Distance(t.position, transform.position)).ToList();
        if (enemyList.Count < amountOfEnemiesToFind)
        {
            //Debug.Log(enemyList[0]);
            return enemyList.Take(enemyList.Count).ToList();
        }
        //Debug.Log(enemyList[0]);
        return enemyList.Take(amountOfEnemiesToFind).ToList();
    }

    List<Transform> findAllAlivePals()
    {
        GameObject[] pals = GameObject.FindGameObjectsWithTag("HealthObj");
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < pals.Length; i++)
        {
            if (pals[i] != null && pals[i].GetComponent<SUnitScript>().teamNumber == teamNumber && pals[i].activeSelf)
            {
                list.Add(pals[i].transform);
            }
        }
        return list;
    }

    List<Transform> findAllAliveEnemies()
    {
        GameObject[] pals = GameObject.FindGameObjectsWithTag("HealthObj");
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < pals.Length; i++)
        {
            if (pals[i] != null && pals[i].GetComponent<SUnitScript>().teamNumber != teamNumber && pals[i].activeSelf)
            {
                list.Add(pals[i].transform);
            }
        }
        return list;
    }

    Transform findNearestPal()
    {
        //Debug.Log("targetUpdated");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("HealthObj");
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        Transform nearestMEnemy = null;
        //List<Transform> listToReturn = new List<Transform>();
        foreach (GameObject enemy in enemies)
        {
            Collider2D collider = enemy.GetComponent<Collider2D>();
            SUnitScript sUnitScript = enemy.GetComponent<SUnitScript>();
            if (enemy != gameObject && collider.isActiveAndEnabled && sUnitScript.teamNumber == teamNumber)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, sUnitScript.attackLocation.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    distancetoenemy = distanceToEnemy;
                    nearestEnemy = sUnitScript.attackLocation;
                    nearestMEnemy = enemy.transform;
                }
            }
        }
        if (nearestEnemy != null && shortestDistance <= 100000)
        {
            return nearestMEnemy;

        }
        else
        {
            return null;
        }
    }

    void UpdateTarget()
    {
        //Debug.Log("targetUpdated");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("HealthObj");
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        Transform nearestMEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            Collider2D collider = enemy.GetComponent<Collider2D>();
            SUnitScript sUnitScript = enemy.GetComponent<SUnitScript>();
            if (collider.isActiveAndEnabled && sUnitScript.teamNumber != teamNumber)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, sUnitScript.attackLocation.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    distancetoenemy = distanceToEnemy;
                    nearestEnemy = sUnitScript.attackLocation;
                    nearestMEnemy = enemy.transform;
                }
            }
        }
        if (nearestEnemy != null && shortestDistance <= 100000)
        {
            movementTarget = nearestMEnemy;
            target = nearestEnemy;

        }
        else
        {
            target = null;
        }
    }

    void updateTargeTotLowestHealth()
    {
        //Debug.Log("targetUpdated");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("HealthObj");
        float lowestHealth = Mathf.Infinity;
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        Transform nearestMEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            Collider2D collider = enemy.GetComponent<Collider2D>();
            SUnitScript sUnitScript = enemy.GetComponent<SUnitScript>();
            if (collider.isActiveAndEnabled && sUnitScript.teamNumber != teamNumber)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, sUnitScript.attackLocation.position);
                if (distanceToEnemy < shortestDistance && sUnitScript.health == lowestHealth)
                {
                    shortestDistance = distanceToEnemy;
                    //distancetoenemy = distanceToEnemy;
                    nearestEnemy = sUnitScript.attackLocation;
                    nearestMEnemy = enemy.transform;
                }
                if (sUnitScript.health < lowestHealth)
                {
                    lowestHealth = sUnitScript.health;
                    nearestEnemy = sUnitScript.attackLocation;
                    nearestMEnemy = enemy.transform;
                }
            }
        }
        if (nearestEnemy != null)
        {
            movementTarget = nearestMEnemy;
            target = nearestEnemy;

        }
        else
        {
            target = null;
        }
    }

    IEnumerator startAttack()
    {
        canAttack = false;
        spriteRenderer.sprite = attackSprite;
        for (int i = 0; i <= repeatAmount - 1; i++)
        {
            //if (eAttack[attackNum].playAnimationBeforeWait == false)
            //{
            //    StartCoroutine(animationProcessor(anim, transform, eAttack[attackNum]));
            //}

            if (attribute != null && attribute.targetingType == PalAttributes.customTargeting.LowestHealth)
            {
                updateTargeTotLowestHealth();
            }
            else
            {
                UpdateTarget();
            }
            if (target != null)
            {
                if (attribute != null && attribute.targetingType == PalAttributes.customTargeting.LowestHealth)
                {
                    updateTargeTotLowestHealth();
                }
                else
                {
                    UpdateTarget();
                }
                tPosNoUpdate = target.position;
            }

            //if (eAttack[attackNum].attackFX != null)
            //{
            //    SoundFXManager.instance.PlaySoundFXClip(eAttack[attackNum].attackFX, transform, eAttack[attackNum].volumeFX, eAttack[attackNum].pitchMinFX, eAttack[attackNum].pitchMaxFX, true);
            //}
            StartCoroutine(attack(gameObject.transform, attackLocation.position, tPosNoUpdate));
            yield return new WaitForSeconds(repeatRate);
        }
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = idleSprite;
        canAttack = true;
    }

    public IEnumerator attack(Transform transform, Vector3 attackLocations, Vector3 target)
    {
        if (typeOfAttack == attackType.Aimed)
        {
            Vector3 dir = target - attackLocations;
            float startposition = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float originalStartPos = 0;
            float angleStep = (endRegularAngle - startRegularAngle) / BulletsAmounts;
            if (BulletsAmounts > 1)
            {
                if (BulletsAmounts % 2 == 0)
                {
                    startposition = startposition - (BulletsAmounts / 2 * angleStep) + (angleStep / 2);
                }
                else
                {
                    startposition = startposition - (BulletsAmounts / 2 * angleStep);
                }
            }
            else if (randomAngles == true)
            {
                startposition = startposition - angleStep / 2;
            }
            originalStartPos = startposition;
            for (int i = 0; i < BulletsAmounts; i++)
            {
                if (randomAngles == true)
                {
                    startposition = originalStartPos + Random.Range(startRegularAngle, endRegularAngle);
                }
                float radians = startposition * (Mathf.PI / 180);
                Vector3 bulMoveVector = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);

                GameObject bul = Instantiate(bullet);
                bul.transform.position = attackLocations;
                if (bul.GetComponent<RegularEnemyBulletScript>().alwaysAt0Degrees == false)
                {
                    bul.transform.rotation = transform.rotation;
                }
                bul.SetActive(true);
                if (transform.gameObject.GetComponent<RegularEnemyBulletScript>() != null)
                {
                    bul.GetComponent<RegularEnemyBulletScript>().damageMultiplier = transform.gameObject.GetComponent<RegularEnemyBulletScript>().damageMultiplier;
                }
                if (bul.GetComponent<RegularEnemyBulletScript>().alwaysAt0Degrees == false)
                {
                    if (attribute != null && attribute.attributeEffect == PalAttributes.typeOfEffect.dealPercentMore)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            int newDamage = Mathf.RoundToInt(damage * (1 + attribute.damagePercentIncrease * 0.01f));
                            bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulMoveVector, startposition, teamNumber, newDamage, actualRange, piercing);
                        }
                        else
                        {
                            bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulMoveVector, startposition, teamNumber, damage, actualRange, piercing);
                        }
                    }
                    else
                    {
                        bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulMoveVector, startposition, teamNumber, damage, actualRange, piercing);
                    }
                }

                if (randomAngles == false)
                {
                    startposition += angleStep;
                }
            }
            yield return new WaitForSeconds(RepeatRate);
        }
        else if (typeOfAttack == attackType.Constant)
        {
            Vector3 dir = target - attackLocations;
            float angleOther = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float angleStep = (endRegularAngle - startRegularAngle) / BulletsAmounts;
            float angle = startRegularAngle;
            for (int i = 0; i < BulletsAmounts; i++)
            {
                if (randomAngles == true)
                {
                    angleStep = 0;
                    angle = Random.Range(startRegularAngle, endRegularAngle + 1);
                }
                float bulDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
                float bulDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

                Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
                Vector2 bulDir = (bulMoveVector - transform.position).normalized;

                GameObject bul = Instantiate(bullet);
                bul.transform.position = attackLocations;
                bul.transform.rotation = transform.rotation;
                bul.SetActive(true);
                if (attribute != null && attribute.attributeEffect == PalAttributes.typeOfEffect.dealPercentMore)
                {
                    if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                    {

                        int newDamage = Mathf.RoundToInt(damage * (1 + attribute.damagePercentIncrease * 0.01f));
                        bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulDir, angle, teamNumber, newDamage, actualRange, piercing);
                    }
                    else
                    {
                        bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulDir, angle, teamNumber, damage, actualRange, piercing);
                    }
                }
                else
                {
                    bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulDir, angle, teamNumber, damage, actualRange, piercing);
                }
                angle += angleStep;
            }
            yield return new WaitForSeconds(RepeatRate);
        }
        if (connectionAttackCounter > 0 && thingsToSummonIfConnected.Count > 0)
        {
            connectionAttackCounter = 0;
            for (int index = 0; index < thingsToSummonIfConnected.Count; index++)
            {
                GameObject obj = Instantiate(thingsToSummonIfConnected[index], attackLocation.position, Quaternion.identity);
                obj.GetComponent<RegularEnemyBulletScript>().teamNumber = teamNumber;
                obj.GetComponent<RegularEnemyBulletScript>().activateCollider();
            }
        }
    }

    public void LoseHealth(int amountOfDamage)
    {
        int damageTaken = amountOfDamage;
        if (damageTaken > 0)
        {
            //SoundFXManager.instance.PlaySoundFXClip(damageTakenFX, transform, 0.1f, 0.98f, 1.02f, true);
            if (attribute != null && attribute.attributeType == PalAttributes.typeOfAttribute.OnDamageTaken)
            {
                if (attribute.attributeEffect == PalAttributes.typeOfEffect.baseEffect)
                {
                    Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                    activateBaseAttribute();
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.NearbyPal)
                {
                    Transform target = findNearestPal();
                    if (target != null)
                    {
                        SUnitScript SScript = target.GetComponent<SUnitScript>();
                        Instantiate(attribute.baseParticleEffect, SScript.attackLocation.position, Quaternion.identity);
                        activateBaseAttribute(SScript);
                    }
                }
            }
                if (numOfDFlashesActive < 10 && damageTaken > 0)
            {
                StartCoroutine(damageFlash());
                numOfDFlashesActive++;
            }
            if (shield > 0)
            {
                shield = shield - 1;
            }
            else
            {
                health = health - damageTaken;
            }
        }
    }

    IEnumerator damageFlash()
    {
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;

        //setFlashAmount(0.5f);
        //yield return new WaitForSeconds(0.1f);
        //setFlashAmount(0);
        while (elapsedTime < 0.075)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / 0.075f));
            setFlashAmount(currentFlashAmount * 2);
            yield return null;
        }
        numOfDFlashesActive--;
        setFlashAmount(0);
    }

    private void setFlashAmount(float amount)
    {
        spriteRenderer.material.SetFloat("_FlashAmount", amount);
    }

    public void activateBaseAttribute()
    {
        if (attribute.speedIncrease > 0)
        {
            StartCoroutine(showStatUp(speedRect));
            if (attribute.lvlUpSpeedIncrease)
            {
                speed += attribute.speedIncrease * level;
            }
            else
            {
                speed += attribute.speedIncrease;
            }
        }
        if (attribute.healthIncrease > 0)
        {
            StartCoroutine(showStatUp(healthRect));
            if (attribute.lvlUpHealthIncrease)
            {
                health += attribute.healthIncrease * level;
            }
            else
            {
                health += attribute.healthIncrease;
            }
        }
        if (attribute.damageIncrease > 0)
        {
            StartCoroutine(showStatUp(damageRect));
            if (attribute.lvlUpDamageIncrease)
            {
                damage += attribute.damageIncrease * level;
            }
            else
            {
                damage += attribute.damageIncrease;
            }
        }
        if (attribute.firerateIncrease > 0)
        {
            StartCoroutine(showStatUp(firerateRect));
            if (attribute.lvlUpFirerateIncrease)
            {
                firerate += attribute.firerateIncrease * level;
            }
            else
            {
                firerate += attribute.firerateIncrease;
            }
        }
        if (attribute.lvlUpRangeIncrease)
        {
            range += attribute.rangeIncrease * level;
        }
        else
        {
            range += attribute.rangeIncrease;
        }
        if (attribute.shieldIncrease > 0)
        {
            StartCoroutine(showStatUp(healthRect));
            if (attribute.lvlUpShieldIncrease)
            {
                shield += attribute.shieldIncrease * level;
            }
            else
            {
                shield += attribute.shieldIncrease;
            }
        }
        if (attribute.lvlUpPiercingIncrease)
        {
            piercing += attribute.piercingIncrease * level;
        }
        else
        {
            piercing += attribute.piercingIncrease;
        }
        if (attribute.lvlUpGoldIncrease)
        {
            GameManagerScript.playerMoney += attribute.goldIncrease * level;
        }
        else
        {
            GameManagerScript.playerMoney += attribute.goldIncrease;
        }
        updateActualStats();
    }

    public void activateBaseAttribute(SUnitScript target)
    {
        if (attribute.speedIncrease > 0)
        {
            target.StartCoroutine(target.showStatUp(target.speedRect));
            if (attribute.lvlUpSpeedIncrease)
            {
                target.speed += attribute.speedIncrease * level;
            }
            else
            {
                target.speed += attribute.speedIncrease;
            }
        }
        if (attribute.healthIncrease > 0)
        {
            target.StartCoroutine(target.showStatUp(target.healthRect));
            if (attribute.lvlUpHealthIncrease)
            {
                target.health += attribute.healthIncrease * level;
            }
            else
            {
                target.health += attribute.healthIncrease;
            }
        }
        if (attribute.damageIncrease > 0)
        {
            target.StartCoroutine(target.showStatUp(target.damageRect));
            if (attribute.lvlUpDamageIncrease)
            {
                target.damage += attribute.damageIncrease * level;
            }
            else
            {
                target.damage += attribute.damageIncrease;
            }
        }
        if (attribute.firerateIncrease > 0)
        {
            target.StartCoroutine(target.showStatUp(target.firerateRect));
            if (attribute.lvlUpFirerateIncrease)
            {
                target.firerate += attribute.firerateIncrease * level;
            }
            else
            {
                target.firerate += attribute.firerateIncrease;
            }
        }
        if (attribute.lvlUpRangeIncrease)
        {
            target.range += attribute.rangeIncrease * level;
        }
        else
        {
            target.range += attribute.rangeIncrease;
        }
        if (attribute.shieldIncrease > 0)
        {
            target.StartCoroutine(target.showStatUp(target.healthRect));
            if (attribute.lvlUpShieldIncrease)
            {
                target.shield += attribute.shieldIncrease * level;
            }
            else
            {
                target.shield += attribute.shieldIncrease;
            }
        }
        if (attribute.lvlUpPiercingIncrease)
        {
            target.piercing += attribute.piercingIncrease * level;
        }
        else
        {
            target.piercing += attribute.piercingIncrease;
        }
        if (attribute.lvlUpGoldIncrease)
        {
            GameManagerScript.playerMoney += attribute.goldIncrease * level;
        }
        else
        {
            GameManagerScript.playerMoney += attribute.goldIncrease;
        }
        updateActualStats();
    }

    private float HandleObstacles()
    {
        _obstacleAvoidanceCooldown -= Time.deltaTime;

        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(basicPathfindingContacts);
        Vector2 moveDirection = target.position - attackLocation.position;

        int numberOfCollision = Physics2D.CircleCast(
            attackLocation.position,
            gameObject.GetComponent<CircleCollider2D>().radius * 0.45f,
            moveDirection,
            contactFilter,
            _obstacleCollisions,
            distanceOfCircleCast  
            );
        if (numberOfCollision <= 1)
        {
            _targetDirection = moveDirection;
        }
        for (int index = 0; index < numberOfCollision; index++)
        {
            var obstacleCollision = _obstacleCollisions[index];
            if (obstacleCollision.collider.gameObject == gameObject || obstacleCollision.collider.gameObject == target)
            {
                continue;
            }
            if (_obstacleAvoidanceCooldown <= 0)
            {
                _obstacleAvoidanceCooldown = 0.5f;
                _obstacleAvoidanceTargetDirection = obstacleCollision.normal;
            }
            float targetRad = Mathf.Atan2(_obstacleAvoidanceTargetDirection.y, _obstacleAvoidanceTargetDirection.x);
            float targetRotate = Mathf.Rad2Deg * targetRad;
            return Mathf.LerpAngle(currentRotation, targetRotate, Time.deltaTime * 12f);

            //var targetRotation = Quaternion.FromToRotation(transform.forward, _obstacleAvoidanceTargetDirection);
            //var rotation = Quaternion.RotateTowards(Quaternion.Euler(0, 0, currentRotation), targetRotation, Time.deltaTime * 2);
            //var rotationTarget2 = rotation.eulerAngles;
            //float targetRadian = Mathf.Atan2(rotationTarget2.y, rotationTarget2.x);
            //return Mathf.Rad2Deg * targetRadian;

            //float startRotation = Mathf.Atan2(targetRotation.y, targetRotation.x) * Mathf.Rad2Deg;
            //var startRotation = Quaternion.Euler(moveDirection);
            //var rotation = Quaternion.RotateTowards(startRotation, targetRotation, 30 * Time.deltaTime);
            //var rotationVectorfied = rotation.eulerAngles;
            //_targetDirection = rotationVectorfied;
            //float radians = Quaternion.Angle(startRotation, targetRotation) * (Mathf.PI / 180);
            //_targetDirection = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
            //_targetDirection = targetRotation;
            //break;
        }
        return -999;
    }

    private bool CheckIfThereIsPath()
    {
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(basicPathfindingContacts);
        float rotationAmount = 0;
        int directionsHit = 0;
        for (int index = 0; index < 24; index++)
        {
            float radians = Mathf.Deg2Rad * rotationAmount;
            Vector2 moveDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            RaycastHit2D[] results = new RaycastHit2D[10];
            int numberOfCollision = Physics2D.CircleCast(
            target.position,
            0.4f,
            moveDirection,
            contactFilter,
            results,
            1.2f
            );

            int objHits = 0;
            for (int i = 0; i < numberOfCollision; i++)
            {
                var obstacleCollision = results[i];
                if (obstacleCollision.collider.gameObject == gameObject)
                {
                    continue;
                }
                objHits++;
            }
            if (objHits > 1)
            {
                directionsHit++;
            }

            rotationAmount += 15;
        }
        if (directionsHit < 24)
        {
            return true;
        }

        return false;
    }

    private void MovementHandler()
    {
        _targetDirection = movementTarget.position - transform.position;
        float targetRotation = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg;
        float tempObstacleRotation = HandleObstacles();
        if (tempObstacleRotation != -999)
        {
            targetRotation = tempObstacleRotation;
        }
        if (Mathf.Abs(currentRotation - targetRotation) >= 1f)
        {
            currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * 6f);
            //Debug.Log(currentRotation);
        }
        float radians = Mathf.Deg2Rad * currentRotation;
        Vector2 currentDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        if (true) //Vector2.Distance(transform.position, movementTarget.position) <= actualRange * 1.2 || CheckIfThereIsPath()
        {
            anim.Play("PlayerWalk");
            transform.Translate(currentDirection.normalized * actualSpeed * Time.deltaTime);
        }
        else
        {
            anim.Play("PlayerIdle");
        }
    }

    public void destroyThisObj()
    {
        Destroy(gameObject);
    }

    public IEnumerator showStatUp(RectTransform meshToChange) //purely Visual
    {
        meshToChange.localScale = new Vector2(1.75f, 1.75f);
        statIconsArePopping = true;
        makeSureStatsCanBeSeen = true;
        while (meshToChange.localScale.x >= 1.05)
        {
            yield return new WaitForSeconds(0.01f);
            meshToChange.localScale = Vector2.Lerp(meshToChange.localScale, new Vector2(1, 1), 0.5f);
        }
        yield return new WaitForSeconds(0.01f);
        meshToChange.localScale = new Vector2(1f, 1f);
        makeSureStatsCanBeSeen = false;
        statIconsArePopping = false;
    }

    public void callStartFightEffect()
    {
        if (attribute != null && GameManagerScript.prefightStarted && attribute.attributeType == PalAttributes.typeOfAttribute.OnFightStart)
        {
            if (GameManagerScript.currentPreFightNum == attribute.orderInFightStart)
            {
                startEffectAchieved = true;
                if (attribute.attributeEffect == PalAttributes.typeOfEffect.PalsAround)
                {
                    List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                    for (int i = 0; i < palList.Count; i++)
                    {
                        SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                        Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                        activateBaseAttribute(unitScript);
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.ForEveryPalAround)
                {
                    List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                    if (palList.Count > 0)
                    {
                        Instantiate(attribute.baseParticleEffect, attackLocation.position, Quaternion.identity);
                    }
                    for (int i = 0; i < palList.Count; i++)
                    {
                        activateBaseAttribute();
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.EveryPal)
                {
                    List<GameObject> palList = PalPlacementSystem.getAllPals(startRow, startColumn);
                    for (int i = 0; i < palList.Count; i++)
                    {
                        SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                        Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                        activateBaseAttribute(unitScript);
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAhead)
                {
                    List<GameObject> palList = PalPlacementSystem.getPalInFront(startRow, startColumn);
                    Instantiate(attribute.startConnectionEffect, attackLocation.position, Quaternion.identity);
                    for (int i = 0; i < palList.Count; i++)
                    {
                        SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                        connectionList.Add(unitScript);
                        Instantiate(attribute.connectionEffect, unitScript.attackLocation.position, Quaternion.identity, unitScript.transform);
                        if (attribute.connectionType == PalAttributes.typeOfConnect.SummonAtConnected)
                        {
                            if (level == 1)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv1);
                            }
                            else if (level == 2)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv2);
                            }
                            else if (level == 3)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv3);
                            }
                        }
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAround)
                {
                    List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                    Instantiate(attribute.startConnectionEffect, attackLocation.position, Quaternion.identity);
                    for (int i = 0; i < palList.Count; i++)
                    {
                        SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                        connectionList.Add(unitScript);
                        Instantiate(attribute.connectionEffect, unitScript.attackLocation.position, Quaternion.identity, unitScript.transform);
                        if (attribute.connectionType == PalAttributes.typeOfConnect.SummonAtConnected)
                        {
                            if (level == 1)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv1);
                            }
                            else if (level == 2)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv2);
                            }
                            else if (level == 3)
                            {
                                unitScript.thingsToSummonIfConnected.Add(attribute.objToSummonLv3);
                            }
                        }
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.DealDamage)
                {
                    if (attribute.baseParticleEffect != null)
                    {
                        Instantiate(attribute.baseParticleEffect, attackLocation.position, Quaternion.identity);
                    }
                    if (attribute.dealDamageTargetsFarthest)
                    {
                        List<Transform> targetList = findFurthestEnemy(attribute.rangeOfEffect);
                        //Debug.Log(targetList);
                        for (int i = 0; i < targetList.Count; i++)
                        {
                            Vector2 moveDirection = targetList[i].position - transform.position;
                            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                            GameObject bullet = Instantiate(attribute.objToSummonLv1, attackLocation.position, Quaternion.identity);
                            bullet.GetComponent<RegularEnemyBulletScript>().SetMoveDirectionAndTarget(moveDirection.normalized, angle, teamNumber, attribute.damageDeltFromAttribute * level, 999, 99999, targetList[i]);
                            bullet.SetActive(true);
                        }
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.baseEffect)
                {
                    Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                    activateBaseAttribute();
                }
                //GameManagerScript.preFightObjects.Remove(gameObject.GetComponent<SUnitScript>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.isOnTurn && PalPlacementSystem.isBuying && teamNumber == 0)
        {
            spriteRenderer.color = Vector4.Lerp(spriteRenderer.color, colorToSetWhenBuying, Time.deltaTime * 12);

            healthRect.GetComponent<Image>().color = Vector4.Lerp(healthRect.GetComponent<Image>().color, colorToSetWhenBuying, Time.deltaTime * 12);
            damageRect.GetComponent<Image>().color = Vector4.Lerp(damageRect.GetComponent<Image>().color, colorToSetWhenBuying, Time.deltaTime * 12);
            speedRect.GetComponent<Image>().color = Vector4.Lerp(speedRect.GetComponent<Image>().color, colorToSetWhenBuying, Time.deltaTime * 12);
            firerateRect.GetComponent<Image>().color = Vector4.Lerp(firerateRect.GetComponent<Image>().color, colorToSetWhenBuying, Time.deltaTime * 12);

            xpBar.gameObject.SetActive(false);

            healthNum.color = Vector4.Lerp(healthNum.color, colorToSetWhenBuying, Time.deltaTime * 12);
            damageNum.color = Vector4.Lerp(damageNum.color, colorToSetWhenBuying, Time.deltaTime * 12);
            speedNum.color = Vector4.Lerp(speedNum.color, colorToSetWhenBuying, Time.deltaTime * 12);
            firerateNum.color = Vector4.Lerp(firerateNum.color, colorToSetWhenBuying, Time.deltaTime * 12);

            if ((PalPlacementSystem.findGridCurPosition() != null && PalPlacementSystem.findGridCurPosition() == gameObject) || (PalPlacementSystem.currentBuyingPal != null && (PalPlacementSystem.currentBuyingPal.name + "(Clone)") == gameObject.name && level < 3))
            {
                spriteRenderer.color = Color.white;
                healthRect.GetComponent<Image>().color = Color.white;
                damageRect.GetComponent<Image>().color = Color.white;
                speedRect.GetComponent<Image>().color = Color.white;
                firerateRect.GetComponent<Image>().color = Color.white;

                healthNum.color = Color.white;
                firerateNum.color = Color.white;
                damageNum.color = Color.white;
                speedNum.color = Color.white;

                if (xpBar != null)
                {
                    xpBar.gameObject.SetActive(true);
                }
            }
        }
        else if (teamNumber == 0 && !PalPlacementSystem.isBuying)
        {
            spriteRenderer.color = Color.white;
            healthRect.GetComponent<Image>().color = Color.white;
            damageRect.GetComponent<Image>().color = Color.white;
            speedRect.GetComponent<Image>().color = Color.white;
            firerateRect.GetComponent<Image>().color = Color.white;

            healthNum.color = Color.white;
            firerateNum.color = Color.white;
            damageNum.color = Color.white;
            speedNum.color = Color.white;

            if (xpBar != null)
            {
                xpBar.gameObject.SetActive(true);
            }
        }

        if (GameManagerScript.prefightStarted && attribute != null && attribute.attributeType == PalAttributes.typeOfAttribute.OnFriendFaint && GameManagerScript.playerPalCount + timesFaintAttributeActivated < GameManagerScript.mostAlliedPalCount)
        {
            if (attribute.attributeEffect == PalAttributes.typeOfEffect.baseEffect)
            {
                timesFaintAttributeActivated++;
                Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                activateBaseAttribute();
            }
        }

        firerateNum.text = "" + firerate;
        speedNum.text = "" + speed;
        damageNum.text = "" + damage;
        if (shield > 0)
        {
            healthRectImage.sprite = shieldIcon;
            healthNum.text = "" + shield;
        }
        else if (shield <= 0)
        {
            healthRectImage.sprite = healthIcon;
            healthNum.text = "" + health;
        }
        if (makeSureStatsCanBeSeen)
        {
            if (palCanvas != null)
            {
                //spriteRenderer.sortingOrder = 100;
                palCanvas.sortingLayerName = "Character UI's";
                if (xpBar != null)
                {
                    palXPBarCanvas.sortingLayerName = "Character UI's";
                }
            }
        }
        else if (!makeSureStatsCanBeSeen)
        {
            if (palCanvas != null)
            {
                //spriteRenderer.sortingOrder = 0;
                palCanvas.sortingLayerName = "Characters";
                if (xpBar != null)
                {
                    palXPBarCanvas.sortingLayerName = "Characters";
                }
            }
        }
        if (GameManagerScript.prefightAllHealthObjSummoned && attribute != null && attribute.attributeType == PalAttributes.typeOfAttribute.OnFightStart && GameManagerScript.currentPreFightNum >= attribute.orderInFightStart && !startEffectAchieved)
        {
            GameManagerScript.preFightObjects.Add(gameObject.GetComponent<SUnitScript>());
            startEffectAchieved = true;
        }
        if (GameManagerScript.prefightAllHealthObjSummoned && !startDirectionSet)
        {
            if (attribute != null && attribute.targetingType == PalAttributes.customTargeting.LowestHealth)
            {
                updateTargeTotLowestHealth();
            }
            else
            {
                UpdateTarget();
            }
            _targetDirection = movementTarget.position - transform.position;
            float targetRotation = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg;
            currentRotation = targetRotation;
            if (!isSummon)
            {
                if (attribute != null && (attribute.attributeEffect == PalAttributes.typeOfEffect.Summon || attribute.attributeEffect == PalAttributes.typeOfEffect.SummonAt))
                {
                    if (teamNumber == 0)
                    {
                        GameManagerScript.summonedQue += 1 * attribute.rangeOfEffect;
                    }
                    else if (teamNumber > 0)
                    {
                        GameManagerScript.enemySummonedQue += 1 * attribute.rangeOfEffect;
                    }
                }
                if (teamNumber == 0)
                {
                    GameManagerScript.playerPalCount++;
                }
                else if (teamNumber > 0)
                {
                    GameManagerScript.enemyPalCount++;
                }
            }
            startDirectionSet = true;
        }
        if (GameManagerScript.fightStarted)
        {
            if (attribute != null)
            {
                if (attribute.attributeType == PalAttributes.typeOfAttribute.DuringFight)
                {
                    if (attribute.attributeEffect == PalAttributes.typeOfEffect.baseEffect)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            activateBaseAttribute();
                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            activateBaseAttribute();
                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.dealPercentMore)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            activateBaseAttribute();
                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            activateBaseAttribute();
                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.NearbyPal)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            Transform target = findNearestPal();
                            if (target != null)
                            {
                                SUnitScript SScript = target.GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, SScript.attackLocation.position, Quaternion.identity);
                                activateBaseAttribute(SScript);
                            }
                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            Transform target = findNearestPal();
                            if (target != null)
                            {
                                SUnitScript SScript = target.GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, SScript.attackLocation.position, Quaternion.identity);
                                activateBaseAttribute(SScript);
                            }
                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.Summon)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            summonAttributeObj(attribute.rangeOfEffect);

                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            summonAttributeObj(attribute.rangeOfEffect);

                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.EveryPal)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            List<Transform> palList = findAllAlivePals();
                            for (int i = 0; i < palList.Count; i++)
                            {
                                SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                                activateBaseAttribute(unitScript);
                            }

                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            List<Transform> palList = findAllAlivePals();
                            for (int i = 0; i < palList.Count; i++)
                            {
                                SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                                activateBaseAttribute(unitScript);
                            }
                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.DealDamageToEveryEnemy)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            List<Transform> palList = findAllAliveEnemies();
                            for (int i = 0; i < palList.Count; i++)
                            {
                                SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                                unitScript.LoseHealth(attribute.damageDeltFromAttribute);
                            }

                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            List<Transform> palList = findAllAliveEnemies();
                            for (int i = 0; i < palList.Count; i++)
                            {
                                SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                                Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                                activateBaseAttribute(unitScript);
                            }
                        }
                    }
                    else if (attribute.attributeEffect == PalAttributes.typeOfEffect.EnemyDefeated)
                    {
                        if (teamNumber != 0 && GameManagerScript.playerPalCount + timesFaintAttributeActivated < GameManagerScript.mostAlliedPalCount)
                        {
                            timesFaintAttributeActivated++;
                            Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                            activateBaseAttribute();
                        }
                        //else if (teamNumber == 0 && GameManagerScript.enemyPalCount + timesFaintAttributeActivated)
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAhead)
                {
                    if (attribute.connectionType == PalAttributes.typeOfConnect.BaseConnect)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            for (int i = 0; i < connectionList.Count; i++)
                            {
                                if (connectionList[i] != null)
                                {
                                    Instantiate(attribute.baseParticleEffect, connectionList[i].attackLocation.position, Quaternion.identity);
                                    activateBaseAttribute(connectionList[i]);
                                }
                            }
                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            for (int i = 0; i < connectionList.Count; i++)
                            {
                                if (connectionList[i] != null)
                                {
                                    Instantiate(attribute.baseParticleEffect, connectionList[i].attackLocation.position, Quaternion.identity);
                                    activateBaseAttribute(connectionList[i]);
                                }
                            }
                        }
                    }
                }
                else if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAround)
                {
                    if (attribute.connectionType == PalAttributes.typeOfConnect.BaseConnect)
                    {
                        if (attribute.AttacksTillEffect > 0 && attackCounter >= attribute.AttacksTillEffect)
                        {
                            attackCounter = 0;
                            for (int i = 0; i < connectionList.Count; i++)
                            {
                                if (connectionList[i] != null)
                                {
                                    Instantiate(attribute.baseParticleEffect, connectionList[i].attackLocation.position, Quaternion.identity);
                                    activateBaseAttribute(connectionList[i]);
                                }
                            }
                        }
                        else if (attribute.AttacksTillEffect <= 0)
                        {
                            for (int i = 0; i < connectionList.Count; i++)
                            {
                                if (connectionList[i] != null)
                                {
                                    Instantiate(attribute.baseParticleEffect, connectionList[i].attackLocation.position, Quaternion.identity);
                                    activateBaseAttribute(connectionList[i]);
                                }
                            }
                        }
                    }
                }
            }
            if (attribute != null && attribute.targetingType == PalAttributes.customTargeting.LowestHealth)
            {
                updateTargeTotLowestHealth();
            }
            else
            {
                UpdateTarget();
            }
            timer += Time.deltaTime;
            if (target != null)
            {
                faceTarget();
                if (Vector2.Distance(transform.position, movementTarget.position) > actualRange)
                {
                    if (anim != null)
                    {
                        //HandleObstacles();
                        MovementHandler();
                        //transform.position = Vector2.MoveTowards(transform.position, movementTarget.position, actualSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    if (target != null && timer >= actualFirerate - 0.15f)
                    {
                        timer = 0;
                        attackCounter++;
                        connectionAttackCounter++;
                        StartCoroutine(startAttack());
                    }
                    if (anim != null)
                    {
                        anim.Play("PlayerIdle");
                    }
                }
            }
            else
            {
                anim.Play("PlayerIdle");
            }

            if (health < 1)
            {
                death();
            }
        }
    }

    void death()
    {
        if (attribute != null && attribute.attributeType == PalAttributes.typeOfAttribute.OnFaint)
        {
            if (attribute.attributeEffect == PalAttributes.typeOfEffect.NearbyPal)
            {
                Transform target = findNearestPal();
                if (target != null)
                {
                    SUnitScript SScript = target.GetComponent<SUnitScript>();
                    Instantiate(attribute.baseParticleEffect, SScript.attackLocation.position, Quaternion.identity);
                    activateBaseAttribute(SScript);
                }
            }
            else if (attribute.attributeEffect == PalAttributes.typeOfEffect.DealDamage)
            {
                if (attribute.baseParticleEffect != null)
                {
                    Instantiate(attribute.baseParticleEffect, attackLocation.position, Quaternion.identity);
                }
                GameObject explosion = Instantiate(attribute.objToSummonLv1, attackLocation.position, Quaternion.identity);
                explosion.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(Vector2.zero, 0, teamNumber, attribute.damageDeltFromAttribute * level, 3, 99999);
                float actualRange = attribute.rangeOfEffect * 0.75f;
                explosion.transform.localScale = new Vector3(actualRange, actualRange, actualRange);
            }
            else if (attribute.attributeEffect == PalAttributes.typeOfEffect.Summon)
            {
                summonAttributeObj(attribute.rangeOfEffect);
            }
        }

        if (teamNumber == 0)
        {
            GameManagerScript.playerPalCount--;
            if (!hasUniqueDeathEffect && deathEffectPlayer != null)
            {
                GameObject obj = Instantiate(deathEffectPlayer, transform.position, Quaternion.identity);
                HealthObjDeath objDeath = obj.GetComponent<HealthObjDeath>();
                objDeath.spriteRenderer.sprite = idleSprite;
            }
            else if (deathEffectPlayer != null)
            {
                Instantiate(deathEffectPlayer, transform.position, Quaternion.identity);
            }
        }
        else
        {
            GameManagerScript.enemyPalCount--;
            if (!hasUniqueDeathEffect && deathEffectEnemy != null)
            {
                GameObject obj = Instantiate(deathEffectEnemy, transform.position, Quaternion.identity);
                HealthObjDeath objDeath = obj.GetComponent<HealthObjDeath>();
                objDeath.spriteRenderer.sprite = idleSprite;
            }
            else if (deathEffectEnemy != null)
            {
                Instantiate(deathEffectEnemy, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }

    public string createAttributeDescription()
    {
        string returner = "";
        if (attribute.attributeType == PalAttributes.typeOfAttribute.OnFirstPlace)
        {
            returner += "<b>First Place</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnFightStart)
        {
            returner += "<b>Fight Start</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.DuringFight)
        {
            returner += "<b>During Fight</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnSell)
        {
            returner += "<b>Sell</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnTurnStart)
        {
            returner += "<b>Turn Start</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnFaint)
        {
            returner += "<b>Faint</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnFriendFaint)
        {
            returner += "<b>Friend Faint</b> ";
        }
        else if (attribute.attributeType == PalAttributes.typeOfAttribute.OnDamageTaken)
        {
            returner += "<b>Damage Taken</b> ";
        }

        returner += "<sprite index=0> ";

        if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAround)
        {
            returner += "Connect to pals in a square of " + attribute.rangeOfEffect + " <sprite index=2> range and those pals, ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.ConnectAhead)
        {
            returner += "Connect to the pal ahead and that pal, ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.PalsAround)
        {
            returner += "Pals in a square of " + attribute.rangeOfEffect + " <sprite index=2> range ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.ForEveryPalAround)
        {
            returner += "For each pal in a square of " + attribute.rangeOfEffect + " <sprite index=2> range, this pal ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.NearbyPal)
        {
            returner += "Nearby pal ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.EveryPal)
        {
            returner += "Every pal ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.EnemyDefeated)
        {
            returner += "Enemy faint ";
        }
        if (attribute.attributeEffect == PalAttributes.typeOfEffect.DealDamage)
        {
            if (attribute.dealDamageTargetsFarthest)
            {
                if (attribute.rangeOfEffect == 1)
                {
                    returner += "Deal " + attribute.damageDeltFromAttribute * level + "<sprite index=5> damage to the furthest enemy";
                }
                else
                {
                    returner += "Deal " + attribute.damageDeltFromAttribute * level + "<sprite index=5> damage to the " + attribute.rangeOfEffect + " furthest enemies";
                }
            }
            else
            {
                returner += "Deal " + attribute.damageDeltFromAttribute * level + "<sprite index=5> damage to all enemies in a " + attribute.rangeOfEffect + " <sprite index=2> tile radius";
            }
        }

        if (attribute.AttacksTillEffect > 0)
        {
            if (attribute.AttacksTillEffect == 1)
            {
                returner += "every " + attribute.AttacksTillEffect + " attack taken, ";
            }
            else
            {
                returner += "every " + attribute.AttacksTillEffect + " attacks taken, ";
            }
        }

        if (attribute.attributeEffect == PalAttributes.typeOfEffect.DealDamageToEveryEnemy)
        {
            returner += "Deal " + attribute.damageDeltFromAttribute * level + " <sprite index=5> damage to every enemy";
        }

        if (attribute.connectionType == PalAttributes.typeOfConnect.SummonAtConnected)
        {
            if (level == 1)
            {
                returner += attribute.connectionBasedTextLv1;
            }
            else if (level == 2)
            {
                returner += attribute.connectionBasedTextLv2;
            }
            else if (level == 2)
            {
                returner += attribute.connectionBasedTextLv2;
            }
        }

        if (attribute.attributeEffect == PalAttributes.typeOfEffect.Summon)
        {
            returner += "summon <b>" + attribute.rangeOfEffect + "</b> ";
            if (level == 1)
            {
                SUnitScript unitScript = attribute.objToSummonLv1.GetComponent<SUnitScript>();
                returner += unitScript.health + "/" + unitScript.firerate + "/" + unitScript.speed + "/" + unitScript.damage + " " + unitScript.palName;
            }
            else if (level == 2)
            {
                SUnitScript unitScript = attribute.objToSummonLv2.GetComponent<SUnitScript>();
                returner += unitScript.health + "/" + unitScript.firerate + "/" + unitScript.speed + "/" + unitScript.damage + " " + unitScript.palName;
            }
            else if (level == 3)
            {
                SUnitScript unitScript = attribute.objToSummonLv3.GetComponent<SUnitScript>();
                returner += unitScript.health + "/" + unitScript.firerate + "/" + unitScript.speed + "/" + unitScript.damage + " " + unitScript.palName;
            }
        }
        else if (attribute.attributeEffect == PalAttributes.typeOfEffect.dealPercentMore)
        {
            returner += "deal <b>" + attribute.damagePercentIncrease + "%</b> more damage and ";
        }

        returner += statDescriptionCreator();

        if (attribute.targetingType == PalAttributes.customTargeting.LowestHealth)
        {
            returner += ". Always target the lowest health enemy";
        }

        returner += ".";

        return returner;
    }

    string statDescriptionCreator()
    {
        int statsIncreasedCounter = 0;
        string statsString = "";
        if (attribute.healthIncrease > 0) //make sure to add in sprites once they are in game
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpHealthIncrease)
            {
                increaseAmount = attribute.healthIncrease * level;
            }
            else
            {
                increaseAmount = attribute.healthIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=1> health, ";
        }
        if (attribute.firerateIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpFirerateIncrease)
            {
                increaseAmount = attribute.firerateIncrease * level;
            }
            else
            {
                increaseAmount = attribute.firerateIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=6> firerate, ";
        }
        if (attribute.speedIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpSpeedIncrease)
            {
                increaseAmount = attribute.speedIncrease * level;
            }
            else
            {
                increaseAmount = attribute.speedIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=4> speed, ";
        }
        if (attribute.damageIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpDamageIncrease)
            {
                increaseAmount = attribute.damageIncrease * level;
            }
            else
            {
                increaseAmount = attribute.damageIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=5> damage, ";
        }
        if (attribute.shieldIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpShieldIncrease)
            {
                increaseAmount = attribute.shieldIncrease * level;
            }
            else
            {
                increaseAmount = attribute.shieldIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=3> shield, ";
        }
        if (attribute.rangeIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpRangeIncrease)
            {
                increaseAmount = attribute.rangeIncrease * level;
            }
            else
            {
                increaseAmount = attribute.rangeIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=2> range, ";
        }
        if (attribute.piercingIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpPiercingIncrease)
            {
                increaseAmount = attribute.piercingIncrease * level;
            }
            else
            {
                increaseAmount = attribute.piercingIncrease;
            }
            statsString += "+" + increaseAmount + " <sprite index=7> piercing, ";
        }
        if (attribute.goldIncrease > 0)
        {
            statsIncreasedCounter++;
            int increaseAmount = 0;
            if (attribute.lvlUpGoldIncrease)
            {
                increaseAmount = attribute.goldIncrease * level;
            }
            else
            {
                increaseAmount = attribute.goldIncrease;
            }
            statsString += "+" + increaseAmount + " gold, ";
        }
        if (statsIncreasedCounter > 0)
        {
            int index = statsString.LastIndexOf(", ");
            statsString = statsString.Remove(index);
            if (statsIncreasedCounter == 2)
            {
                int inde2 = statsString.LastIndexOf(", ");
                statsString = statsString.Remove(inde2, 2).Insert(inde2, " and ");
            }
            else if (statsIncreasedCounter > 2)
            {
                int inde2 = statsString.LastIndexOf(", ");
                statsString = statsString.Remove(inde2, 2).Insert(inde2, "and ");
            }
        }

        return statsString;
    }

    public void summonAttributeObj(int summonAmount)
    {
        for (int i = 0; i < summonAmount; i++)
        {
            if (teamNumber == 0)
            {
                GameManagerScript.playerPalCount++;
                GameManagerScript.mostAlliedPalCount++;
            }
            else
            {
                GameManagerScript.enemyPalCount++;
            }
            float randomDistance = Random.Range(0f, 3f);
            float randomVectorX = Random.Range(-1f, 1f);
            float randomVectorY = Random.Range(-1f, 1f);
            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, new Vector2(randomVectorX, randomVectorY), randomDistance, summonLayerMask);
            if (rayHit.collider != null)
            {
                if (level == 1)
                {
                    Instantiate(attribute.baseParticleEffect, rayHit.transform.position, Quaternion.identity);
                    Instantiate(attribute.objToSummonLv1, rayHit.point, Quaternion.identity);
                }
                else if (level == 2)
                {
                    Instantiate(attribute.baseParticleEffect, rayHit.transform.position, Quaternion.identity);
                    Instantiate(attribute.objToSummonLv2, rayHit.point, Quaternion.identity);
                }
                else if (level == 3)
                {
                    Instantiate(attribute.baseParticleEffect, rayHit.transform.position, Quaternion.identity);
                    Instantiate(attribute.objToSummonLv3, rayHit.point, Quaternion.identity);
                }
            }
            else if (rayHit.collider == null)
            {
                if (level == 1)
                {
                    GameObject obj = Instantiate(attribute.objToSummonLv1, transform.position, Quaternion.identity);
                    obj.transform.Translate(new Vector2(randomVectorX, randomVectorY) * randomDistance);
                    Instantiate(attribute.baseParticleEffect, obj.transform.position, Quaternion.identity);
                }
                else if (level == 2)
                {
                    GameObject obj = Instantiate(attribute.objToSummonLv2, transform.position, Quaternion.identity);
                    obj.transform.Translate(new Vector2(randomVectorX, randomVectorY) * randomDistance);
                    Instantiate(attribute.baseParticleEffect, obj.transform.position, Quaternion.identity);
                }
                else if (level == 3)
                {
                    GameObject obj = Instantiate(attribute.objToSummonLv3, transform.position, Quaternion.identity);
                    obj.transform.Translate(new Vector2(randomVectorX, randomVectorY) * randomDistance);
                    Instantiate(attribute.baseParticleEffect, obj.transform.position, Quaternion.identity);
                }
            }
            if (teamNumber == 0)
            {
                GameManagerScript.summonedQue--;
            }
            else if (teamNumber > 0)
            {
                GameManagerScript.enemySummonedQue--;
            }
        }
    }
    private void OnMouseEnter()
    {
        if (PalPlacementSystem.palThatWantsToMove == null)
        {
            if (!statIconsArePopping && palCanvas != null)
            {
                makeSureStatsCanBeSeen = true;
            }
        }
        if (PalPlacementSystem.palThatWantsToMove == null && !beganMoveSystem && !GameManagerScript.fightStarted && !PalPlacementSystem.isBuying)
        {
            Vector2 wantedVector = new Vector2(transform.position.x, transform.position.y + 3f);
            Vector2 toScreenVector = mainCam.WorldToScreenPoint(wantedVector);
            string palNameUpdated = "<b>" + palName + "</b>\n<size=23><u> attacks in a range of " + range + " <sprite index=2></u></size>";
            if (attribute != null)
            {
                pSystem.showAttributeText(createAttributeDescription(), palNameUpdated, toScreenVector);
            }
            else
            {
                pSystem.showAttributeText("This pal has no attribute.", palNameUpdated, toScreenVector);
            }
        }
    }
    private void OnMouseExit()
    {
        if (PalPlacementSystem.palThatWantsToMove == null)
        {
            if (!statIconsArePopping && palCanvas != null)
            {
                makeSureStatsCanBeSeen = false;
            }
        }
        if (PalPlacementSystem.palThatWantsToMove == null && !beganMoveSystem && !GameManagerScript.fightStarted && !PalPlacementSystem.isBuying)
        {
            pSystem.hideAttributeText();
        }
    }
    private void OnMouseDrag()
    {
        if (PalPlacementSystem.palThatWantsToMove == null && !beganMoveSystem && GameManagerScript.isOnTurn && teamNumber == 0)
        {
            if (!statIconsArePopping && palCanvas != null)
            {
                makeSureStatsCanBeSeen = true;
            }
            PalPlacementSystem.showSellBox();
            pSystem.hideAttributeText();
            spriteRenderer.sortingOrder = -1;
            spriteRenderer.sortingLayerName = "Character UI's";
            beganMoveSystem = true;
            posBeforeMoveBegan = transform.position;
            PalPlacementSystem.palThatWantsToMove = gameObject;
            PalPlacementSystem.palThatWantsToMovePreviousPos = transform.position;
            PalPlacementSystem.movingPalsOriginalX = startRow;
            PalPlacementSystem.movingPalsOriginalY = startColumn;
            PalPlacementSystem.showIndicator();
            palCollider.enabled = false;


        }
        if (beganMoveSystem)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 wantedPos = new Vector2(mousePos.x, mousePos.y - 0.7f);
            transform.position = Vector2.Lerp(transform.position, wantedPos, Time.deltaTime * 10f);
        }
    }

    public void changeGridPosition(int row, int column)
    {
        startRow = row;
        startColumn = column;
    }

    private void OnMouseUp()
    {
        if (beganMoveSystem && teamNumber == 0)
        {
            if (PalPlacementSystem.movePal())
            {
                if (!statIconsArePopping && palCanvas != null)
                {
                    makeSureStatsCanBeSeen = false;
                }
                startRow = PalPlacementSystem.movingPalsOriginalX;
                startColumn = PalPlacementSystem.movingPalsOriginalY;
                palCollider.enabled = true;
                beganMoveSystem = false;
            }
            else if (!PalPlacementSystem.canPlace && !wInteractive.isLockInsteadOfSell && wInteractive.isMouseOver)
            {
                int moneyGained = 1;
                if (level == 1)
                {
                    moneyGained = 1 * xp + 1;
                }
                else if (level == 2)
                {
                    moneyGained = 1 * xp + 3;
                }
                else if (level == 3)
                {
                    moneyGained = 6;
                }
                GameManagerScript.playerMoney += moneyGained;
                if (attribute != null && attribute.attributeType == PalAttributes.typeOfAttribute.OnSell)
                {
                    if (attribute.attributeEffect == PalAttributes.typeOfEffect.baseEffect)
                    {
                        Instantiate(attribute.baseParticleEffect, transform.position, Quaternion.identity);
                        activateBaseAttribute();
                    }
                    if (attribute.attributeEffect == PalAttributes.typeOfEffect.PalsAround)
                    {
                        List<GameObject> palList = PalPlacementSystem.getPalsAround(startRow, startColumn, attribute.rangeOfEffect);
                        for (int i = 0; i < palList.Count; i++)
                        {
                            SUnitScript unitScript = palList[i].GetComponent<SUnitScript>();
                            Instantiate(attribute.baseParticleEffect, unitScript.attackLocation.position, Quaternion.identity);
                            activateBaseAttribute(unitScript);
                        }
                    }
                }
                PalPlacementSystem.removeMovingPal();
            }
            else
            {
                if (!statIconsArePopping && palCanvas != null)
                {
                    makeSureStatsCanBeSeen = false;
                }
                PalPlacementSystem.palThatWantsToMove = null;
                transform.position = posBeforeMoveBegan;
                palCollider.enabled = true;
                beganMoveSystem = false;
            }
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.sortingLayerName = "Characters";
            PalPlacementSystem.hideSellBox();
            PalPlacementSystem.disableIndicator();
        }
    }
}
