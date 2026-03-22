using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

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
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public Dictionary<string, QuestData> questDict = new Dictionary<string, QuestData>();
    public Dictionary<string, QuestState> questStateDict = new Dictionary<string, QuestState>();

    private void Awake()
    {
        if (Instance == null ) Instance = this;

        LoadQuestCsv("NpcQuestDataBase");
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

            if (columns.Length > 2) data.questType = columns[2];
            if (columns.Length > 3) data.questTarget = columns[3];
            if (columns.Length > 4) int.TryParse(columns[4], out data.questCount);
            if (columns.Length > 5) int.TryParse(columns[5], out data.rewardGold);
            if (columns.Length > 6) int.TryParse(columns[6], out data.rewardExp);

            if (columns.Length > 8 && !string.IsNullOrWhiteSpace(columns[7]))
            {
                int.TryParse(columns[8], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[7], count = count });
            }
            // 보상 2 (Index 9, 10)
            if (columns.Length > 10 && !string.IsNullOrWhiteSpace(columns[9]))
            {
                int.TryParse(columns[10], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[9], count = count });
            }
            // 보상 3 (Index 11, 12)
            if (columns.Length > 12 && !string.IsNullOrWhiteSpace(columns[11]))
            {
                int.TryParse(columns[12], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[11], count = count });
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

    public QuestState GetQuestState(string questID)
    {
        if (questStateDict.TryGetValue(questID, out QuestState state)) return state;
        return QuestState.NotStart;
    }

    public string GetQuestName(string questID)
    {
        if (questDict.TryGetValue(questID, out QuestData data)) return data.questName;
        return "알 수 없는 퀘스트";
    }

    public void AcceptQuest(string questID)
    {
        if (GetQuestState(questID)==QuestState.NotStart)
        {
            questStateDict[questID] = QuestState.InProgress;


        }
    }

    public void CompleteQuest(string questID)
    {
        if (GetQuestState(questID) == QuestState.CanClear)
        {
            questStateDict[questID] = QuestState.Completed;

            if (questDict.TryGetValue(questID, out QuestData data))
            {
                if (data.rewardExp > 0) Debug.Log($"경험치 {data.rewardExp} 획득");
                if (data.rewardGold > 0) Debug.Log($"골드 {data.rewardGold} 획득");

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
    }
}
