using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainShop : MonoBehaviour
{
    public ShopPal[] commonPals;
    public ShopFruit[] commonFruits;

    public ShopPal[] uncommonPals; // used later when adding in uncommons and other stuff
    public ShopFruit[] uncommonFruits; // used later when adding in uncommons and other stuff

    public void fightEndReroll()
    {
        BuyerScript[] shopList = new BuyerScript[10];
        shopList = GameObject.FindObjectsOfType<BuyerScript>();

        foreach (var shop in shopList)
        {
            if (shop != null && !shop.isLocked)
            {
                if (shop.shopForFruit)
                {
                    shop.shopFruit = commonFruits[Random.Range(0, commonFruits.Length)];
                }
                else
                {
                    shop.shopForWhat = commonPals[Random.Range(0, commonPals.Length)];
                }
                shop.rerolled();
            }
        }
    }
    public void reroll()
    {
        if (GameManagerScript.playerMoney >= 1)
        {
            GameManagerScript.playerMoney -= 1;
            BuyerScript[] shopList = new BuyerScript[10];
            shopList = GameObject.FindObjectsOfType<BuyerScript>();

            foreach (var shop in shopList)
            {
                if (shop != null && !shop.isLocked)
                {
                    if (shop.shopForFruit)
                    {
                        shop.shopFruit = commonFruits[Random.Range(0, commonFruits.Length)];
                    }
                    else
                    {
                        shop.shopForWhat = commonPals[Random.Range(0, commonPals.Length)];
                    }
                    shop.rerolled();
                }
            }
        }
    }
}
