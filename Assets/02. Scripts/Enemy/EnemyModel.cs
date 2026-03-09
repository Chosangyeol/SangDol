using UnityEngine;

public class EnemyModel : MonoBehaviour
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;

    private E_Stat stat;
    private E_Stat Stat => stat;

    private void Awake()
    {
        stat = new E_Stat(this, statSO);
    }

    public void Damaged(float damage)
    {
        stat.Damaged(damage);
        if (stat.Stat.curHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        stat.Heal(healAmount);
    }

    public void Die()
    {
        // 적 사망 처리
        Debug.Log($"{stat.Stat.enemyName}이(가) 사망했습니다.");
    }
}
