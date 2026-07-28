using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPortalObject : InteractableObject
{
    [Header("던전 이동 포탈 정보")]
    public DungeonDataSO dungeonData;

    protected override void Start()
    {
        base.Start();
        Init($"G - 던전입장");
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;

        UIManager.Instance.dungentEnterUI.Toggle();
        UIManager.Instance.dungentEnterUI.UpdateDungeonEnterUI(dungeonData);

        return true;
    }
}
