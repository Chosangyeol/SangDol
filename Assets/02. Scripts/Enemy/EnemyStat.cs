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

    public EnemyStat(EnemyStatSO statSO)
    {
        enemyName = statSO.enemyName;
        currentLevel = (int)statSO.level;
        maxHp = statSO.maxHp;
        curHp = maxHp;
        attackDamage = statSO.attackDamage;
        moveSpeed = statSO.moveSpeed;
        attackSpeed = statSO.attackSpeed;
    }

    public void Damaged(float damage)
    {
        curHp -= damage;
        if (curHp <= 0)
            curHp = 0;
    }

    public void Heal(float healAmount)
    {
        curHp += healAmount;
        if (curHp > maxHp)
            curHp = maxHp;
    }
}
