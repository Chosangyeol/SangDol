using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public enum EStigmaType
{
    Lv5_A, Lv5_B,
    Lv6_A, Lv6_B,
    Lv7_A, Lv7_B,
    Lv8_A, Lv8_B,
    Lv9_A, Lv9_B,
    Lv10_A, Lv10_B
}

public class C_Stigma
{
    private CharacterModel _model;
    public Dictionary<int, EStigmaType> selectedStigmas = new Dictionary<int, EStigmaType>();

    private BuffSO lv5ABuffSO;
    private BuffSO lv5BBuffSO;
    private BuffSO lv6ABuffSO;
    private BuffSO lv10ABuffSO;
    private BuffSO stunSO;
    private GameObject clonePrefeb;

    // 성흔 별 관리에 사용되는 변수들
    private float lv5ACooldown = 0f;
    private float lv8ACooldown = 0f;

    public C_Stigma(CharacterModel model, BuffSO lv5A, BuffSO lv5B, BuffSO lv6A,BuffSO lv10A, BuffSO stunSO, GameObject clonePrefeb)
    {
        _model = model;

        lv5ABuffSO = lv5A;
        lv5BBuffSO = lv5B;
        lv6ABuffSO = lv6A;
        lv10ABuffSO = lv10A;

        this.stunSO = stunSO;
        this.clonePrefeb = clonePrefeb;

        model.OnHitTarget += HandleOnHitTarget;
        model.OnSkillUsed += HandleOnSkillUsed;
        model.OnDodgeUsed += HandleOnSpaceUsed;

        EquipStigma(5, EStigmaType.Lv5_B);
        EquipStigma(5, EStigmaType.Lv6_A);
        EquipStigma(7, EStigmaType.Lv7_B);
        EquipStigma(7, EStigmaType.Lv10_A);
    }

    public void UpdateStigma(float delta)
    {
        if (lv5ACooldown > 0) lv5ACooldown -= delta;
    }

    public void EquipStigma(int level, EStigmaType stigmaType)
    {
        if (selectedStigmas.TryGetValue(level, out EStigmaType oldStigma))
            RemoveStigmaStats(oldStigma);

        selectedStigmas[level] = stigmaType;
        ApplyStigmaStats(stigmaType);
        Debug.Log($"성흔 장착 완료 : {level} / {selectedStigmas[level]}");
    }

    public void UnEquipStigma(int level)
    {
        if (selectedStigmas.TryGetValue(level,out EStigmaType oldStigma))
        {
            RemoveStigmaStats(oldStigma);
            selectedStigmas.Remove(level);
        }
    }

    /// <summary>
    /// 스탯에 영향을 주는 스티그마 활성화
    /// </summary>
    /// <param name="type">활성화할 스티그마 타입</param>
    private void ApplyStigmaStats(EStigmaType type)
    {
        if (type == EStigmaType.Lv6_A)
        {
            SBuff buff = new SBuff
                (
                    _model.gameObject,
                    _model.gameObject,
                    new HealBuff(_model, lv6ABuffSO, -1, false, 10, 5f)
                );

            _model.Buff.AddBuff(buff);
        }

        if (type == EStigmaType.Lv6_B)
        {
            _model.AddStat(C_Enums.CharacterStat.CooldownReduction, false, -0.1f);
        }

        if (type == EStigmaType.Lv7_A)
        {
            _model.AddStat(C_Enums.CharacterStat.CriticalChance,false,0.05f);
        }

        if (type == EStigmaType.Lv7_B)
        {
            _model.AddStat(C_Enums.CharacterStat.DamageTakeMultiplier,false,0.1f);
        }

        if (type == EStigmaType.Lv9_A)
        {
            _model.AddStat(C_Enums.CharacterStat.DodgeCooldownReduction, false, 1);
        }

        if (type == EStigmaType.Lv9_B)
        {
            _model.AddStat(C_Enums.CharacterStat.DodgeCooldownReduction, false, -3);
        }

        Debug.Log($"성흔 장착 완료 : {type}");
    }

    /// <summary>
    /// 스탯에 영향을 주는 스티그마 비활성화
    /// </summary>
    /// <param name="type">비활성화할 스티그마 타입</param>
    private void RemoveStigmaStats(EStigmaType type)
    {
        if (type == EStigmaType.Lv6_A)
        {
            _model.Buff.RemoveBuff(lv6ABuffSO);
        }

        if (type == EStigmaType.Lv6_B)
        {
            _model.RemoveStat(C_Enums.CharacterStat.CooldownReduction, false, -0.1f);
        }

        if (type == EStigmaType.Lv7_A)
        {
            _model.RemoveStat(C_Enums.CharacterStat.CriticalChance, false, 0.05f);
        }

        if (type == EStigmaType.Lv7_B)
        {
            _model.RemoveStat(C_Enums.CharacterStat.DamageTakeMultiplier, false, 0.1f);
        }

        if (type == EStigmaType.Lv9_A)
        {
            _model.RemoveStat(C_Enums.CharacterStat.DodgeCooldownReduction, false, 1);
        }

        if (type == EStigmaType.Lv9_B)
        {
            _model.RemoveStat(C_Enums.CharacterStat.DodgeCooldownReduction, false, -3);
        }

        Debug.Log($"성흔 해제 완료 : {type}");

    }

