using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum InteractType
{
    NPC,        // 대화만 함 (플레이어 애니메이션 없음)
    Lever,      // 레버 당기기 애니메이션
    Gathering,  // 줍기/채집 애니메이션 (허리 숙이기)
    Portal,      // 포탈/입장 (단순 이동 등)
    Jump,
    Item
}

public interface IInteractable
{
    InteractType interactType { get; }
    string interactName { get; }
    bool canInteract { get; }
    bool isLocked { get; }
    bool isHoldInteraction { get; } // 홀딩형 오브젝트인지 확인
    bool isAutoProgress { get; }
    float holdTime { get; }

    void Init(string text);
    void SetLock(bool locked);

    bool Interact(Transform target);

    void EnableInteract(Transform target);

    void DisableInteract(Transform target);
    void OnInteractCancel();

}
