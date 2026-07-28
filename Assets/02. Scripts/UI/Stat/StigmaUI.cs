using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StigmaUI : MonoBehaviour
{
    [Header("스티그마 노드들")]
    public List<StigmaSlotUI> allNodes;

    public StigmaDescUI stigmaDescUI;

    private CharacterModel _model;

    // 🌟 추가: 현재 유저가 클릭해서 선택한 노드를 기억함
    private StigmaSlotUI _selectedNode;

    public void Init(CharacterModel model)
    {
        _model = model;

        foreach (var node in allNodes)
        {
            node.Init(this);
        }

        // 🌟 추가: 장착 UI의 버튼에 실제 장착/해제 함수를 연결함
        if (stigmaDescUI != null && stigmaDescUI.stigmaEquipButton != null)
        {
            stigmaDescUI.stigmaEquipButton.onClick.RemoveAllListeners();
            stigmaDescUI.stigmaEquipButton.onClick.AddListener(OnEquipButtonPressed);
        }

        // 초기 상태에는 정보창을 숨기거나 첫 노드를 강제 선택할 수 있음
        _selectedNode = null;
        if (stigmaDescUI != null) stigmaDescUI.gameObject.SetActive(false);

        RefreshAllNodes();
    }

    public void OnNodeClicked(StigmaSlotUI clickedNode)
    {
        _selectedNode = clickedNode;

        C_Stigma stigma = _model.Stigma;
        int level = clickedNode.stigmaLevel;
        EStigmaType type = clickedNode.stigamType;

        // 현재 이 노드가 이미 장착된 상태인지 확인
        bool isEquipped = false;
        if (stigma.selectedStigmas.TryGetValue(level, out EStigmaType currentType))
        {
            isEquipped = (currentType == type);
        }

        // 잠김 상태 확인
        bool isLocked = _model.Stat.Stat.currentLevel < level;

        // 🌟 우측 정보창 UI 활성화 및 데이터 전달
        if (stigmaDescUI != null)
        {
            stigmaDescUI.gameObject.SetActive(true);
            stigmaDescUI.ShowDescription(clickedNode.stigmaDataSO, isEquipped, isLocked);
        }

        RefreshAllNodes();
    }

    // 🌟 신규 추가: 장착/해제 버튼을 눌렀을 때 실행되는 최종 결정 함수
    public void OnEquipButtonPressed()
    {
        if (_selectedNode == null || _model == null) return;

        int level = _selectedNode.stigmaLevel;
        EStigmaType type = _selectedNode.stigamType;

        // 1. 레벨 제한 조건 체크
        if (_model.Stat.Stat.currentLevel < level)
        {
            Debug.LogWarning("레벨이 부족하여 장착할 수 없습니다.");
            return;
        }

        C_Stigma stigma = _model.Stigma;

        // 2. 기존 토글/장착 로직 실행 (동일 라인 해제 로직은 데이터 구조 내에서 자동 처리됨)
        if (stigma.selectedStigmas.TryGetValue(level, out EStigmaType currentType) && currentType == type)
        {
            stigma.UnEquipStigma(level);
        }
        else
        {
            stigma.EquipStigma(level, type);
        }

        // 3. 전 지점 비주얼 리프레시
        RefreshAllNodes();

        // 4. 장착 상태가 변했으므로 우측 정보창의 버튼 텍스트도 업데이트 ("장착" ↔ "해제")
        if (stigmaDescUI != null)
        {
            bool isNowEquipped = stigma.selectedStigmas.TryGetValue(level, out EStigmaType newType) && newType == type;
            stigmaDescUI.ShowDescription(_selectedNode.stigmaDataSO, isNowEquipped, false);
        }
    }

    public void RefreshAllNodes()
    {
        if (_model == null || _model.Stigma == null) return;

        C_Stigma stigma = _model.Stigma;

        foreach (var node in allNodes)
        {
            bool isEquiped = stigma.selectedStigmas.ContainsValue(node.stigamType);
            bool isLocked = _model.Stat.Stat.currentLevel < node.stigmaLevel;

            node.UpdateVisual(isEquiped, isLocked);
        }
    }
}