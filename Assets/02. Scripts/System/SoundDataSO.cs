using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName ="Audio/Sound Data")]
public class SoundDataSO : ScriptableObject
{
    public SFXData[] sfxDatabase;
}
