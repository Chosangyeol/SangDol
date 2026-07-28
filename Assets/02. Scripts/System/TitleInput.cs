using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleInput : MonoBehaviour
{
    [Header("연결할 UI 리스트")]
    [SerializeField] private C_Enums.UIList optionType = C_Enums.UIList.Option;

    void Update()
    {
        // Keyboard.current가 null이 아닐 때만 작동 (입력 장치 체크)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleTitleOption();
        }
    }

    private void ToggleTitleOption()
    {
        // UIManager가 싱글톤일 경우 바로 호출
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleUI(optionType);
        }
        else
        {
            Debug.LogWarning("UIManager 인스턴스를 찾을 수 없습니다!");
        }
    }
}
