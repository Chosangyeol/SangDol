using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBase : InteractableObject
{
    [SerializeField] private NpcSO npcSO;

    protected override void Start()
    {
        base.Start();

        if (npcSO != null)
            Init($"G - {npcSO.npcName}과 대화");
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;

        if (npcSO == null) return false;

        GameEvent.OnTalkNpc?.Invoke(npcSO.npcID);

        NpcDialogManager.Instance.OpenNpcUI(npcSO);
        return true;
    }
}
