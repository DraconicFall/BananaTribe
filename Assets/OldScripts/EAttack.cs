using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Attack")]
public class EAttack : ScriptableObject
{
    public enum attackType
    {
        Aimed,
        Constant,
        Movement,
        Combine,
        Sequential
    }
    public enum atkMovementType
    {
        dashAndTPTowards,
        dashAndTPAway,
        FullShield, //Make sure that invincibleWhenAttack is true when this is on. (MAYBE???)
        jumpTowardsTimedBased,
        jumpTowardsDistanceBased,
    }
    public attackType typeOfAttack;
    public bool invincibleWhenAttack = false; //affects whether or not the enemy will be invincible when it is attacking
    [Header("Other")]
    public float atkDelayS = 0; //attack delay at the start.
    public float atkDelayE = 0; // attack delay at the end
    public GameObject attackIndicator; //for now only works with jumping
    public GameObject otherIndicator; //could be something like for shadows
    public bool deleteSelfAfterAtk = false;
    [Header("Audio")]
    public AudioClip attackFX;
    public float volumeFX;
    public float pitchMinFX;
    public float pitchMaxFX;
    [Header("Animation")]
    public bool usesAnimation = true;
    public bool playAnimationBeforeWait = true;
    public bool facePlayerWhenAnimating = true;
    public bool idleAtEnd = true;
    public string idleAnimation;
    public string attackingAnimation;
    //public string endAnimation; //replaces idle animation if exists.
    public GameObject animationParticleEffect; //if not null, will create a object as part of the animation.
    public Vector2 animPartEffectPos; //(0, 0) is enemy center pos.
    public float idleAnimDelay = 0; //time till start idleAnimation
    public float attackAnimDelay; //when to start attackingAnimation;
    public float animEndTime; //how long to wait until ending the animation

    [Header("Aimed * Constant Field")]
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private int BulletsAmounts = 1;
    [SerializeField]
    private float startRegularAngle = 0.0f, endRegularAngle = 360.0f;
    public bool randomAngles = false;
    public int RepeatAmount = 1;
    public float RepeatRate = 0.0f;
    public bool alwaysChangeTarget = false;

    [Header("Movement")]
    public atkMovementType movementType;
    public float repeatAmount = 0; //the amount of times enemy repeats something. Ex. Dashing, not really used for shield,
        //Jumping = jump height multiplier.
    public float mAmount = 0; //the amount enemy acts everytime it acts. Ex. Dash amount, or shield health
        //Jumping = distance to the target, can be negative.
    public float mRate = 0; //amount of sec before enacts another movement. Ex. dash, how long the shield lasts,
        //JumpingTimeBased = how long the jump will take.
        //JumpingDistanceBased = the speed of the jump.
    public GameObject mEffect; //the effect that plays when acting.
    public bool rDEAtEnd = false; //whether or not enemy repeats mEffect at the end, more likely for TP

    [Header("Combined * Sequential")]
    public EAttack[] attacksToCombine;
    public bool canAtk = true;

    public IEnumerator attack(Transform transform, Vector3 attackLocations, Vector3 target)
    {
        canAtk = false;
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
                    bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulMoveVector, startposition, 0, 1, 1, 0);
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
                bul.GetComponent<RegularEnemyBulletScript>().SetMoveDirection(bulDir, angle, 0, 1, 1, 0);
                angle += angleStep;
            }
            yield return new WaitForSeconds(RepeatRate);
        }
        else if (typeOfAttack == attackType.Combine)
        {
            for (int i = 0; i < attacksToCombine.Length; i++)
            {

            }
        }
        canAtk = true;
    }
}
