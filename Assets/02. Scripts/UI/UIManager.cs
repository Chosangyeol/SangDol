using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public MainUI mainUI;
    public InventoryUI inventoryUI;
    public SkillTreeUI skillTreeUI;
    public StatusUI statusUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CharacterModel character = FindAnyObjectByType<CharacterModel>();

        mainUI.Init(character.SkillSystem,character.Inventory, character);
        inventoryUI.Init(character.Inventory, character.Equipment);
        skillTreeUI.Init(character.SkillSystem, character);
        statusUI.Init(character.Stat,character.Equipment,character.SpecialStat);

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
    }

    public void RefreshAll()
    {
        inventoryUI.RefreshAll();
        skillTreeUI.RefreshAll();
        statusUI.RefreshAll();

    }
}
