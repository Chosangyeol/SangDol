using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungentEnterUI : MonoBehaviour
{
    private DungeonDataSO dataSO;

    [Header("던전 입장 UI")]
    public Image dungeonBackGroundImage;
    public TMP_Text dungeonNameText;
    public TMP_Text dungeonDescText;
    public TMP_Text dungeonRewardGoldText;

    public RewardItemSlot[] rewardSlots;

    public ItemTooltip tooltip;

    public void Init()
    {

    }

    public void EnterDungeon()
    {
        SceneChanger.instance.LoadScene(dataSO.dungeonSceneName);
    }

    public void UpdateDungeonEnterUI(DungeonDataSO data)
    {
        dataSO = data;

        dungeonBackGroundImage.sprite = data.dungeonBackGroundImage;
        dungeonDescText.text = data.dungeonDescription;
        dungeonNameText.text = data.dungeonName;
        dungeonRewardGoldText.text = $"{data.dungeonRewardGold}Gold";

        int index = 0;

        for (int i = 0; i < data.dungeonRewardItemId.Length; i++ )
        {
            ItemBaseSO item = ItemManager.Instance.GetItemBaseSO(data.dungeonRewardItemId[i]);
            rewardSlots[i].InitSlot(item, tooltip);
            index++;
        }

        for (int i = index; i< rewardSlots.Length; i++ )
        {
            rewardSlots[i].ClearSlot();
        }
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!gameObject.activeSelf);
    }
}