    /// <summary>
    /// 공격 적중 시 발생하는 스티그마 핸들
    /// </summary>
    /// <param name="target"></param>
    /// <param name="damage"></param>
    private void HandleOnHitTarget(CharacterModel model, float damage, bool isBasicAttack, EnemyBase target)
    {
        // Lv5 A 루트 스티그마 ( 공격력 증가 )
        if (HasStigma(EStigmaType.Lv5_A) && lv5ACooldown <= 0)
        {
            SBuff buff = new SBuff
                (
                    model.gameObject,
                    model.gameObject,
                    new StatBuff(model, lv5ABuffSO, 10f, C_Enums.CharacterStat.AttackDamage, false, 0.1f)
                );
            model.Buff.AddBuff(buff);
            lv5ACooldown = 25f;
        }

        if (HasStigma(EStigmaType.Lv5_B))
        {
            SBuff buff = new SBuff
                (
                    model.gameObject,
                    model.gameObject,
                    new StatBuff(model, lv5BBuffSO, 4, C_Enums.CharacterStat.MoveSpeed, true, 0.02f)
                );
            model.Buff.AddBuff(buff);

            int buffIndex = model.Buff.ListBuff.FindIndex(x => x.act.buffSO == lv5BBuffSO);

            if (buffIndex != -1)
            {
                int currentStack = model.Buff.ListBuff[buffIndex].act.currentStack;

                if (currentStack >= 30)
                {
                    model.Buff.RemoveBuff(lv5BBuffSO);

                    SBuff stun = new SBuff
                        (
                            model.gameObject,
                            model.gameObject,
                            new StunDeBuff(model, stunSO, 2f)
                        );

                    model.Buff.AddBuff(stun);
                }
            }
        }

        if (isBasicAttack && HasStigma(EStigmaType.Lv7_B))
        {
            if (UnityEngine.Random.Range(0f, 100f) <= 10f)
            {
                _model.StartCoroutine(SpawnCloneAttack(_model, target, damage));
            }
        }

        if (isBasicAttack && HasStigma(EStigmaType.Lv9_B))
        {
            _model.Heal(Mathf.RoundToInt(_model.Stat.Stat.maxHp.FinalValue * 0.02f));
        }
    }

    private void HandleOnSkillUsed()
    {
        if (HasStigma(EStigmaType.Lv6_B))
        {
            _model.Damaged(0.05f, true);
        }
    }

    /// <summary>
    /// Space 이동기를 사용 시 발동하는 스티그마 핸들
    /// </summary>
    private void HandleOnSpaceUsed()
    {
        if (HasStigma(EStigmaType.Lv10_A))
        {
            SBuff buff = new SBuff
                (
                    _model.gameObject,
                    _model.gameObject,
                    new InvincibilityBuff(_model, lv10ABuffSO, 2f)
                );

            _model.Buff.AddBuff(buff);
        }
    }

    /// <summary>
    /// 해당 스티그마가 활성화 상태인지 확인
    /// </summary>
    /// <param name="type">활성화 여부를 확인할 스티그마 타입</param>
    /// <returns>스티그마 활성화 여부를 true / false로 반환</returns>
    public bool HasStigma(EStigmaType type) => selectedStigmas.ContainsValue(type);



    #region Lv7_B 환영 타격
    private IEnumerator SpawnCloneAttack(CharacterModel model, EnemyBase target, float cloneDamage)
    {
        float spawnRadius = 2.0f;

        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;

        // 3. 얻어낸 2D 좌표를 3D 공간의 X축과 Z축(바닥 평면)으로 변환하여 타겟 위치에 더해줍니다.
        Vector3 randomSpawnPos = target.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);


        GameObject clone = GameObject.Instantiate(clonePrefeb, randomSpawnPos, Quaternion.identity);

        Vector3 lookDirection = target.transform.position - clone.transform.position;
        lookDirection.y = 0f;
        clone.transform.rotation = Quaternion.LookRotation(lookDirection);

        int attackMotion = UnityEngine.Random.Range(1, 5);
        clone.GetComponent<Animator>().SetInteger("AttackMotion", attackMotion);

        yield return new WaitForSeconds(0.7f);

        if (target == null || model == null)
        {
            if (clone != null) GameObject.Destroy(clone);
            yield break; // 데미지를 주지 않고 조용히 코루틴 종료
        }

        SDamageInfo info = new SDamageInfo()
        {
            damage = Mathf.RoundToInt(cloneDamage * 0.1f),
            source = model.gameObject,
            knockDownPower = 0,
            isCounterable = false,
            isCritical = false,
            isHeadattack = false,
            isBackattack = false
        };

        target.Damaged(info);

        GameObject.Destroy(clone);
    }
    #endregion
}
