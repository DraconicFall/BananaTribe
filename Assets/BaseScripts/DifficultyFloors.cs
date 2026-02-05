using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Difficulty Floors")]
public class DifficultyFloors : ScriptableObject
{
    public GameObject[] floor1Waves;

    public GameObject[] floor2Waves;
    public GameObject[] floor3And4Waves;
    public GameObject[] floor5Waves;

    public GameObject[] BossWaves;
}
