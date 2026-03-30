using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine.UIElements;
using System;

[System.Serializable]
public struct RewardItem
{
    public string itemID;
    public int count;
}

[System.Serializable]
public class QuestData
{
    public string questID;
    public string questName;
    public string questType;
    public string questTarget;
    public int questCount;

    public int rewardGold;
    public int rewardExp;
    public List<RewardItem> rewardItems = new List<RewardItem>();

    public string questDialog;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public Dictionary<string, QuestData> questDict = new Dictionary<string, QuestData>();
    public Dictionary<string, QuestState> questStateDict = new Dictionary<string, QuestState>();

    public Dictionary<string, int> questKillProgressDict = new Dictionary<string, int>();
    public Dictionary<string, int> questItemProgressDict = new Dictionary<string, int>();
    public Dictionary<string, bool> questTalkProgressDict = new Dictionary<string, bool>();

    public Dictionary<string, bool> questTrackDict = new Dictionary<string, bool>(); // 추적(체크) 여부 저장
    public event Action OnQuestProgressUpdated;

    private CharacterModel _model;

    [Header("퀘스트 미리보기")]
    public GameObject questPreview;
    public TMP_Text questNameText;
    public TMP_Text questDialogText;

    

    private void Awake()
    {
        if (Instance == null ) Instance = this;

        LoadQuestCsv("NpcQuestDataBase");
    }

    private void Start()
    {
        _model = FindAnyObjectByType<CharacterModel>();
    }

    private void OnEnable()
    {
        GameEvent.OnMonsterKill += HandleCountMonsterKill;
        GameEvent.OnGetItem += HandleCountItem;
        GameEvent.OnTalkNpc += HandleTalkNpc;
    }

    private void OnDisable()
    {
        GameEvent.OnMonsterKill -= HandleCountMonsterKill;
        GameEvent.OnGetItem -= HandleCountItem;
        GameEvent.OnTalkNpc -= HandleTalkNpc;
    }

    private void LoadQuestCsv(string fileName)
    {
        questDict.Clear();
        TextAsset csvData = Resources.Load<TextAsset>(fileName);

        if (csvData == null)
        {
            Debug.LogError($"[QuestManager] {fileName}.csv 파일을 Resources 폴더에서 찾을 수 없습니다!");
            return;
        }

        string[] rows = csvData.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrEmpty(rows[i])) continue;

            string[] columns = SplitCSVLine(rows[i]);

            QuestData data = new QuestData();

            data.questID = columns[0];
            data.questName = columns[1];
            data.questDialog = columns[2];

            if (columns.Length > 3) data.questType = columns[3];
            if (columns.Length > 4) data.questTarget = columns[4];
            if (columns.Length > 5) int.TryParse(columns[5], out data.questCount);
            if (columns.Length > 6) int.TryParse(columns[6], out data.rewardGold);
            if (columns.Length > 7) int.TryParse(columns[7], out data.rewardExp);

