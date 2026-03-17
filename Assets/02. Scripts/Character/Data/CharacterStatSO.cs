using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatSO", menuName = "SO/Character/CharacterStatSO", order = 1)]
public class CharacterStatSO : ScriptableObject
{
    [Header("캐릭터 스탯")]
    public float maxHp;
    public float attackDamage;
    public float defense;

    public float moveSpeed = 1f;
    public float attackSpeed = 1f;
    public float downPower = 1f;

    public float criticalChance = 0.3f;
    public float criticalDamage = 1.5f;

}
