using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractObjectBase : InteractableObject
{
    [Header("던전 이벤트")]
    protected bool hasActivated = false;

    public Outline outline;

    public System.Action OnActivated;

    protected override void Start()
    {
        base.Start();
        if (outline != null) outline.enabled = false;
        Init($"G - 상호작용");
    }

    public override void SetLock(bool locked)
    {
        base.SetLock(locked); // IsLocked 상태 업데이트 및 UI 갱신

        if (outline != null)
        {
            // 잠금이 해제(false)되면 아웃라인을 켜고, 잠기면 끕니다.
            outline.enabled = !locked;

            // 만약 이미 사용 완료된 오브젝트라면 아웃라인을 켜지 않음
            if (hasActivated) outline.enabled = false;
        }
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;
        if (hasActivated) return false;

        OnActivated?.Invoke();
        hasActivated = true;

        SetLock(true); // 다시 잠금 처리
        UpdateUIState();

        return true;
    }
}
