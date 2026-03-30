using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour
{
    [SerializeField] string questID = "";
    [SerializeField] TMP_Text tmpQuestNameText;
    [SerializeField] QuestPreview previewUI;
    [SerializeField] Toggle trackingToggle;


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

        trackingToggle.onValueChanged.RemoveAllListeners();

        if (QuestManager.Instance.questTrackDict.TryGetValue(questID, out bool isTracked))
            trackingToggle.isOn = isTracked;
        else
            trackingToggle.isOn = false;

        // 2. 유저가 체크박스를 클릭했을 때 Manager로 상태 전달
        trackingToggle.onValueChanged.AddListener((isOn) =>
        {
            QuestManager.Instance.SetQuestTracking(questID, isOn);
        });

        UpdateQuestSlot();
    }

    public void ToggleQuestPreview()
    {
        if (!previewUI.gameObject.activeSelf)
            previewUI.gameObject.SetActive(true);

        previewUI.SetQuestPreview(questID);
    }

    private void UpdateQuestSlot()
    {
        tmpQuestNameText.text = nowQuestData.questName;
    }
}
