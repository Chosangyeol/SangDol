using System;
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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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

    public void InitGameUIs()
    {
        hasInit = true;

        CharacterModel character = FindAnyObjectByType<CharacterModel>();

        mainUI.Init(character.SkillSystem, character.Inventory, character);
        inventoryUI.Init(character.Inventory, character.Equipment);
        skillTreeUI.Init(character.SkillSystem, character);
        statusUI.Init(character.Stat, character.Equipment, character.SpecialStat);
        questUI.Init();
        buffUI.Init(character.Buff, character);

        inGameUIs.SetActive(true);
    }

    public void ToggleUI(C_Enums.UIList ui)
    {
        if (ui == C_Enums.UIList.Inventory)
        {
            inventoryUI.Toggle();
        }
        else if (ui == C_Enums.UIList.SkillTree)
        {
            skillTreeUI.Toggle();
        }
        else if (ui == C_Enums.UIList.Status)
        {
            statusUI.Toggle();
        }
        else if (ui == C_Enums.UIList.Quest)
        {
            questUI.Toggle();
        }
        else if (ui == C_Enums.UIList.Option)
        {
            optionUI.Toggle();
        }
    }

    public void RefreshAll()
    {
        inventoryUI.RefreshAll();
        skillTreeUI.RefreshAll();
        statusUI.RefreshAll();

    }
}
