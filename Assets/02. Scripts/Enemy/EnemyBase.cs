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

    [Header("외곽선 컴포넌트 참조")]
    [SerializeField] public Outline _outlineComponent;

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
        _stat = new EnemyStat(statSO,this);
        _target = FindAnyObjectByType<CharacterModel>();
        _anim = GetComponentInChildren<Animator>();
        if (_outlineComponent == null) _outlineComponent = GetComponentInChildren<Outline>();
        if (_outlineComponent != null) _outlineComponent.enabled = false;
    }

    protected virtual void Start()
    {
        
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

    public virtual void ToggleOutline(bool enable)
    {
        if (_outlineComponent == null || _isDead) return;

        // 외곽선 컴포넌트 활성화/비활성화
        _outlineComponent.enabled = enable;

        // 🌟 로스트아크 디테일: 외곽선 색상을 빨간색으로 고정
        _outlineComponent.OutlineColor = Color.red;
        _outlineComponent.OutlineWidth = 4f; // 두께 조절

        if (enable)
        {
            Debug.Log($"<color=yellow>[하이라이트] {gameObject.name}에게 마우스 호버됨!</color>");
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
            ToggleOutline(false);
            Die(info.source);
        }
    }

    protected virtual void Die(GameObject source = null)
    {
        
    }
}
