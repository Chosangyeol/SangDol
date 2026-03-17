using UnityEngine;

public class EnemyStat
{
    public string enemyName;
    public int currentLevel;
    public float maxHp;
    public float curHp;
    public float attackDamage;
    public float moveSpeed;
    public float attackSpeed;
    public bool canDown;
    public float down;

    public float detactRange;
    public float attackRange;
    

    public EnemyStat(EnemyStatSO statSO)
    {
        enemyName = statSO.enemyName;
        currentLevel = (int)statSO.level;
        maxHp = statSO.maxHp;
        curHp = maxHp;
        attackDamage = statSO.attackDamage;
        moveSpeed = statSO.moveSpeed;
        attackSpeed = statSO.attackSpeed;
        canDown = statSO.canDown;
        down = statSO.down;

        detactRange = statSO.detactRange;
        attackRange = statSO.attackRange;
    }

    public void Damaged(float damage, float downPower = 0f)
    {
        curHp -= damage;
        if (canDown)
        {
            down -= downPower;
            if (down <= 0)
                down = 0;
        }
        if (curHp <= 0)
            curHp = 0;

        Debug.Log($"받은 피해량 : {damage} /  몬스터 남은 채력 : {curHp}");
    }

    public void Heal(float healAmount)
    {
        curHp += healAmount;
        if (curHp > maxHp)
            curHp = maxHp;
    }
}
