using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSetCountUI : MonoBehaviour
{
    public static ShopSetCountUI instance;

    [Header("UI 연결")]
    public GameObject setCountPanel;
    public TMP_InputField inputField;
    public TMP_Text totalPriceText;

    private ItemBaseSO currentItem;
    private int currentBuyPrice;
    private int maxBuyableAmount;

    private void Awake()
    {
        if (instance == null) instance = this;

        // 유저가 InputField에 숫자를 타이핑할 때마다 이벤트 발생
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    public void OpenUI(ItemBaseSO item, int price)
    {
        currentItem = item;
        currentBuyPrice = price;
        setCountPanel.SetActive(true);

        // 현재 소지 금액으로 구매 가능한 최대 수량 계산
        if (currentBuyPrice > 0)
        {
            maxBuyableAmount = NpcShopManager.instance.model.Stat.Stat.gold / currentBuyPrice;
        }
        else
        {
            maxBuyableAmount = 99; // 공짜라면 99개
        }

        // 최대치는 99개로 제한
        if (maxBuyableAmount > 99) maxBuyableAmount = 99;
        // 돈이 전혀 없어서 0개 이하가 되면 0으로 고정
        if (maxBuyableAmount <= 0) maxBuyableAmount = 0;

        // 살 수 있는 돈이 있다면 1, 없으면 0을 기본값으로 표시
        int initialAmount = maxBuyableAmount > 0 ? 1 : 0;
        inputField.text = initialAmount.ToString();
        UpdateTotalPrice(initialAmount);
    }

    private void OnInputValueChanged(string inputSty)
    {
        if (string.IsNullOrEmpty(inputSty)) return;
    
        if (int.TryParse(inputSty, out int inputAmount))
        {
            if (inputAmount > maxBuyableAmount)
            {
                inputAmount = maxBuyableAmount;
                inputField.text = inputAmount.ToString();
            }

            if (inputAmount <= 0 && maxBuyableAmount > 0)
            {
                inputAmount = 1;
                inputField.text = "1";
            }
            else if (maxBuyableAmount == 0)
            {
                inputAmount = 0;
                inputField.text = "0";
            }

            UpdateTotalPrice(inputAmount);
        }
    }


    private void UpdateTotalPrice(int amount)
    {
        int total = currentBuyPrice * amount;
        totalPriceText.text = $"총 비용: {total} G";
    }

    public void ConfirmPurchase()
    {
        int amount = int.Parse(inputField.text);

        if (amount > 0)
        {
            NpcShopManager.instance.BuyItem(currentItem,currentBuyPrice,amount);
        }

        CloseUI();
    }

    public void CloseUI()
    {
        setCountPanel.SetActive(false);
    }
}
