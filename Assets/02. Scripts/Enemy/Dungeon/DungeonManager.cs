using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class WarpData
{
    public Transform targetPos;
    public bool isBossRoom = false;
    public Transform bossSpawnPos;
    public BossModel bossModel;
    public Transform playerRespawn;
    public bool hasVideo;
    public VideoClip clip;
    public bool hasPlayed;
}

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    [Header("던전 내부 계산 변수들")]
    [SerializeField] private int dungeonStepIndex = 0;
    [SerializeField] private List<WarpData> warpDatas = new List<WarpData>();
    private CharacterModel _model;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        dungeonStepIndex = 0;

        _model = GameObject.FindObjectOfType<CharacterModel>();
    }



    public void WarpPlayer(int index)
    {
        StartCoroutine(WarpSequence(index));
    }

    private IEnumerator WarpSequence(int index)
    {
        dungeonStepIndex = index;

        _model.PlayerController.StopMove();
        _model.canMove = false;
        _model.canAttack = false;
        _model.canUse = false;
        _model.Navmesh.enabled = false;

        GameEvent.OnBossRoomEnterCount?.Invoke(true, 0f);

        yield return new WaitForSeconds(2f);

        GameEvent.OnBossRoomEnterCount?.Invoke(false, 0f);

        yield return new WaitForSeconds(1f);

        if (warpDatas[index].hasVideo && !warpDatas[index].hasPlayed)
        {
            VideoPlayManager.instance.PlayVideo(warpDatas[index].clip);

            yield return new WaitUntil(() => !VideoPlayManager.instance.isPlaying);

            // 3. 다음번에 워프할 때는 다시 재생되지 않도록 true로 바꿔줍니다.
            warpDatas[index].hasPlayed = true;
        }

        _model.transform.position = warpDatas[index].targetPos.position;

        _model.cams[0].PreviousStateIsValid = false;

        _model.Navmesh.enabled = true;
        _model.canAttack = true;
        _model.canUse = true;
        _model.canMove = true;

        

        if (warpDatas[index].isBossRoom)
        {
            BossModel boss = Instantiate(warpDatas[index].bossModel, warpDatas[index].bossSpawnPos.position, Quaternion.Euler(0,180f,0f));
            boss.isActive = true;
        }
    }

    public void ReplacePlayer()
    {
        StartCoroutine(ReplaceSequence());
    }

    private IEnumerator ReplaceSequence()
    {
        yield return new WaitForSeconds(3f);

        _model.Navmesh.enabled = false;
        _model.canMove = false;
        _model.canUse = false;

        _model.transform.position = warpDatas[dungeonStepIndex].playerRespawn.position;

        _model.Revive();

        yield return new WaitForSeconds(3f);

        _model.canMove = true;
    }





}
