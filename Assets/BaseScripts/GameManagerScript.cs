using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class GameManagerScript : MonoBehaviour
{
    public bool isTheTower = false;
    static public TowerEnemySpawner towerEnemySystem;
    static public bool playBossSong = false;

    static public float playerHealth = 5;
    static public int turnNumber = 0;
    static public int winAmount = 0;
    static public bool fightStarted = false;
    static public bool prefightStarted = false;
    static public bool prefightAllHealthObjSummoned = false;
    static public int currentPreFightNum = 0;
    static public List<SUnitScript> preFightObjects = new List<SUnitScript>();
    static public bool isOnTurn = true;

    static public int playerPalCount = 0; //pals with the team number 0 are playerPals
    static public int alliesPalCount = 0; //for future when adding CoOp (probably gonna use team number 999 or something)
    static public int mostAlliedPalCount = 0; //doesnt change with pal faints
    static public int enemyPalCount = 0;
    static public int summonedQue = 0;
    static public int enemySummonedQue = 0;

    static public int playerMoney = 12;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI winNumText;
    public TextMeshProUGUI turnNumText;
    //public GameObject moneyObj;

    static public GameObject returnToLvlSelect;
    public GameObject sUnitXPUp;
    static public bool playerWon = false;
    float timer;
    static public bool endingWave = false;

    static public Animator canvasAnim;

    static public void resetAll()
    {
        endingWave = false;
        summonedQue = 0;
        prefightAllHealthObjSummoned = false;
        mostAlliedPalCount = 0;
        playerPalCount = 0;
        enemyPalCount = 0;
        playBossSong = false;
        turnNumber = 0;
        winAmount = 0;
        fightStarted = false;
        prefightStarted = false;
        isOnTurn = false;
        playerMoney = 0;
        playerHealth = 1;
        currentPreFightNum = 0;
        playerWon = false;
    }

    static public void turnStarter()
    {
        preFightObjects.Clear();
        MainShop mainShop = GameObject.Find("LeftSideBuyArea").GetComponent<MainShop>();
        mainShop.fightEndReroll();
        summonedQue = 0;
        mostAlliedPalCount = 0;
        playerPalCount = 0;
        enemyPalCount = 0;
        turnNumber++;
        playerMoney = 10;
        fightStarted = false;
        prefightStarted = false;
        prefightAllHealthObjSummoned = false;
        isOnTurn = true;
        currentPreFightNum = 0;
        if (turnNumber > 1)
        {
            canvasAnim.Play("TurnStart");
        }
        if (towerEnemySystem != null)
        {
            TowerEnemySpawner.currentDifficulty = Mathf.CeilToInt(turnNumber / 6f);
            towerEnemySystem.spawnEnemies();
        }
        endingWave = false;
    }

    public void fightStarter() //starts the coroutine
    {
        canvasAnim.Play("FightStart");
        StartCoroutine(fightStart());
    }

    static public IEnumerator fightStart()
    {
        PalPlacementSystem.saveCurrentField();
        isOnTurn = false;
        yield return new WaitForSeconds(0.01f);
        prefightAllHealthObjSummoned = true;
        yield return new WaitForSeconds(0.01f);
        mostAlliedPalCount = playerPalCount;
        yield return new WaitForSeconds(0.01f);
        prefightStarted = true;
        yield return new WaitForSeconds(0.1f);
        while (currentPreFightNum < 5)
        {
            while (preFightObjects.Count > 0)
            {
                preFightObjects[0].callStartFightEffect();
                preFightObjects.RemoveAt(0);
                //Debug.Log(preFightObjects[0]);
                yield return new WaitForSeconds(0.16f);
            }
            currentPreFightNum++;
            yield return new WaitForSeconds(0.1f);
        }
        fightStarted = true;
    }

    static public void waveStarter()
    {
        playBossSong = false;
        playerHealth = 10;
    }
    // Start is called before the first frame update
    void Start()
    {
        resetAll();
        canvasAnim = GameObject.Find("Canvas").GetComponent<Animator>();
        TowerEnemySpawner.currentDifficulty = 1;
        if (isTheTower)
        {
            towerEnemySystem = GameObject.Find("TheTowerEnemySpawner").GetComponent<TowerEnemySpawner>();
        }
        turnStarter();
    }

    IEnumerator startEndFightProcess()
    {
        yield return new WaitForSeconds(0.8f);
        if (playerHealth > 0)
        {
            PalPlacementSystem.resetField();
            turnStarter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (isTheTower)
        //{
        //    TowerEnemySpawner.currentDifficulty = Mathf.CeilToInt(turnNumber / 6f);
        //    //Debug.Log("Current Difficulty: " + TowerEnemySpawner.currentDifficulty);
        //    //Debug.Log("Current Status: " + (turnNumber - ((TowerEnemySpawner.currentDifficulty - 1) * 6)));
        //}
        //timer += Time.deltaTime;
        if (moneyText != null)
        {
            moneyText.text = "" + playerMoney;
        }
        if (healthText != null)
        {
            healthText.text = "" + playerHealth;
        }
        if (winNumText != null)
        {
            winNumText.text = "" + winAmount;
        }
        if (turnNumText != null)
        {
            turnNumText.text = "" + turnNumber;
        }

        if (winAmount >= 12)
        {
            canvasAnim.Play("WinAnim");
        }

        if (playerHealth < 1)
        {
            canvasAnim.Play("LoseAnim");
        }
        if (fightStarted == true && playerHealth > 0)
        {
            Debug.Log("Player Pals: " + playerPalCount);
            Debug.Log("Enemy Pals: " + enemyPalCount);
            if (playerPalCount < 1 && summonedQue <= 0)
            {
                if (!endingWave)
                {
                    playerHealth -= 1;
                    endingWave = true;
                    StartCoroutine(startEndFightProcess());
                }
            }
            else if (enemyPalCount < 1 && enemySummonedQue <= 0)
            {
                if (!endingWave)
                {
                    winAmount += 1;
                    endingWave = true;
                    StartCoroutine(startEndFightProcess());
                }
            }
        }
    }
}

//git gud