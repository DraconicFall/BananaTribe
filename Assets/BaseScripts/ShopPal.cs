using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Shop Pal")]
public class ShopPal : ScriptableObject
{
    public Sprite palSprite;
    public string palName;
    public GameObject thePal;
    public int baseHealth = 1;
    public int baseSpeed = 1;
    public int baseFirerate = 1;
    public int baseDamage = 1;
    public int baseRange = 1;
}
