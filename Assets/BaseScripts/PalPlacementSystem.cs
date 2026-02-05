using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI.Table;

public class PalPlacementSystem : MonoBehaviour
{
    static public bool isBuying;
    static public SUnitScript currentBuyingPal;
    bool calledAttributeAlreadyWhileBuying = false;
    SUnitScript previouslySelectedPalWhileInBuyingMode;

    public GameObject lockObj;
    public GameObject sellObj;

    static public GameObject lockBox;
    static public GameObject sellBox;

    public GameObject attributeDescription;
    RectTransform attributeRect;
    public TextMeshProUGUI attributeText;
    public TextMeshProUGUI palNameText;

    public GameObject cellIndicator;
    static public SpriteRenderer cellIndicatorRenderer;
    static public bool canPlace = true;
    public Color canPlaceColor;
    public Color cannotPlaceColor;
    static public int cellIndicatorRow = 0;
    static public int cellIndicatorColumn = 0;
    public Grid grid;
    static Vector2 cellIndicatorPos;
    static public GameObject[][] palGrid;
    static public GameObject[][] palGridCopy;
    Vector2 startPos;
    Vector2 curPosOffsetted;

    static public GameObject palThatWantsToMove;
    static public Vector2 palThatWantsToMovePreviousPos;
    static public int movingPalsOriginalX;
    static public int movingPalsOriginalY;

    private void Awake()
    {
        lockBox = lockObj;
        sellBox = sellObj;
        attributeRect = attributeDescription.GetComponent<RectTransform>();
        palGrid = new GameObject[5][]; //NEED THIS OR NOTHING WORKS
        palGridCopy = new GameObject[5][];
        for (int i = 0; i < palGrid.Length; i++)
        {
            palGrid[i] = new GameObject[6];
        }
        for (int i = 0; i < palGridCopy.Length; i++)
        {
            palGridCopy[i] = new GameObject[6];
        }
        startPos = cellIndicator.transform.position;
        cellIndicatorRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        cellIndicatorRenderer.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        Camera mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = grid.WorldToCell(mousePos);
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);

        curPosOffsetted = new Vector2(cellIndicator.transform.position.x - startPos.x, cellIndicator.transform.position.y - startPos.y);
        cellIndicatorRow = (int)(curPosOffsetted.y / 1.5f);
        cellIndicatorColumn = (int)(curPosOffsetted.x / 1.5f);

