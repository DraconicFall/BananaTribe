using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerEnemySpawner : MonoBehaviour
{
    //25 floors, 5 difficulties
    public Material enemyMaterial;
    public DifficultyFloors difficulty1Floor;
    public DifficultyFloors difficulty2Floor;
    public DifficultyFloors difficulty3Floor;
    public DifficultyFloors difficulty4Floor;
    public DifficultyFloors difficulty5Floor;

    static public int currentDifficulty = 0;
    private int currentFloor = 1;
    // Update is called once per frame

    public void spawnEnemies()
    {
        int random = 0;
        if (currentDifficulty <= 1)
        {
            assignFloor(difficulty1Floor);
        }
        else if (currentDifficulty <= 2)
        {
            assignFloor(difficulty2Floor);
        }
        else if (currentDifficulty <= 3)
        {
            assignFloor(difficulty3Floor);
        }
        else if (currentDifficulty <= 4)
        {
            assignFloor(difficulty4Floor);
        }
        else if (currentDifficulty <= 5)
        {
            assignFloor(difficulty5Floor);
        }
    }

    private void assignFloor(DifficultyFloors dFloors)
    {
        int random = 0;
        if (GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 1)
        {
            random = Random.Range(0, dFloors.floor1Waves.Length);
            waveUnpacker(dFloors.floor1Waves[random]);
        }
        else if (GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 2)
        {
            random = Random.Range(0, dFloors.floor2Waves.Length);
            waveUnpacker(dFloors.floor2Waves[random]);
        }
        else if (GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 3 || GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 4)
        {
            random = Random.Range(0, dFloors.floor3And4Waves.Length);
            waveUnpacker(dFloors.floor3And4Waves[random]);
        }
        else if (GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 5)
        {
            random = Random.Range(0, dFloors.floor5Waves.Length);
            waveUnpacker(dFloors.floor5Waves[random]);
        }
        else if (GameManagerScript.turnNumber - ((currentDifficulty - 1) * 6) == 6)
        {
            random = Random.Range(0, dFloors.BossWaves.Length);
            waveUnpacker(dFloors.BossWaves[random]);
        }
    }

    private void waveUnpacker(GameObject wave)
    {
        GameObject thisWave = Instantiate(wave);
        foreach (Transform transform in thisWave.transform)
        {
            SUnitScript sUnit = transform.GetComponent<SUnitScript>();
            if (sUnit != null)
            {
                sUnit.spriteRenderer.material = enemyMaterial;
            }
        }
        thisWave.transform.DetachChildren();
        Destroy(thisWave);
    }
}
