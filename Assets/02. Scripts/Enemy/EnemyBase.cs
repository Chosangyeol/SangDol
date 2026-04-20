using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : PoolableMono
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;
    public Transform textSpawnPos;
    [SerializeField] protected LayerMask _playerLayer;
    [SerializeField] protected LayerMask _groundLayer;

    protected bool _isDead = false;
    public bool IsDead => _isDead;

    protected CharacterModel _target;
    public CharacterModel Target  => _target;

    protected Animator _anim;
    public Animator Anim => _anim;

    protected EnemyStat _stat;
    public EnemyStat Stat => _stat;

    public System.Action<EnemyModel> OnReturnToPool;


    protected virtual void Awake()
    {
        _stat = new EnemyStat(statSO);
    }

    protected virtual void Start()
    {
        _target = FindAnyObjectByType<CharacterModel>();
        _anim = GetComponentInChildren<Animator>();
    }

    public override void Reset()
    {
        _isDead = false;
        OnReturnToPool = null;

        if (_stat != null)
        {
            _stat.curHp = _stat.maxHp;
        }

        if (_anim != null)
        {
            _anim.Rebind();       // 애니메이터를 기본 상태로 되돌림
            _anim.Update(0f);
        }
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
