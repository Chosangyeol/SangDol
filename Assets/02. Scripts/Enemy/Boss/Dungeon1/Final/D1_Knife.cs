using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Knife : MonoBehaviour
{
    public float speed = 5f;

    private BuffSO slowDeBuffSO;
    private float damage = 0f;
    private float slowPercent = 0f;
    private float slowDuration = 0f;

    private SBuff sBuff;
    private Transform center;
    private Transform boss;
    private CharacterModel _model;
    private bool isReverse = false;

    public void Init(BuffSO debuffSO,float damage,float slowPercent,float slowDuration,CharacterModel model, Transform boss, Transform center)
    {
        this.slowDeBuffSO = debuffSO;
        this.damage = damage;
        this.slowPercent = slowPercent;
        this.slowDuration = slowDuration;

        this.boss = boss;
        _model = model;
        this.center = center;

        sBuff = new SBuff(
            this.gameObject,
            _model.gameObject,
            new StatBuff(_model, slowDeBuffSO, this.slowDuration, C_Enums.CharacterStat.MoveSpeed, false, this.slowPercent)
            );
    }

    private void Update()
    {
        if (!isReverse)
            UpdatePos();
        else
            UpdatePosReverse();
    }

    private void UpdatePos()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        float dis = Vector3.Distance(center.position, transform.position);
        if (dis > 50f)
            isReverse = true;
    }

    private void UpdatePosReverse()
    {
        if (boss == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 groundTargetPos = new Vector3(boss.position.x, 0.5f, boss.position.z);

        // 2. 칼과 '지면 기준 목표 지점' 사이의 거리를 잽니다.
        float dis = Vector3.Distance(transform.position, groundTargetPos);

        if (dis <= 1.0f)
        {
            Destroy(gameObject);
            return;
        }

        // ⭐️ 3. 하늘에 있는 보스가 아니라, 방금 만든 '지면 목표 지점'을 향하는 방향을 구합니다.
        Vector3 dirToBoss = (groundTargetPos - transform.position).normalized;

        // 칼의 회전(방향)을 목표 지점 쪽으로 부드럽게 꺾어줍니다.
        Quaternion targetRotation = Quaternion.LookRotation(dirToBoss);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        // 꺾인 방향(앞쪽)으로 전진합니다.
        transform.position += transform.forward * speed * 2 * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            if (_model == null) return;

            _model.Buff.AddBuff(sBuff);

            _model.Damaged(damage, true);

            Destroy(this.gameObject);
        }

    }

}
