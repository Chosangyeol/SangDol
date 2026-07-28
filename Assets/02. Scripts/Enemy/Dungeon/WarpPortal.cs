using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPortal : MonoBehaviour
{
    [Header("워프 설정")]
    [SerializeField] private int warpPortalID;
    [SerializeField] private bool isCountdown;
    [SerializeField] private float maxCount;

    private float nowCount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterModel model = other.GetComponent<CharacterModel>();

            if (model != null )
            {
                isCountdown = true;
                nowCount = maxCount;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterModel model = other.GetComponent<CharacterModel>();

            if (model != null && isCountdown)
            {
                nowCount -= Time.deltaTime;
                GameEvent.OnBossRoomEnterCount?.Invoke(true,nowCount);

                if (nowCount < 0 && DungeonManager.instance != null)
                {
                    DungeonManager.instance.WarpPlayer(warpPortalID);
                    isCountdown = false;
                    nowCount = 0;
                }
                    
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvent.OnBossRoomEnterCount?.Invoke(false, nowCount);
            isCountdown = false;
            nowCount = maxCount;
        }
    }

}
