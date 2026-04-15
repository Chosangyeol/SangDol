using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [Header("트래커 UI 세팅")]
    public Transform trackerParent;
    public QuestTrackerSlot trackerSlotPrefab;

    private List<QuestTrackerSlot> activeSlots = new List<QuestTrackerSlot>();

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestProgressUpdated -= UpdateTracker;
    }

    public void Init()
    {
        QuestManager.Instance.OnQuestProgressUpdated += UpdateTracker;
        UpdateTracker();
    }

    public void UpdateTracker()
    {
        // 1. 기존 슬롯 초기화
        foreach (var slot in activeSlots)
        {
            Destroy(slot.gameObject);
        }
        activeSlots.Clear();

        // 2. 현재 진행 중이거나 완료 가능한 퀘스트 순회
        foreach (var kvp in QuestManager.Instance.questStateDict)
        {
            string questID = kvp.Key;
            QuestState state = kvp.Value;

            if (state == QuestState.InProgress || state == QuestState.CanClear)
            {
                // 3. 딕셔너리에 추적(체크)이 true로 되어 있는지 확인
                if (QuestManager.Instance.questTrackDict.TryGetValue(questID, out bool isTracked) && isTracked)
                {
                    // 트래커 슬롯 생성 및 데이터 전달
                    QuestTrackerSlot newSlot = Instantiate(trackerSlotPrefab, trackerParent);
                    newSlot.Init(questID);
                    activeSlots.Add(newSlot);
                }
            }
        }
    }


}
