using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Pal Attribute")]
public class PalAttributes : ScriptableObject
{
    public enum customTargeting
    {
        NormalTargeting,
        LowestHealth,
    }

    public enum typeOfAttribute
    {
        OnFightStart,
        DuringFight,
        OnSell,
        OnTurnStart,
        OnFaint,
        OnFriendFaint,
        OnFirstPlace,
        OnDamageTaken,
    }

    public enum typeOfEffect
    {
        baseEffect,
        dealPercentMore,
        ConnectAround, //effected by rangeOfEffect
        ConnectAhead, //effected by rangeOfEffect
        Summon,
        SummonAt,
        PalsAhead, //effected by rangeOfEffect
        PalsHorizontal, //effected by rangeOfEffect
        PalsBehind, //effected by rangeOfEffect
        PalsAround, //effected by rangeOfEffect
        DealDamage, //effected by rangeOfEffects
        EveryPal,
        ForEveryPalAround, //effected by rangeOfEffect
        NearbyPal,
        NearestEnemyTakesDamage,
        EnemyDefeated,
        DealDamageToEveryEnemy,
        testForAttributes,
    }

    public enum typeOfConnect
    {
        NoConnect,
        BaseConnect, //just stats
        ConnectedSummons,
        SummonAtConnected
    }
    [Header("base unique effects")]
    public bool doesThisPalTaunt = false;
    public int tauntRange = 0;

    [Header("specific attribute parts")]
    public bool dealDamageTargetsFarthest = false;
    public int damageDeltFromAttribute = 0; //only for DealDamage and DealDamageToEveryEnemy
    public float damagePercentIncrease;
    public int orderInFightStart; //0 happens first, with later numbers up to X happening last.

    [Header("connection related effects")]
    public typeOfConnect connectionType;
    public GameObject startConnectionEffect;
    public GameObject connectionEffect;
    public string connectionBasedTextLv1;
    public string connectionBasedTextLv2;
    public string connectionBasedTextLv3;

    [Header("Base attribute effects")]
    public customTargeting targetingType;
    public string uniqueDescriptionEndText;
    public GameObject baseParticleEffect;
    public typeOfAttribute attributeType;
    public typeOfEffect attributeEffect;
    public GameObject objToSummonLv1;
    public int AttacksTillEffect; //amount of attacks till proccing attribute once.
    public int rangeOfEffect; //for those attribute effects based on other pals.
    public int healthIncrease;
    public int damageIncrease;
    public int speedIncrease;
    public int firerateIncrease;
    public int goldIncrease;
    public int rangeIncrease;
    public int shieldIncrease;
    public int piercingIncrease;

    [Header("Base leveling effects")]
    public GameObject objToSummonLv2;
    public GameObject objToSummonLv3;
    public bool lvlUpHealthIncrease;
    public bool lvlUpDamageIncrease;
    public bool lvlUpSpeedIncrease;
    public bool lvlUpFirerateIncrease;
    public bool lvlUpGoldIncrease;
    public bool lvlUpRangeIncrease;
    public bool lvlUpShieldIncrease;
    public bool lvlUpPiercingIncrease;
}
