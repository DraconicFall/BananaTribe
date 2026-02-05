using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerWaveCreator : MonoBehaviour
{
    [Header("SPACE MUST BE USED TO PLACE ENEMIES, REMEMBER TO PAUSE WHEN SAVING WAVE")]
    public GameObject[] enemyList;
    public int currentEnemySelected = 0;
    private bool isCreatingWave = false;
    GameObject createdWave;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //isCreatingWave = true;
            createdWave = new GameObject();
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                PalPlacementSystem.placeObjIntoParent(enemyList[currentEnemySelected], createdWave.transform);
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                currentEnemySelected++;
                if (currentEnemySelected >= enemyList.Length)
                {
                    currentEnemySelected = 0;
                }
            }
        }
    }
}
