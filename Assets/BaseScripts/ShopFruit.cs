using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Shop Fruit")]
public class ShopFruit : ScriptableObject
{
    public Sprite fruitSprite;
    public string fruitName;
    public int healthIncrease = 1;
    public int speedIncrease = 1;
    public int firerateIncrease = 1;
    public int damageIncrease = 1;
    public bool choosesRandom = false;
    public int randomCount = 0;
}
