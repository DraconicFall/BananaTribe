using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    public static bool ifBossHealthBarActive = false;
    public GameObject BossHealthBars;

    private void Awake()
    {
        BossHealthBars = GameObject.Find("BossBar");
        BossHealthBars.SetActive(false);
        disableBossBar();
    }
    private void Start()
    {
        BossHealthBars.SetActive(false);
        disableBossBar();
    }
    // Start is called before the first frame update
    //private void FixedUpdate()
    //{
    //    BossHealthBars.SetActive(false);
    //}
    public void enableBossBar()
    {
        ifBossHealthBarActive = true;
        BossHealthBars.SetActive(true);
    }
    public void disableBossBar()
    {
        ifBossHealthBarActive = false;
        BossHealthBars.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if(WaveSpawner.typeOfStep == Map.NodeType.Boss)
        //{
        //    BossHealthBars.SetActive(true);
        //    ifBossHealthBarActive = true;
        //}
    }
}
