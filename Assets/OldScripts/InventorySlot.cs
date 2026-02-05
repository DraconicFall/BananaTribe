using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotID;
    public Image image;
    public Sprite emptyImage;
    public TextMeshProUGUI levelText;

    public GameObject sellButton;
    public TextMeshProUGUI sellText;
    int sellPrice;
    // Update is called once per frame
    void Update()
    {
        //if (GameManagerScript.palList.Count > slotID)
        //{
        //    image.sprite = GameManagerScript.palList[slotID].palSprite;
        //    if (GameManagerScript.palLevel[slotID] == 5)
        //    {
        //        levelText.text = "LVMAX";
        //    }
        //    else
        //    {
        //        levelText.text = "LV" + GameManagerScript.palLevel[slotID];
        //    }
        //    sellPrice = (int)(GameManagerScript.palList[slotID].price * GameManagerScript.palLevel[slotID] / 2f);
        //    sellText.text = "" + sellPrice;
        //}
        //else
        //{
        //    levelText.text = "";
        //    image.sprite = emptyImage;
        //}
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (GameManagerScript.palList.Count > slotID)
        //{
        //    sellButton.SetActive(true);
        //}
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //if (GameManagerScript.palList.Count > slotID)
        //{
        //    sellButton.SetActive(false);
        //}
    }

    public void sell()
    {
        //GameManagerScript.playerMoney += sellPrice;
        //GameManagerScript.palList.RemoveAt(slotID);
        //GameManagerScript.palLevel.RemoveAt(slotID);
        //sellButton.SetActive(false);
    }
}
