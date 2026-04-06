using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buff/BuffBaseSO")]
public class BuffSO : ScriptableObject
{
    public string buffName;
    public Sprite buffIcon;
    public string buffDesc;
    public EBuffType type;
}
