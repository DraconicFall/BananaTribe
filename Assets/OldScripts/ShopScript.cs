using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{
    public List<ShopPal> palList;
    public CardScript card1;
    public CardScript card2;
    public CardScript card3;
    public int rerollPrice = 0;
    public int rerolledTimes = 0;
    public Color canBuy;
    public Color cannotBuy;
    public Image rButtonImag;
    public TextMeshProUGUI rerollButton;
    public AudioClip clip;
    // Update is called once per frame
    private void Start()
    {
        switchCards();
    }
    void Update()
    {
        rerollButton.text = "" + rerollPrice;
        if (GameManagerScript.playerMoney >= rerollPrice)
        {
            rButtonImag.color = canBuy;
        }
        else
        {
            rButtonImag.color = cannotBuy;
        }
    }
    public void resetShop()
    {
        switchCards();
        rerollPrice = 0;
        rerolledTimes = 0;
    }

    public void switchCards()
    {
        card1.palData = palList[Random.Range(0, palList.Count)];
        card1.isBought = false;
        card2.palData = palList[Random.Range(0, palList.Count)];
        card2.isBought = false;
        card3.palData = palList[Random.Range(0, palList.Count)];
        card3.isBought = false;
    }

    public void reroll()
    {
        //if (GameManagerScript.playerMoney >= rerollPrice && GameManagerScript.playerHasStartedWave == false)
        //{
        //    SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        //    GameManagerScript.playerMoney -= rerollPrice;
        //    switchCards();
        //    rerolledTimes++;
        //    rerollPrice += (int)(rerolledTimes * 1.25f);
        //}
    }
}
