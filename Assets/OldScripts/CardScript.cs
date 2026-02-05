using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardScript : MonoBehaviour
{
    public ShopPal palData;
    public Image palImage;
    public TextMeshProUGUI palName;
    public TextMeshProUGUI attackDesc;
    public TextMeshProUGUI followDesc;
    public TextMeshProUGUI palPriceText;
    public bool isBought = false;
    public RawImage cardImage;
    public GameObject buyButton;
    public GameObject shadow;
    public Image buyButImag;
    public GameObject text1;
    public GameObject text2;
    public Color canBuy;
    public Color cannotBuy;
    public AudioClip clip;

    bool isDuplicate = false;
    int duplicateSlot = 0;

    int actualPrice;
    // Update is called once per frame

    void checkForDuplicate()
    {
        //isDuplicate = false;
        //for (int i = 0; i < GameManagerScript.palList.Count; i++)
        //{
        //    if (GameManagerScript.palList[i] == palData)
        //    {
        //        duplicateSlot = i;
        //        isDuplicate = true;
        //        break;
        //    }
        //}
    }
    void Update()
    {
        //if (isBought == false)
        //{
        //    checkForDuplicate();
        //    shadow.SetActive(true);
        //    palName.enabled = true;
        //    attackDesc.enabled = true;
        //    followDesc.enabled = true;
        //    palImage.enabled = true;
        //    cardImage.enabled = true;
        //    buyButton.SetActive(true);
        //    text1.SetActive(true);
        //    text2.SetActive(true);

        //    actualPrice = (int)(palData.price * (1 + GameManagerScript.waveNumber * 0.1f));
        //    palName.text = palData.palName;
        //    attackDesc.text = palData.palAtkDescription;
        //    followDesc.text = palData.palFollowDescription;
        //    palPriceText.text = "" + actualPrice;
        //    palImage.sprite = palData.palSprite;
        //    cardImage.color = palData.palCardColor;

        //    if (isDuplicate == false)
        //    {
        //        if (GameManagerScript.playerMoney >= actualPrice && GameManagerScript.palList.Count < 6)
        //        {
        //            buyButImag.color = canBuy;
        //        }
        //        else
        //        {
        //            buyButImag.color = cannotBuy;
        //        }
        //    }
        //    else if (GameManagerScript.palLevel[duplicateSlot] != 5 && GameManagerScript.playerMoney >= actualPrice)
        //    {
        //        buyButImag.color = canBuy;
        //    }
        //    else
        //    {
        //        buyButImag.color = cannotBuy;
        //    }


        //}
        //else
        //{
        //    shadow.SetActive(false);
        //    palName.enabled = false;
        //    attackDesc.enabled = false;
        //    followDesc.enabled = false;
        //    palImage.enabled = false;
        //    cardImage.enabled = false;
        //    buyButton.SetActive(false);
        //    text1.SetActive(false);
        //    text2.SetActive(false);
        //}
    }

    public void buyPal()
    {
        //if (GameManagerScript.playerHasStartedWave == false)
        //{
        //    if (isDuplicate == false)
        //    {
        //        if (GameManagerScript.playerMoney >= actualPrice && GameManagerScript.palList.Count < 6)
        //        {
        //            SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        //            GameManagerScript.playerMoney -= actualPrice;
        //            GameManagerScript.palList.Add(palData);
        //            GameManagerScript.palLevel.Add(1);
        //            isBought = true;
        //        }
        //    }
        //    else if (GameManagerScript.palLevel[duplicateSlot] != 5 && GameManagerScript.playerMoney >= actualPrice)
        //    {
        //        SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        //        GameManagerScript.playerMoney -= actualPrice;
        //        GameManagerScript.palLevel[duplicateSlot] += 1;
        //        isBought = true;
        //    }
        //}
    }
}
