using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStigmaLine
{
    A,
    B
};

[CreateAssetMenu(fileName = "StigmaData", menuName = "SO/Character/StigmaData", order = 3)]
public class StigmaDataSO : ScriptableObject
{
    public string stigmaName;
    public int requireLevel;
    public EStigmaLine stigmaLine;
    public string stigmaDescription;
}
