using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpecialPopup : MonoBehaviour
{
    [Header("팝업 내부 컴포넌트")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("위치 오프셋 (마우스 커서 기준)")]
    [SerializeField] private Vector3 offset = new Vector3(25f, -25f, 0f);

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        // 팝업이 활성화되어 있는 동안 마우스 커서 위치를 따라 움직입니다.
        if (popupPanel.activeSelf)
        {
            transform.position = Input.mousePosition + offset;
        }
    }

    public void Show(string name, string desc)
    {
        popupPanel.SetActive(true);
        statNameText.text = name;
        descriptionText.text = desc;
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
    }
}
