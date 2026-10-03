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
    public string questDialog;
    public string questType;
    public string questTarget;
    public string questTargetName;
    public int questCount;

    public string clearNpcID;

    public int rewardGold;
    public int rewardExp;
    public List<RewardItem> rewardItems = new List<RewardItem>();

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
    private C_Inventory observedInventory;
    private readonly HashSet<string> completingQuests = new HashSet<string>();
    [SerializeField] ItemTooltip tooltip;

    [Header("퀘스트 미리보기")]
    public GameObject questPreview;
    public TMP_Text questNameText;
    public TMP_Text questDialogText;
    public TMP_Text questGoldReward;
    public TMP_Text questExpReward;
    public RewardItemSlot[] questRewardItemSlots;
    public TMP_Text questItemReward1;
    public TMP_Text questItemReward2;
    public TMP_Text questItemReward3;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadQuestCsv("NpcQuestDataBase");
    }

    private void Start()
    {
        _model = FindAnyObjectByType<CharacterModel>();
        BindInventory();
        RefreshItemQuests();
    }

    private void OnEnable()
    {
        GameEvent.OnMonsterKill += HandleCountMonsterKill;
        GameEvent.OnGetItem += HandleCountItem;
        GameEvent.OnTalkNpc += HandleTalkNpc;
        BindInventory();
        RefreshItemQuests();
    }

    private void OnDisable()
    {
        GameEvent.OnMonsterKill -= HandleCountMonsterKill;
        GameEvent.OnGetItem -= HandleCountItem;
        GameEvent.OnTalkNpc -= HandleTalkNpc;
        if (observedInventory != null) observedInventory.OnInventoryUpdated -= OnInventoryChanged;
        observedInventory = null;
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
            data.clearNpcID = columns[3];

            if (columns.Length > 4) data.questType = columns[4];
            if (columns.Length > 5) data.questTarget = columns[5];
            if (columns.Length > 6) int.TryParse(columns[6], out data.questCount);
            if (columns.Length > 7) data.questTargetName = columns[7];
            if (columns.Length > 8) int.TryParse(columns[8], out data.rewardGold);
            if (columns.Length > 9) int.TryParse(columns[9], out data.rewardExp);

            // 보상 1 (Index 8, 9)
            if (columns.Length > 11 && !string.IsNullOrWhiteSpace(columns[10]))
            {
                int.TryParse(columns[11], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[10], count = count });
            }
            // 보상 2 (Index 10, 11)
            if (columns.Length > 13 && !string.IsNullOrWhiteSpace(columns[12]))
            {
                int.TryParse(columns[13], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[12], count = count });
            }
            // 보상 3 (Index 12, 13)
            if (columns.Length > 15 && !string.IsNullOrWhiteSpace(columns[14]))
            {
                int.TryParse(columns[15], out int count);
                data.rewardItems.Add(new RewardItem { itemID = columns[14], count = count });
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

    private void HandleCountItem(string targetItemID) => RefreshItemQuests(targetItemID);

    private void BindInventory()
    {
        if (observedInventory != null) observedInventory.OnInventoryUpdated -= OnInventoryChanged;
        observedInventory = _model != null ? _model.Inventory : null;
        if (observedInventory != null) observedInventory.OnInventoryUpdated += OnInventoryChanged;
    }
    private void OnInventoryChanged() => RefreshItemQuests();

    private void RefreshItemQuests(string targetItemID = null)
    {
        if (_model == null || _model.Inventory == null) return;
        foreach (var id in new List<string>(questStateDict.Keys))
        {
            var state = questStateDict[id];
            if (state != QuestState.InProgress && state != QuestState.CanClear) continue;
            if (!questDict.TryGetValue(id, out var data) || data == null || data.questType != "Item" ||
                string.IsNullOrEmpty(data.questTarget) ||
                (targetItemID != null && data.questTarget != targetItemID)) continue;
            int count = _model.Inventory.GetTotalItemCount(data.questTarget);
            questItemProgressDict[id] = count;
            questStateDict[id] = count >= data.questCount ? QuestState.CanClear : QuestState.InProgress;
        }
        OnQuestProgressUpdated?.Invoke();
    }

    private void OnDestroy()
    {
        if (observedInventory != null) observedInventory.OnInventoryUpdated -= OnInventoryChanged;
        if (Instance == this) Instance = null;
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
        questPreview?.SetActive(false);
    }

    public void CompleteQuest(string questID)
    {
        TryCompleteQuest(questID, out _);
    }

    internal bool TryCompleteQuest(string questID, out string failureMessage)
    {
        failureMessage = "퀘스트 보상을 받을 수 없습니다. 잠시 후 다시 시도해주세요.";
        if (string.IsNullOrWhiteSpace(questID)) return false;
        questID = questID.Trim();
        if (!questDict.TryGetValue(questID, out var data) || data == null ||
            !questStateDict.TryGetValue(questID, out var state)) return false;
        if (state == QuestState.Completed)
        {
            failureMessage = null;
            return true;
        }
        if (state != QuestState.CanClear)
        {
            failureMessage = "아직 퀘스트 완료 조건을 충족하지 못했습니다.";
            return false;
        }
        if (_model == null || _model.Inventory == null || _model.Stat == null || !completingQuests.Add(questID)) return false;
        try
        {
            if (data.questType == "Item")
            {
                RefreshItemQuests(data.questTarget);
                if (GetQuestState(questID) != QuestState.CanClear)
                {
                    failureMessage = "퀘스트에 필요한 아이템이 부족합니다.";
                    return false;
                }
            }
            if (data.rewardGold < 0 || data.rewardExp < 0 ||
                (long)_model.Stat.Stat.gold + data.rewardGold > int.MaxValue) return false;
            var rewards = new List<KeyValuePair<ItemBaseSO, int>>();
            if (data.rewardItems != null)
            {
                foreach (var reward in data.rewardItems)
                {
                    if (string.IsNullOrWhiteSpace(reward.itemID) || reward.count <= 0 || ItemManager.Instance == null) return false;
                    var item = ItemManager.Instance.GetItemBaseSO(reward.itemID.Trim());
                    if (item == null) return false;
                    rewards.Add(new KeyValuePair<ItemBaseSO, int>(item, reward.count));
                }
            }
            bool granted = _model.Inventory.TryAddRewards(rewards, () =>
            {
                // Final inventory and completion state are visible to reentrant subscribers.
                questStateDict[questID] = QuestState.Completed;
                if (data.rewardGold > 0) _model.GainGold(data.rewardGold);
                if (data.rewardExp > 0) _model.GainExp(data.rewardExp);
            });
            if (!granted)
            {
                Debug.LogWarning("[QuestManager] Not enough inventory space for all quest rewards.");
                failureMessage = "보상을 받을 인벤토리 공간이 부족합니다. 공간을 확보한 뒤 다시 시도해주세요.";
                return false;
            }
            OnQuestProgressUpdated?.Invoke();
            failureMessage = null;
            return true;
        }
        finally { completingQuests.Remove(questID); }
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
        questGoldReward.text = $"{questDict[questID].rewardGold}G";
        questExpReward.text = $"{questDict[questID].rewardExp}Exp";

        questRewardItemSlots[0].gameObject.SetActive(false);
        questRewardItemSlots[1].gameObject.SetActive(false);
        questRewardItemSlots[2].gameObject.SetActive(false);
        questItemReward1.gameObject.SetActive(false);
        questItemReward2.gameObject.SetActive(false);
        questItemReward3.gameObject.SetActive(false);

        if (questDict[questID].rewardItems.Count != 0)
        {
            for (int i = 0; i < questDict[questID].rewardItems.Count; i++)
            {
                ItemBaseSO rewardItem = ItemManager.Instance.GetItemBaseSO(questDict[questID].rewardItems[i].itemID);
                switch (i)
                {
                    case 0:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        questItemReward1.gameObject.SetActive(true);
                        questItemReward1.text = $"{questDict[questID].rewardItems[i].count}개";
                        break;
                    case 1:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        questItemReward2.gameObject.SetActive(true);
                        questItemReward2.text = $"{questDict[questID].rewardItems[i].count}개";
                        break;
                    case 2:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        questItemReward3.gameObject.SetActive(true);
                        questItemReward3.text = $"{questDict[questID].rewardItems[i].count}개";
                        break;
                }
            }
        }

    }

    public void SetQuestTracking(string questID, bool isTracked)
    {
        questID = questID.Trim();
        questTrackDict[questID] = isTracked;
        OnQuestProgressUpdated?.Invoke(); // UI 갱신 신호 발송
    }
}
