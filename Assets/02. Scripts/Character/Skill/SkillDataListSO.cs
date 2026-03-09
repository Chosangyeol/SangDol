using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataList", menuName = "SkillList")]
public class SkillDataListSO : ScriptableObject
{
    public List<SkillBaseSO> skillList;
}
