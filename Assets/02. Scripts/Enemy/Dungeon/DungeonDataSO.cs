using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DungeonDataSO", menuName = "SO/Dungeon/DungeonDataSO", order = 1)]
public class DungeonDataSO : ScriptableObject
{
    [Header("던전 정보")]
    public string dungeonName;
    public string dungeonSceneName;
    public string dungeonDescription;

    public Sprite dungeonBackGroundImage;
    public int dungeonRewardGold;
    public string[] dungeonRewardItemId;
}
