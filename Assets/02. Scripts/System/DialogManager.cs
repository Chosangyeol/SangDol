using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[System.Serializable]
public class DialogueData
{
    public string dialogID;      // Index 0
    public string npcID;         // Index 1
    public string npcName;       // Index 2
    public string textKR;        // Index 3

    public string nextDialogID;  // Index 4

    // 선택지 데이터 (Index 5 ~ 8)
    public string choice1Text;
    public string choice1Next;
    public string choice2Text;
    public string choice2Next;

    public string actionType;    // Index 9
    public string actionValue;   // Index 10

    // 선택지가 존재하는지 확인하는 프로퍼티
    public bool HasChoices => !string.IsNullOrWhiteSpace(choice1Text);
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("대화 UI 연결")]
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;
    public Button nextButton;           // 일반 진행(클릭) 버튼

    [Header("선택지 UI 연결")]
    public GameObject choicePanel;      // 선택지 버튼들의 부모 패널
    public Button choice1Button;
    public TMP_Text choice1TextUI;
    public Button choice2Button;
    public TMP_Text choice2TextUI;

    public Dictionary<string, DialogueData> dialogueDict = new Dictionary<string, DialogueData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        LoadDialogueCSV("Dialogue_Data"); // Assets/Resources/Dialogue_Data.csv 로드
    }

    // 1. CSV 로드 및 파싱 기능
    private void LoadDialogueCSV(string fileName)
    {
        dialogueDict.Clear();
        TextAsset csvData = Resources.Load<TextAsset>(fileName);
        if (csvData == null) { Debug.LogError($"[DialogueManager] {fileName} 로드 실패!"); return; }

        string[] rows = csvData.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            string[] columns = SplitCSVLine(rows[i]);

            DialogueData data = new DialogueData();
            data.dialogID = columns[0];
            data.npcID = columns[1];
            data.npcName = columns[2];
            data.textKR = columns[3];

            if (columns.Length > 4) data.nextDialogID = columns[4];

            if (columns.Length > 5) data.choice1Text = columns[5];
            if (columns.Length > 6) data.choice1Next = columns[6];
            if (columns.Length > 7) data.choice2Text = columns[7];
            if (columns.Length > 8) data.choice2Next = columns[8];

            if (columns.Length > 9) data.actionType = columns[9];
            if (columns.Length > 10) data.actionValue = columns[10];

            // ID 중복 방지 처리
            if (!dialogueDict.ContainsKey(data.dialogID))
            {
                dialogueDict.Add(data.dialogID, data);
            }
        }
        Debug.Log($"[DialogueManager] 대사 로드 완료: {dialogueDict.Count}개");
    }

    // 쉼표 예외 처리 파싱 헬퍼 함수
    private string[] SplitCSVLine(string line)
    {
        string[] columns = Regex.Split(line, @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = columns[i].TrimStart('"').TrimEnd('"').Replace("\"\"", "\"").Trim();
        }
        return columns;
    }

    // 2. 대화 시작 및 UI 재생 기능
    public void StartDialogue(string dialogID)
    {
        dialoguePanel.SetActive(true);
        PlayDialogue(dialogID);
    }

    private void PlayDialogue(string id)
    {
        // 빈 값이거나 END면 대화 종료
        if (string.IsNullOrWhiteSpace(id) || id == "END")
        {
            EndDialogue();
            return;
        }

        if (dialogueDict.TryGetValue(id, out DialogueData data))
        {
            // 텍스트 갱신
            npcNameText.text = data.npcName;
            dialogueText.text = data.textKR;

            // 💡 액션(Action) 처리: 데이터에 액션이 있으면 즉시 실행
            if (!string.IsNullOrWhiteSpace(data.actionType))
            {
                ExecuteAction(data.actionType, data.actionValue);
            }

            // 이전 버튼 이벤트 초기화
            nextButton.onClick.RemoveAllListeners();
            choice1Button.onClick.RemoveAllListeners();
            choice2Button.onClick.RemoveAllListeners();

            // 💡 선택지 분기 처리
            if (data.HasChoices)
            {
                nextButton.gameObject.SetActive(false); // 화면 전체 클릭 버튼 끄기
                choicePanel.SetActive(true);            // 선택지 패널 켜기

                // 1번 선택지 세팅
                choice1TextUI.text = data.choice1Text;
                choice1Button.onClick.AddListener(() => PlayDialogue(data.choice1Next));

                // 2번 선택지 세팅 (데이터가 있을 때만 켜기)
                if (!string.IsNullOrWhiteSpace(data.choice2Text))
                {
                    choice2Button.gameObject.SetActive(true);
                    choice2TextUI.text = data.choice2Text;
                    choice2Button.onClick.AddListener(() => PlayDialogue(data.choice2Next));
                }
                else
                {
                    choice2Button.gameObject.SetActive(false);
                }
            }
            else // 일반 대사 진행 처리
            {
                nextButton.gameObject.SetActive(true); // 화면 전체 클릭 버튼 켜기
                choicePanel.SetActive(false);          // 선택지 패널 끄기

                nextButton.onClick.AddListener(() => PlayDialogue(data.nextDialogID));
            }
        }
        else
        {
            Debug.LogWarning($"대사 ID '{id}'를 찾을 수 없습니다.");
            EndDialogue();
        }
    }

    // 3. 액션(Action) 실행 기능
    private void ExecuteAction(string type, string value)
    {
        switch (type)
        {
            case "Quest_Show":
                Debug.Log($"[{value}] 퀘스트 상세 정보창 띄우기 요청 (UI 연결 필요)");
                // 예: QuestUIManager.Instance.ShowQuestDetails(value);
                break;

            case "Quest_Accept":
                // QuestManager로 수락 요청 전송!
                QuestManager.Instance.AcceptQuest(value);
                break;

            case "Quest_Refuse":
                Debug.Log($"[{value}] 퀘스트를 거절했습니다.");
                break;

            case "Quest_Complete": // 나중에 퀘스트 완료 대사용 액션
                QuestManager.Instance.CompleteQuest(value);
                break;
        }
    }

    // 4. 대화 종료 기능
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Debug.Log("대화가 종료되었습니다.");

        // 시네머신 카메라 우선순위 원상복구 로직 호출 위치
    }
}