            // 보상 1 (Index 8, 9)
            if (columns.Length > 9 && !string.IsNullOrWhiteSpace(columns[8]))
            {
                int.TryParse(columns[9], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[8], count = count });
            }
            // 보상 2 (Index 10, 11)
            if (columns.Length > 11 && !string.IsNullOrWhiteSpace(columns[10]))
            {
                int.TryParse(columns[11], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[10], count = count });
            }
            // 보상 3 (Index 12, 13)
            if (columns.Length > 13 && !string.IsNullOrWhiteSpace(columns[12]))
            {
                int.TryParse(columns[13], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[12], count = count });
            }

            

            questDict.Add(data.questID, data);
        }
        Debug.Log($"[QuestManager] 퀘스트 로드 완료: {questDict.Count}개");
    }

    private string[] SplitCSVLine(string line)
    {
        // 큰따옴표 안에 있는 쉼표는 건너뛰고 분리하는 정규표현식
        string[] columns = Regex.Split(line, @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");

        for (int i = 0; i < columns.Length; i++)
        {
            // 엑셀이 자동으로 붙인 양끝의 큰따옴표 제거 및 내부 따옴표 복구
            columns[i] = columns[i].TrimStart('"').TrimEnd('"').Replace("\"\"", "\"");
        }

        return columns;
    }

    private void HandleCountMonsterKill(string targetMonsterID)
    {
        List<string> activeQuests = new List<string>(questStateDict.Keys);

        foreach (var questID in activeQuests)
        {
            if (questStateDict[questID] == QuestState.InProgress)
            {
                QuestData data = questDict[questID];

                if (data.questType == "Kill" && data.questTarget == targetMonsterID)
                {
                    if (!questKillProgressDict.ContainsKey(questID))
                        questKillProgressDict[questID] = 0;

                    questKillProgressDict[questID]++;

                    Debug.Log($"[퀘스트 진행] {data.questName} : {questKillProgressDict[questID]} / {data.questCount}");

                    if (questKillProgressDict[questID] >= data.questCount)
                    {
                        questStateDict[questID] = QuestState.CanClear;
                        Debug.Log($"<color=cyan>[퀘스트 조건 달성] NPC에게 돌아가 보상을 받으세요!</color>");
                    }
                }
            }
        }

        OnQuestProgressUpdated?.Invoke();
    }

    private void HandleCountItem(string targetItemID)
    {
        List<string> activeQuests = new List<string>(questStateDict.Keys);

        foreach (var questID in activeQuests)
        {
            QuestData data = questDict[questID];

            if (data.questType == "Item" && data.questTarget == targetItemID)
            {
                int currentItemCount = _model.Inventory.GetTotalItemCount(data.questTarget);
                Debug.Log(currentItemCount);
                questItemProgressDict[questID] = currentItemCount;

                if (questStateDict[questID] == QuestState.InProgress)
                {
                    if (questItemProgressDict[questID] >= data.questCount)
                    {
                        questStateDict[questID] = QuestState.CanClear;
                        Debug.Log($"<color=cyan>[아이템 수집 완료] NPC에게 돌아가세요!</color>");
                    }
                }
                else if (questStateDict[questID] == QuestState.CanClear)
                {
                    if (questItemProgressDict[questID] < data.questCount)
                    {
                        questStateDict[questID] = QuestState.InProgress;
                        Debug.Log($"<color=red>퀘스트 아이템이 부족해졌습니다.</color>");
                    }
                }
            }
        }
        OnQuestProgressUpdated?.Invoke();
    }

    private void HandleTalkNpc(string targetNpcID)
    {
        List<string> activeQuests = new List<string>(questStateDict.Keys);

        foreach (var questID in activeQuests)
        {
            if (questStateDict[questID] == QuestState.InProgress)
            {
                QuestData data = questDict[questID];

                if (data.questType == "Talk" && data.questTarget == targetNpcID)
                {
                    questTalkProgressDict[questID] = true;
                    questStateDict[questID] = QuestState.CanClear;

                    Debug.Log($"<color=cyan>[대화 완료] 대화 퀘스트 조건을 달성했습니다!</color>");
                }
            }
        }
        OnQuestProgressUpdated?.Invoke();
    }

    public QuestState GetQuestState(string questID)
    {
        questID = questID.Trim();

        if (questStateDict.TryGetValue(questID, out QuestState state)) return state;
        return QuestState.NotStart;
    }

    public string GetQuestName(string questID)
    {
        questID = questID.Trim();

        if (questDict.TryGetValue(questID, out QuestData data)) return data.questName;
        return "알 수 없는 퀘스트";
    }

    public void AcceptQuest(string questID)
    {
        questID = questID.Trim();

        if (GetQuestState(questID) == QuestState.NotStart)
        {
            QuestData data = questDict[questID];

            questStateDict[questID] = QuestState.InProgress;

            if (questDict[questID].questType == "Kill")
                questKillProgressDict.Add(questID, 0);
            else if (questDict[questID].questType == "Item")
            {
                int currentItemCount = _model.Inventory.GetTotalItemCount(data.questTarget);
                Debug.Log(currentItemCount);


                questItemProgressDict.Add(questID, 0);

                HandleCountItem(data.questTarget); 
            }
            else if (questDict[questID].questType == "Talk")
            {
                questTalkProgressDict.Add(questID, false);
                Debug.Log(questTalkProgressDict[questID]);
            }

            questTrackDict[questID] = true;

            questPreview.SetActive(false);
            OnQuestProgressUpdated?.Invoke();
        }
    }

    public void RefuseQuest()
    {
        questPreview.SetActive(false);
    }

    public void CompleteQuest(string questID)
    {
        questID = questID.Trim();

        Debug.Log(questDict[questID].questName);

        if (GetQuestState(questID) == QuestState.CanClear)
        {
            questStateDict[questID] = QuestState.Completed;

            if (questDict.TryGetValue(questID, out QuestData data))
            {
                if (data.rewardExp > 0)
                {
                    Debug.Log($"경험치 {data.rewardExp} 획득");
                    _model.GainExp(data.rewardExp);
                }

                if (data.rewardGold > 0)
                {
                    Debug.Log($"골드 {data.rewardGold} 획득");
                    _model.GainGold(data.rewardGold);
                }

                foreach (var reward in data.rewardItems)
                {
                    Debug.Log($"아이템 획득: {reward.itemID} x {reward.count}");
                    // 실제 연동 시: _model.Inventory.AddItem(reward.itemID, reward.count);
                }
            }
        }
        else
        {
            Debug.LogWarning("아직 퀘스트 완료 조건을 달성하지 못했습니다.");
        }
        OnQuestProgressUpdated?.Invoke();
    }

    public QuestData GetQuestData(string questID)
    {
        if (questDict.TryGetValue(questID, out QuestData data)) return data;
        return null;
    }

    public void ShowQuestPreview(string questID)
    {
        questPreview.SetActive(true);
        questNameText.text = GetQuestName(questID);
        questDialogText.text = questDict[questID].questDialog;
    }

    public void SetQuestTracking(string questID, bool isTracked)
    {
        questID = questID.Trim();
        questTrackDict[questID] = isTracked;
        OnQuestProgressUpdated?.Invoke(); // UI 갱신 신호 발송
    }
}
