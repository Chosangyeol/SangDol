using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneAndStart : MonoBehaviour
{
    public void OnCutSceneEnd(BossModel boss)
    {
        boss.isCutsceneFinished = true;
    }
}
