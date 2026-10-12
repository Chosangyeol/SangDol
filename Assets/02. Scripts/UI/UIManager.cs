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
    public DungentEnterUI dungentEnterUI;

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

        GameEvent.OnUIInvisable += HideGameUIs;
        GameEvent.OnMainUIviable += ShowMainUI;
    }

    void Start()
    {
        if (!hasInit)
            inGameUIs.SetActive(false);
    }

    public void InitGameUIs()
    {
        if (hasInit) return;

        CharacterModel character = FindAnyObjectByType<CharacterModel>();

        if (character == null)
        {
            Debug.LogWarning("씬에 플레이어가 없어 UI 초기화를 대기합니다.");
            return;
        }

        mainUI.Init(character.SkillSystem, character.Inventory, character);
        inventoryUI.Init(character.Inventory, character.Equipment);
        skillTreeUI.Init(character.SkillSystem, character, mainUI.skillToolTip);
        statusUI.Init(character,character.Stat, character.Equipment, character.SpecialStat);
        questUI.Init();
        buffUI.Init(character.Buff, character);

        hasInit = true;
        inGameUIs.SetActive(true);
    }

    private void OnDestroy()
    {
        GameEvent.OnUIInvisable -= HideGameUIs;
        GameEvent.OnMainUIviable -= ShowMainUI;

        if (Instance == this)
            Instance = null;
    }

    private void HideGameUIs()
    {
        mainUI.Toggle(true);
        inventoryUI.Toggle(true);
        skillTreeUI.Toggle(true);
        statusUI.Toggle(true);
        questUI.Toggle(true);
        optionUI.Toggle(true);
        dungentEnterUI.Toggle(true);
        openedUIList.Clear();
    }

    private void ShowMainUI()
    {
        mainUI.Toggle(false);
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
            if (optionUI == null) return;
            optionUI.Toggle();
            isNowOpen = optionUI.IsOpen;
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

    private bool IsPanelOpen(C_Enums.UIList ui)
    {
        switch (ui)
        {
            case C_Enums.UIList.Inventory: return inventoryUI != null && inventoryUI.gameObject.activeSelf;
            case C_Enums.UIList.SkillTree: return skillTreeUI != null && skillTreeUI.gameObject.activeSelf;
            case C_Enums.UIList.Status: return statusUI != null && statusUI.gameObject.activeSelf;
            case C_Enums.UIList.Quest: return questUI != null && questUI.gameObject.activeSelf;
            case C_Enums.UIList.Option: return optionUI != null && optionUI.IsOpen;
            default: return false;
        }
    }

    internal void CloseTopUI()
    {
        while (openedUIList.Count > 0)
        {
            int index = openedUIList.Count - 1;
            var top = openedUIList[index];
            openedUIList.RemoveAt(index);
            if (!IsPanelOpen(top)) continue;
            switch (top)
            {
                case C_Enums.UIList.Inventory: inventoryUI.Toggle(true); break;
                case C_Enums.UIList.SkillTree: skillTreeUI.Toggle(true); break;
                case C_Enums.UIList.Status: statusUI.Toggle(true); break;
                case C_Enums.UIList.Quest: questUI.Toggle(true); break;
                case C_Enums.UIList.Option: optionUI.Toggle(true); break;
            }
            return;
        }
        ToggleUI(C_Enums.UIList.Option);
    }
}
