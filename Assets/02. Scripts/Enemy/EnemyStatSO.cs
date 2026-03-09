using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatSO", menuName = "SO/Character/EnemyStatSO", order = 2)]
public class EnemyStatSO : ScriptableObject
{
    [Header("적 세팅값")]
    public string enemyName;
    public bool isAggressive = false;
    public bool canDown = false;

    [Header("적 스탯")]
    public float maxHp;
    public float level;
    public float attackDamage;
    public float moveSpeed = 1f;
    public float attackSpeed = 1f;
    

}
