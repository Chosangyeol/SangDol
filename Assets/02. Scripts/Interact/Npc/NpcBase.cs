using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NpcBase : InteractableObject
{
    [SerializeField] private NpcSO npcSO;
    [SerializeField] private TMP_Text nameTag;

    protected override void Start()
    {
        base.Start();

        if (npcSO != null)
        {
            Init($"G - {npcSO.npcName}과 대화");
            nameTag.text = npcSO.npcName;
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        nameTag.gameObject.transform.forward = _mainCam.transform.forward;
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
