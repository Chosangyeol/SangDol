using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WarpData
{
    public Transform targetPos;
    public bool isBossRoom = false;
    public Transform bossSpawnPos;
    public BossModel bossModel;
}

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    [Header("던전 내부 계산 변수들")]
    [SerializeField] private int dungeonStepIndex = 0;
    [SerializeField] private List<WarpData> warpDatas = new List<WarpData>();
    private CharacterModel _model;

    private void Start()
    {
        dungeonStepIndex = 0;

        _model = GameObject.FindObjectOfType<CharacterModel>();
    }



    public void WarpPlayer(int index)
    {
        _model.Navmesh.enabled = false;
    }

    private IEnumerator WarpSequence(int index)
    {
        _model.PlayerController.StopMove();
        _model.canMove = false;
        _model.Navmesh.enabled = false;

        yield return new WaitForSeconds(3f);

        _model.transform.position = warpDatas[index].targetPos.position;

        _model.Navmesh.enabled = true;
        _model.canMove = true;

        if (warpDatas[index].isBossRoom)
            Instantiate(warpDatas[index].bossModel, warpDatas[index].bossSpawnPos.position, Quaternion.identity);
    }





}
