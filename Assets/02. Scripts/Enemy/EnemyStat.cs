using UnityEngine;

public class EnemyStat
{
    public string enemyName;
    public int currentLevel;
    public int maxHp;
    public int curHp;
    public float attackDamage;
    public float moveSpeed;
    public float attackSpeed;
    public bool canDown;
    public float maxDown;
    public float curDown;

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

        if (statSO.canDown)
        {
            maxDown = statSO.down;
            curDown = maxDown;
        }
        else
        {
            maxDown = 1;
            curDown = 1;
        }


        detactRange = statSO.detactRange;
        attackRange = statSO.attackRange;
    }

    public void Damaged(SDamageInfo info)
    {
        float finalDamage = info.damage;

        if (info.isHeadattack || info.isBackattack)
            finalDamage *= 1.1f;

        int intFinalDamage = Mathf.RoundToInt(finalDamage);

        curHp -= intFinalDamage;

        if (canDown)
        {
            curDown -= info.knockDownPower;
            if (curDown <= 0)
                curDown = 0;
        }
        if (curHp <= 0)
            curHp = 0;

        Debug.Log($"받은 피해량 : {finalDamage} /  몬스터 남은 채력 : {curHp}");
    }

    public void Heal(int healAmount)
    {
        curHp += healAmount;
        if (curHp > maxHp)
            curHp = maxHp;
    }
}
