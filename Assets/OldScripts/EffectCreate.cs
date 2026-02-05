using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Effect")]
public class EffectCreate : ScriptableObject
{
    public enum EffectType
    {
        Bleeding,
        Chained,
        EhwazDagger,
        Burning,
        Poison,
        Stun,
        Frozen,
        Electrified
    }
    public EffectType effectType;
    public float effectAmount; //damage, slow amount, weak amount, other stuff
    public float timeAmount = 0;
}
