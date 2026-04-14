using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;
    public Transform textSpawnPos;
    [SerializeField] protected LayerMask _playerLayer;
    [SerializeField] protected LayerMask _groundLayer;

    protected bool _isDead = false;

    protected CharacterModel _target;
    public CharacterModel Target  => _target;

    protected Animator _anim;
    public Animator Anim => _anim;

    protected EnemyStat _stat;
    public EnemyStat Stat => _stat;


    protected virtual void Awake()
    {
        _stat = new EnemyStat(statSO);
    }

    protected virtual void Start()
    {
        _target = FindAnyObjectByType<CharacterModel>();
        _anim = GetComponentInChildren<Animator>();
    }

    public virtual void Damaged(SDamageInfo info)
    {
        if (_isDead) return;

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.SpawnDamageText(textSpawnPos.position, info.damage, info.isCritical);
        }


        _stat.Damaged(info);   

        if (_stat.curHp <= 0)
        {
            Die(info.source);
        }
    }

    protected virtual void Die(GameObject source = null)
    {
        
    }
}
