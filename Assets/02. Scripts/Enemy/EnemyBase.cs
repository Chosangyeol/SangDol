using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;
    [SerializeField] private LayerMask _playerLayer;


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
        _anim = GetComponent<Animator>();
    }

    public void Damaged(float damage, GameObject source = null)
    {
        if (_isDead) return;

        _stat.Damaged(damage);
        if (_stat.down <= 0)
        {
            Debug.Log("무력화");
        }

        if (_stat.curHp <= 0)
        {
            Die(source);
        }
    }

    protected virtual void Die(GameObject source = null)
    {
        
    }
}
