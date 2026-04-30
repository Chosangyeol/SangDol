using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatSO", menuName = "SO/Character/EnemyStatSO", order = 2)]
public class EnemyStatSO : ScriptableObject
{
    [Header("적 세팅값")]
    public string enemyID;
    public string enemyName;
    public bool isAggressive = false;
    public bool canDown = false;

    [Header("적 스탯")]
    public int maxHp;
    public float level;
    public float attackDamage;
    public float moveSpeed = 1f;
    public float attackSpeed = 1f;
    public float down = 100f;

    [Header("범위 설정")]
    public float detactRange = 5f;
    public float attackRange = 2f;

    [Header("적 드랍 테이블")]
    public float expAmount = 0f;
    public int goldAmount = 0;
    public ItemDropTableSO dropTableSO;

}
