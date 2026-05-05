using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageObject : InteractableObject
{
    [Header("이미지 팝업 UI")]
    public Sprite targetImage;

    protected override void Start()
    {
        base.Start();
        Init($"G - 조사하기");
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;

        UIManager.Instance.mainUI.SetPopUpImage(true, targetImage);

        return true;
    }
}