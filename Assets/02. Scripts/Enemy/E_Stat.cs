using UnityEngine;

public class E_Stat
{
    private EnemyModel owner;
    public EnemyModel Owner => owner;

    private EnemyStatSO statSO;
    private EnemyStat stat;
    public EnemyStat Stat => stat;

    public E_Stat(EnemyModel model, EnemyStatSO statSO)
    {
        this.owner = model;
        this.statSO = statSO;
        this.stat = new EnemyStat(this.statSO);
        return;
    }

    public void Damaged(float damage)
    {
        stat.Damaged(damage);
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
    }
}
