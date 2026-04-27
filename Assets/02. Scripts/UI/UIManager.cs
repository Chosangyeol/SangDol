using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool isInGame = false;
    public bool hasInit = false;

    public GameObject inGameUIs;
    public MainUI mainUI;
    public InventoryUI inventoryUI;
    public SkillTreeUI skillTreeUI;
    public StatusUI statusUI;
    public QuestUI questUI;
    public BuffList buffUI;
    public OptionUI optionUI;

    private List<C_Enums.UIList> openedUIList = new List<C_Enums.UIList>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (isInGame && !hasInit)
            InitGameUIs();    

        GameEvent.OnUIInvisable += () =>
        {
            mainUI.Toggle(true);
            inventoryUI.Toggle(true);
            skillTreeUI.Toggle(true);
            statusUI.Toggle(true);
            questUI.Toggle(true);
            optionUI.Toggle(true);
        };

        GameEvent.OnMainUIviable += () => mainUI.Toggle(false);
    }

    void Start()
    {
        if (!hasInit)
            inGameUIs.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopUI();
        }
    }

    public void InitGameUIs()
    {
        CharacterModel character = FindAnyObjectByType<CharacterModel>();

        if (character == null)
        {
            Debug.LogWarning("씬에 플레이어가 없어 UI 초기화를 대기합니다.");
            return;
        }

        mainUI.Init(character.SkillSystem, character.Inventory, character);
        inventoryUI.Init(character.Inventory, character.Equipment);
        skillTreeUI.Init(character.SkillSystem, character);
        statusUI.Init(character,character.Stat, character.Equipment, character.SpecialStat);
        questUI.Init();
        buffUI.Init(character.Buff, character);

        hasInit = true;
        inGameUIs.SetActive(true);
    }

    public void ToggleUI(C_Enums.UIList ui)
    {
        bool isNowOpen = false;

        if (ui == C_Enums.UIList.Inventory)
        {
            inventoryUI.Toggle();
            isNowOpen = inventoryUI.gameObject.activeSelf;
        }
        else if (ui == C_Enums.UIList.SkillTree)
        {
            skillTreeUI.Toggle();
            isNowOpen = skillTreeUI.gameObject.activeSelf;
        }
        else if (ui == C_Enums.UIList.Status)
        {
            statusUI.Toggle();
            isNowOpen = statusUI.gameObject.activeSelf;
            if (isNowOpen)
                statusUI.ChangeStatusTap(0);
        }
        else if (ui == C_Enums.UIList.Quest)
        {
            questUI.Toggle();
            isNowOpen = questUI.gameObject.activeSelf;
        }
        else if (ui == C_Enums.UIList.Option)
        {
            if (openedUIList.Count == 0)
            {
                optionUI.Toggle();
                isNowOpen = optionUI.gameObject.activeSelf;
            }
        }

        if (isNowOpen)
        {
            if (openedUIList.Contains(ui))
                openedUIList.Remove(ui);

            openedUIList.Add(ui);
            Debug.Log("추가");
        }
        else
        {
            if (openedUIList.Contains(ui))
                openedUIList.Remove(ui);
        }
    }

    public void RefreshAll()
    {
        inventoryUI.RefreshAll();
        skillTreeUI.RefreshAll();
        statusUI.RefreshAll();

    }

    private void CloseTopUI()
    {
        if (openedUIList.Count > 0)
        {
            C_Enums.UIList topUI = openedUIList[openedUIList.Count - 1];
            ToggleUI(topUI);
        }
        else
        {
            ToggleUI(C_Enums.UIList.Option);
        }
    }
}
