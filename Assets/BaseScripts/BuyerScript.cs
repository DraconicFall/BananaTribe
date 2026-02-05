using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;

public class BuyerScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerClickHandler, IDropHandler, IBeginDragHandler, IEndDragHandler
{
    public bool isSelected = false;
    public GameObject draggedArrow;
    GameObject currentActiveArrow;
    GameObject canvas;
    RectTransform rectTransform;
    public ShopPal shopForWhat;
    public bool shopForFruit = false;
    public ShopFruit shopFruit;
    public Sprite boughtSprite;
    public Image isLockedImage;
    public Image image;

    public GameObject statsObj;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI firerateText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI damageText;

    public bool isLocked = false;
    bool isBought = false;

    PalPlacementSystem pSystem;
    Camera mainCam;

    public void Awake()
    {
        pSystem = GameObject.Find("PlacementSystem").GetComponent<PalPlacementSystem>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        isLockedImage.enabled = false;
        rectTransform = GetComponent<RectTransform>();
        canvas = GameObject.Find("Canvas");
        if (image == null)
        {
            image = gameObject.GetComponent<Image>();
        }
        if (shopForFruit)
        {
            image.sprite = shopFruit.fruitSprite;
        }
        else
        {
            image.sprite = shopForWhat.palSprite;
        }
    }

    public void lockThisShop()
    {
        isLocked = true;
        isLockedImage.enabled = true;
    }
    public void unlockThisShop()
    {
        isLocked = false;
        isLockedImage.enabled = false;
    }

    public void rerolled() //doesnt do the actual reroll, just changes the sprites + other information
    {
        isBought = false;
        if (shopForFruit)
        {
            image.sprite = shopFruit.fruitSprite;
        }
        else
        {
            image.sprite = shopForWhat.palSprite;
            healthText.text = "" + shopForWhat.baseHealth;
            firerateText.text = "" + shopForWhat.baseFirerate;
            speedText.text = "" + shopForWhat.baseSpeed;
            damageText.text = "" + shopForWhat.baseDamage;
            statsObj.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isBought)
        {
            if (!shopForFruit)
            {
                Vector2 wantedVector = new Vector2(transform.position.x, transform.position.y + 190);
                SUnitScript unitScript = shopForWhat.thePal.GetComponent<SUnitScript>();
                string palNameUpdated = "<b>" + unitScript.palName + "</b>\n<size=23><u> attacks in a range of " + shopForWhat.baseRange + " <sprite index=2></u></size>";
                pSystem.showAttributeText(unitScript.createAttributeDescription(), palNameUpdated, wantedVector);
            }
            else
            {
                Vector2 wantedVector = new Vector2(transform.position.x, transform.position.y + 190);
                string fruitNameUpdated = "<b>" + shopFruit.fruitName + "</b>";
                pSystem.showAttributeText(createFruitDescription(), fruitNameUpdated, wantedVector);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!shopForFruit)
        {
            pSystem.hideAttributeText();
        }
        else
        {
            pSystem.hideAttributeText();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //isSelected = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isBought && !GameManagerScript.prefightStarted)
        {
            PalPlacementSystem.isBuying = true;
            PalPlacementSystem.showLockBox();
            if (!shopForFruit)
            {
                PalPlacementSystem.currentBuyingPal = shopForWhat.thePal.GetComponent<SUnitScript>();
            }
            //PalPlacementSystem.showOrDisableIndicator();
            currentActiveArrow = Instantiate(draggedArrow, canvas.transform);
            currentActiveArrow.transform.position = rectTransform.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isBought && !GameManagerScript.prefightStarted)
        {
            isSelected = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropArrowScript dragDropArrowScript = currentActiveArrow.GetComponent<DragDropArrowScript>();
        if (dragDropArrowScript != null)
        {
            dragDropArrowScript.destroyArrow();

        }
        if (!isBought && !GameManagerScript.prefightStarted)
        {
            if (PalPlacementSystem.canPlace)
            {
                if (!shopForFruit)
                {
                    int health = shopForWhat.baseHealth;
                    int speed = shopForWhat.baseSpeed;
                    int firerate = shopForWhat.baseFirerate;
                    int damage = shopForWhat.baseDamage;
                    int range = shopForWhat.baseRange;
                    if (GameManagerScript.playerMoney >= 3 && PalPlacementSystem.placePal(shopForWhat.thePal, health, speed, firerate, damage, range))
                    {
                        GameManagerScript.playerMoney -= 3;
                        image.sprite = boughtSprite;
                        isBought = true;
                        statsObj.SetActive(false);
                        unlockThisShop();
                    }
                }
                else
                {
                    int health = shopFruit.healthIncrease;
                    int speed = shopFruit.speedIncrease;
                    int firerate = shopFruit.firerateIncrease;
                    int damage = shopFruit.damageIncrease;
                    if (GameManagerScript.playerMoney >= 3 && PalPlacementSystem.placeFruit(health, speed, firerate, damage))
                    {
                        GameManagerScript.playerMoney -= 3;
                        image.sprite = boughtSprite;
                        isBought = true;
                        unlockThisShop();
                    }
                }
            }
            else
            {
                WorldInteracterUI wInteractive = GameObject.Find("ShopLocker").GetComponent<WorldInteracterUI>();
                //Debug.Log(wInteractive.gameObject);
                if (wInteractive != null && wInteractive.isLockInsteadOfSell)
                {
                    if (wInteractive.isMouseOver)
                    {
                        if (isLocked)
                        {
                            unlockThisShop();
                        }
                        else
                        {
                            lockThisShop();
                        }
                    }
                }
            }
            pSystem.hideAttributeText();
            PalPlacementSystem.currentBuyingPal = null;
            PalPlacementSystem.isBuying = false;
            PalPlacementSystem.hideLockBox();
            //PalPlacementSystem.showOrDisableIndicator();
            currentActiveArrow = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isBought && !GameManagerScript.prefightStarted)
        {
            isSelected = false;
        }
    }

    public string createFruitDescription()
    {
        string returner = "";
        if (!shopFruit.choosesRandom)
        {
            returner += "Gives one pal ";
        }

        returner += statDescriptionCreator();
        returner += ".";

        return returner;
    }

    string statDescriptionCreator()
    {
        int statsIncreasedCounter = 0;
        string statsString = "";
        if (shopFruit.healthIncrease > 0) //make sure to add in sprites once they are in game
        {
            statsIncreasedCounter++;
            statsString += "+" + shopFruit.healthIncrease + " <sprite index=1> health, ";
        }
        if (shopFruit.firerateIncrease > 0)
        {
            statsIncreasedCounter++;
            statsString += "+" + shopFruit.firerateIncrease + " <sprite index=6> firerate, ";
        }
        if (shopFruit.speedIncrease > 0)
        {
            statsIncreasedCounter++;
            statsString += "+" + shopFruit.speedIncrease + " <sprite index=4> speed, ";
        }
        if (shopFruit.damageIncrease > 0)
        {
            statsIncreasedCounter++;
            statsString += "+" + shopFruit.damageIncrease + " <sprite index=5> damage, ";
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
}