        cellIndicatorPos = cellIndicator.transform.position;
        if (cellIndicatorRow >= 0 && cellIndicatorRow <= 4 && cellIndicatorColumn >= 0 && cellIndicatorColumn <= 5)
        {
            canPlace = true;
            showIndicator();
            cellIndicatorRenderer.color = canPlaceColor;
            if (isBuying)
            {
                if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null)
                {
                    if (currentBuyingPal != null && (currentBuyingPal.name + "(Clone)") != palGrid[cellIndicatorRow][cellIndicatorColumn].name)
                    {
                        cellIndicatorRenderer.color = cannotPlaceColor;
                    }

                    if (!calledAttributeAlreadyWhileBuying)
                    {
                        calledAttributeAlreadyWhileBuying = true;
                        SUnitScript unitScript = palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>();
                        previouslySelectedPalWhileInBuyingMode = unitScript;
                        Vector2 wantedVector = new Vector2(unitScript.transform.position.x, unitScript.transform.position.y + 3f);
                        Vector2 toScreenVector = mainCam.WorldToScreenPoint(wantedVector);
                        string palNameUpdated = "<b>" + unitScript.palName + "</b>\n<size=23><u> attacks in a range of " + unitScript.range + " <sprite index=2></u></size>";
                        showAttributeText(unitScript.createAttributeDescription(), palNameUpdated, toScreenVector);
                    }
                    else if (previouslySelectedPalWhileInBuyingMode != palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>())
                    {
                        previouslySelectedPalWhileInBuyingMode = null;
                        calledAttributeAlreadyWhileBuying = false;
                        hideAttributeText();
                    }
                }
                else if (palGrid[cellIndicatorRow][cellIndicatorColumn] == null)
                {
                    previouslySelectedPalWhileInBuyingMode = null;
                    calledAttributeAlreadyWhileBuying = false;
                    hideAttributeText();
                }
            }
            else if (!isBuying)
            {
                previouslySelectedPalWhileInBuyingMode = null;
                calledAttributeAlreadyWhileBuying = false;
            }

        }
        else
        {
            canPlace = false;
            disableIndicator();
            
        }

        if (GameManagerScript.fightStarted)
        {
            hideAttributeText();
        }
    }

    static public GameObject findGridCurPosition()
    {
        if (cellIndicatorRow >= 0 && cellIndicatorRow <= 4 && cellIndicatorColumn >= 0 && cellIndicatorColumn <= 5)
        {
            if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null)
            {
                return palGrid[cellIndicatorRow][cellIndicatorColumn];
            }
        }
        return null;
    }

    static public void hideLockBox()
    {
        lockBox.GetComponent<WorldInteracterUI>().isMouseOver = false;
        lockBox.SetActive(false);
    }
    static public void showLockBox()
    {
        lockBox.SetActive(true);
        lockBox.GetComponent<WorldInteracterUI>().StartCoroutine(lockBox.GetComponent<WorldInteracterUI>().RectPop());
    }
    static public void hideSellBox()
    {
        sellBox.SetActive(false);
    }
    static public void showSellBox()
    {
        sellBox.SetActive(true);
        sellBox.GetComponent<WorldInteracterUI>().StartCoroutine(sellBox.GetComponent<WorldInteracterUI>().RectPop());
    }

    public void onlySetAttributeText(string attributeDescriptText)
    {
        attributeText.text = attributeDescriptText;
    }

    public void hideAttributeText()
    {
        attributeDescription.SetActive(false);
    }

    IEnumerator attributePopIn()
    {
        attributeRect.localScale = new Vector2(1.12f, 1.12f);
        while (attributeRect.localScale.x >= 1.05)
        {
            yield return new WaitForSeconds(0.01f);
            attributeRect.localScale = Vector2.Lerp(attributeRect.localScale, new Vector2(1, 1), 0.5f);
        }
        attributeRect.localScale = new Vector2(1f, 1f);
    }

    public void showAttributeText(string attributeDescriptText, string nameOfPal, Vector2 position2)
    {
        StartCoroutine(attributePopIn());
        Vector2 apos = attributeRect.position;
        float xpos = apos.x;
        float ypos = apos.y;
        xpos = Mathf.Clamp(position2.x, attributeRect.rect.width * 0.51f, Screen.width - attributeRect.rect.width * 0.525f);
        ypos = Mathf.Clamp(position2.y, attributeRect.rect.height * 0.51f, Screen.height - attributeRect.rect.height * 0.525f);

        apos.x = xpos;
        apos.y = ypos;
        attributeRect.position = apos;

        //rectTransform.position = position2;
        attributeDescription.SetActive(true);
        attributeText.text = attributeDescriptText;
        palNameText.text = nameOfPal;
    }

    static public void showIndicator()
    {
        cellIndicatorRenderer.enabled = true;
    }

    static public void disableIndicator()
    {
        cellIndicatorRenderer.enabled = false;
    }

    static public void showOrDisableIndicator()
    {
        if (cellIndicatorRenderer.enabled)
        {
            cellIndicatorRenderer.enabled = false;
        }
        else
        {
            cellIndicatorRenderer.enabled = true;
        }
    }

    static public List<GameObject> getPalsAround(int row, int column, int range)
    {
        List<GameObject> palList = new List<GameObject>();
        for (int i = row - range; i < row + 2 * range; i++)
        {
            for (int j = column - range; j < column + 2 * range; j++)
            {
                if (i >= 0 && i <= 4 && j >= 0 && j <= 5)
                {
                    if (!(i == row && j == column) && palGrid[i][j] != null)
                    {
                        palList.Add(palGrid[i][j]);
                    }
                }
            }
        }
        return palList;
    }

    static public List<GameObject> getPalInFront(int row, int column)
    {
        List<GameObject> palList = new List<GameObject>();
        if (row >= 0 && row <= 4 && column + 1 >= 0 && column + 1 <= 5)
        {
            if (palGrid[row][column + 1] != null)
            {
                palList.Add(palGrid[row][column + 1]);
            }
        }
        return palList;
    }
    static public List<GameObject> getPalsInFront(int row, int column, int range)
    {
        List<GameObject> palList = new List<GameObject>();
        for (int j = column; j < column + range; j++)
        {
            if (row >= 0 && row <= 4 && j >= 0 && j <= 5)
            {
                if (!(j == column) && palGrid[row][j] != null)
                {
                    palList.Add(palGrid[row][j]);
                }
            }
        }
        return palList;
    }

    static public List<GameObject> getAllPals(int startRow, int startColumn)
    {
        List<GameObject> palList = new List<GameObject>();
        for (int i = 0; i < palGrid.Length; i++)
        {
            for (int j = 0; j < palGrid[i].Length; j++)
            {
                if (!(i == startRow && j == startColumn) && palGrid[i][j] != null)
                {
                    palList.Add(palGrid[i][j]);
                }
            }
        }
        return palList;
    }

    static public void saveCurrentField()
    {
        for (int i = 0; i <= 4; i++)
        {
            for (int x = 0; x <= 5; x++)
            {
                if (palGrid[i][x] != null)
                {
                    SUnitScript SUnit = palGrid[i][x].GetComponent<SUnitScript>();
                    Vector2 palCords = SUnit.gameObject.transform.position;
                    palGridCopy[i][x] = Instantiate(palGrid[i][x], palCords, Quaternion.identity);
                    palGridCopy[i][x].GetComponent<SUnitScript>().firstPlaceEffectDone = true;
                    palGridCopy[i][x].name = palGridCopy[i][x].name.Substring(0, palGridCopy[i][x].name.Length - 7);
                    palGridCopy[i][x].SetActive(false);
                }
            }
        }
    }

    static public void resetField() //supposed to be called at turn start
    {
        SUnitScript[] unitsToDelete = GameObject.FindObjectsOfType<SUnitScript>();
        for (int index = 0; index < unitsToDelete.Length; index++)
        {
            unitsToDelete[index].destroyThisObj();
        }

        for (int i = 0; i <= 4; i++)
        {
            for (int x = 0; x <= 5; x++)
            {
                if (palGridCopy[i][x] != null)
                {
                    palGridCopy[i][x].SetActive(true);
                }
            }
        }

        for (int row = 0; row <= 4; row++)
        {
            for (int column = 0; column <= 5; column++)
            {
                palGrid[row][column] = palGridCopy[row][column];
            }
        }
    }

    static public bool movePal() //called after releasing mouse to move pal.
    {
        if (canPlace)
        {
            if (palGrid[cellIndicatorRow][cellIndicatorColumn] == null)
            {
                palGrid[cellIndicatorRow][cellIndicatorColumn] = palThatWantsToMove;
                palGrid[movingPalsOriginalX][movingPalsOriginalY] = null;
                palThatWantsToMove.transform.position = new Vector2(cellIndicatorPos.x + 0.75f, cellIndicatorPos.y + 0.75f);
                palThatWantsToMove = null;
                movingPalsOriginalX = cellIndicatorRow;
                movingPalsOriginalY = cellIndicatorColumn;
                return true;
            }
            else if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null && palGrid[cellIndicatorRow][cellIndicatorColumn].name == palThatWantsToMove.name && palGrid[cellIndicatorRow][cellIndicatorColumn] != palThatWantsToMove && palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>().level != 3 && palThatWantsToMove.GetComponent<SUnitScript>().level != 3)
            {
                int repeatAmount = 0;
                SUnitScript unitScript = palThatWantsToMove.GetComponent<SUnitScript>();
                SUnitScript unitToLevelUp = palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>();

                if (unitScript.level == 1)
                {
                    repeatAmount = 1 * unitScript.xp + 1;
                }
                else if (unitScript.level == 2)
                {
                    repeatAmount = 1 * unitScript.xp + 3;
                }
                else if (unitScript.level == 3)
                {
                    repeatAmount = 6;
                }
                for (int i = 0; i < repeatAmount; i++)
                {
                    Instantiate(unitToLevelUp.gameManager.sUnitXPUp, unitToLevelUp.attackLocation.position, Quaternion.identity);
                    int potentialDifferenceInStats = 0;
                    if (unitScript.level == 1)
                    {
                        potentialDifferenceInStats = unitScript.xp;
                    }
                    if (unitScript.level == 2)
                    {
                        potentialDifferenceInStats = unitScript.xp + 2;
                    }
                    if (unitScript.level == 3)
                    {
                        potentialDifferenceInStats = 5;
                    }

                    if (unitScript.health - potentialDifferenceInStats > unitToLevelUp.health)
                    {
                        unitToLevelUp.health = unitScript.health - potentialDifferenceInStats;
                    }
                    if (unitScript.firerate - potentialDifferenceInStats > unitToLevelUp.firerate)
                    {
                        unitToLevelUp.firerate = unitScript.firerate - potentialDifferenceInStats;
                    }
                    if (unitScript.speed - potentialDifferenceInStats > unitToLevelUp.speed)
                    {
                        unitToLevelUp.speed = unitScript.speed - potentialDifferenceInStats;
                    }
                    if (unitScript.damage - potentialDifferenceInStats > unitToLevelUp.damage)
                    {
                        unitToLevelUp.damage = unitScript.damage - potentialDifferenceInStats;
                    }
                    unitToLevelUp.DraggedXPUp();
                }

                Destroy(palGrid[movingPalsOriginalX][movingPalsOriginalY]);
                palGrid[movingPalsOriginalX][movingPalsOriginalY] = null;
                palThatWantsToMove = null;
            }
            else if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null && palGrid[cellIndicatorRow][cellIndicatorColumn] != palThatWantsToMove)
            {
                palGrid[movingPalsOriginalX][movingPalsOriginalY] = null;
                palGrid[movingPalsOriginalX][movingPalsOriginalY] = palGrid[cellIndicatorRow][cellIndicatorColumn];
                palGrid[movingPalsOriginalX][movingPalsOriginalY].transform.position = palThatWantsToMovePreviousPos;
                palGrid[movingPalsOriginalX][movingPalsOriginalY].GetComponent<SUnitScript>().changeGridPosition(movingPalsOriginalX, movingPalsOriginalY);

                palGrid[cellIndicatorRow][cellIndicatorColumn] = palThatWantsToMove;
                palThatWantsToMove.transform.position = new Vector2(cellIndicatorPos.x + 0.75f, cellIndicatorPos.y + 0.75f);
                palThatWantsToMove = null;
                movingPalsOriginalX = cellIndicatorRow;
                movingPalsOriginalY = cellIndicatorColumn;
                return true;
            }
            //else
            //{
            //    palThatWantsToMove.transform.position = new Vector2(movingPalsOriginalX + 0.75f, movingPalsOriginalY + 0.75f);
            //}
        }
        return false;
    }

    static public void removeMovingPal()
    {
        palThatWantsToMove.GetComponent<SUnitScript>().destroyThisObj();
        palGrid[movingPalsOriginalX][movingPalsOriginalY] = null;
        palThatWantsToMove = null;
    }

    static public bool placePal(GameObject placedObj, int health, int speed, int firerate, int damage, int range)
    {
        if (canPlace)
        {
            if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null)
            {
                SUnitScript UnitScript = palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>();
                if (palGrid[cellIndicatorRow][cellIndicatorColumn].name == placedObj.name + "(Clone)" && UnitScript.XPUp())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Vector2 placedPos = new Vector2(cellIndicatorPos.x + 0.75f, cellIndicatorPos.y + 0.75f);
                GameObject pal = Instantiate(placedObj, placedPos, Quaternion.identity);
                SUnitScript sUnitScript = pal.GetComponent<SUnitScript>();
                sUnitScript.health = health;
                sUnitScript.speed = speed;
                sUnitScript.firerate = firerate;
                sUnitScript.damage = damage;
                sUnitScript.range = range;
                sUnitScript.updateActualStats();


                sUnitScript.startRow = cellIndicatorRow;
                sUnitScript.startColumn = cellIndicatorColumn;
                palGrid[cellIndicatorRow][cellIndicatorColumn] = pal;
                return true;
            }
        }
        return false;
    }

    static public bool placeFruit(int health, int speed, int firerate, int damage)
    {
        if (canPlace)
        {
            if (palGrid[cellIndicatorRow][cellIndicatorColumn] != null)
            {
                int noStatsIncreased = 0;
                int howManyStatsRelevant = 0;
                SUnitScript sUnitScript = palGrid[cellIndicatorRow][cellIndicatorColumn].GetComponent<SUnitScript>();
                if (health > 0)
                {
                    howManyStatsRelevant++;
                    if (!sUnitScript.breakStatLimiter && sUnitScript.health == 100)
                    {
                        noStatsIncreased += 1;
                    }
                    else
                    {
                        sUnitScript.StartCoroutine(sUnitScript.showStatUp(sUnitScript.healthRect));
                    }
                }
                if (speed > 0)
                {
                    howManyStatsRelevant++;
                    if (sUnitScript.speed == 30)
                    {
                        noStatsIncreased += 1;
                    }
                    else
                    {
                        sUnitScript.StartCoroutine(sUnitScript.showStatUp(sUnitScript.speedRect));
                    }
                }
                if (firerate > 0)
                {
                    howManyStatsRelevant++;
                    if (sUnitScript.firerate == 50)
                    {
                        noStatsIncreased += 1;
                    }
                    else
                    {
                        sUnitScript.StartCoroutine(sUnitScript.showStatUp(sUnitScript.firerateRect));
                    }
                }
                if (damage > 0)
                {
                    howManyStatsRelevant++;
                    if (!sUnitScript.breakStatLimiter && sUnitScript.damage == 100)
                    {
                        noStatsIncreased += 1;
                    }
                    else
                    {
                        sUnitScript.StartCoroutine(sUnitScript.showStatUp(sUnitScript.damageRect));
                    }
                }
                if (noStatsIncreased >= howManyStatsRelevant)
                {
                    return false;
                }
                sUnitScript.health += health;
                sUnitScript.speed += speed;
                sUnitScript.firerate += firerate;
                sUnitScript.damage += damage;
                sUnitScript.updateActualStats();
                sUnitScript.displayStatUpEffect();
                return true;
            }
        }
        return false;
    }

    static public void placeObj(GameObject placedObj)
    {
        Vector2 placedPos = new Vector2(cellIndicatorPos.x + 0.75f, cellIndicatorPos.y + 0.75f);
        Instantiate(placedObj, placedPos, Quaternion.identity);
    }

    static public void placeObjIntoParent(GameObject placedObj, Transform parent)
    {
        parent.name = "CreatedWaved";
        Vector2 placedPos = new Vector2(cellIndicatorPos.x + 0.75f, cellIndicatorPos.y + 0.75f);
        Instantiate(placedObj, placedPos, Quaternion.identity, parent);
    }
}
