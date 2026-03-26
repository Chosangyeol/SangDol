using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSlot : MonoBehaviour
{
    [SerializeField] string questID = "";
    [SerializeField] TMP_Text tmpQuestNameText;
    [SerializeField] QuestPreview previewUI;

    private QuestData nowQuestData;

    public void Init(QuestPreview preview)
    {
        questID = "";
        previewUI = preview;
    }

    public void SetQuestSlot(string questID)
    {
        this.questID = questID;
        nowQuestData = QuestManager.Instance.GetQuestData(questID);

        UpdateQuestSlot();
    }

    public void ToggleQuestPreview()
    {
        if (!previewUI.gameObject.activeSelf)
            previewUI.gameObject.SetActive(true);
    }

    private void UpdateQuestSlot()
    {
        tmpQuestNameText.text = nowQuestData.questName;
    }
}
