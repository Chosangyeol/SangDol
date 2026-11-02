using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NpcBase : InteractableObject
{
    [SerializeField] private NpcSO npcSO;
    [SerializeField] private TMP_Text nameTag;
    private QuestManager subscribedQuestManager;

    protected override void Start()
    {
        base.Start();

        if (npcSO != null)
        {
            Init($"G - {npcSO.npcName}과 대화");
            nameTag.text = npcSO.npcName;
        }

        UpdateQuestMarker();
    }

    private void OnEnable()
    {
        SubscribeQuestUpdates();
    }

    private void OnDisable()
    {
        if (subscribedQuestManager != null)
        {
            subscribedQuestManager.OnQuestProgressUpdated -= UpdateQuestMarker;
            subscribedQuestManager = null;
        }
    }

    private void SubscribeQuestUpdates()
    {
        QuestManager questManager = QuestManager.Instance;
        if (questManager == null || subscribedQuestManager == questManager) return;

        if (subscribedQuestManager != null)
            subscribedQuestManager.OnQuestProgressUpdated -= UpdateQuestMarker;

        subscribedQuestManager = questManager;
        subscribedQuestManager.OnQuestProgressUpdated += UpdateQuestMarker;
    }

    private void UpdateQuestMarker()
    {
        SubscribeQuestUpdates();
        if (npcSO == null || nameTag == null || QuestManager.Instance == null) return;

        string marker = null;
        if (npcSO.npcQuests != null)
        {
            foreach (NpcQuestData quest in npcSO.npcQuests)
            {
                if (quest == null) continue;

                QuestState state = QuestManager.Instance.GetQuestState(quest.questID);
                QuestData questData = QuestManager.Instance.GetQuestData(quest.questID);
                if (state == QuestState.CanClear && questData != null && questData.clearNpcID == npcSO.npcID)
                {
                    marker = "<size=150%><color=#FF8A45>!</color></size>";
                    break;
                }

                if (marker == null && !string.IsNullOrWhiteSpace(quest.startDialogID) &&
                    QuestManager.Instance.CanAcceptQuest(quest.questID))
                {
                    marker = "<size=150%><color=#FFD54F>?</color></size>";
                }
            }
        }

        nameTag.text = marker == null ? npcSO.npcName : $"{marker}\n{npcSO.npcName}";
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

        CharacterModel model = target.GetComponent<CharacterModel>();

        if (model.isInteracting) return false;

        AudioManager.instance.PlaySFX(npcSO.npcIncounterSound);

        GameEvent.OnTalkNpc?.Invoke(npcSO.npcID);

        model.isInteracting = true;
        NpcDialogManager.Instance.OpenNpcUI(npcSO);

        return true;
    }
}
