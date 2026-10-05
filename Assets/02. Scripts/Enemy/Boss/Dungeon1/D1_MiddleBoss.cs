using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_MiddleBoss : BossModel
{
    [SerializeField, Min(0f)] private float spellWindup = 1.1f;
    [SerializeField, Min(0f)] private float spellRecovery = 1.2f;
    [SerializeField, Min(0.1f)] private float spellRadius = 2.5f;
    [SerializeField, Min(0f)] private float spellCooldown = 4f;

    protected override void Start()
    {
        base.Start();
        Stat.attackRange = Mathf.Max(Stat.attackRange, spellRadius);
        normalPatterns.Add(new SpellPattern(this));
    }

    internal void PrepareForPresentation()
    {
        enabled = false;
        gameObject.SetActive(true);
        Reset();
        isInField = true;
        isCombatStarted = false;
        isImmunity = true;
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
    }

    internal void BeginCombat(Transform spawnPoint)
    {
        enabled = false;
        bossSpawnPoint = spawnPoint;
        gameObject.SetActive(true);
        Reset();
        isInField = true;
        isCombatStarted = false;
        isImmunity = false;
        if (Stat != null) Stat.attackRange = Mathf.Max(Stat.attackRange, spellRadius);
        foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = true;
        if (Agent != null)
        {
            Agent.enabled = true;
            if (spawnPoint != null && !Agent.Warp(spawnPoint.position))
                Debug.LogError("[D1_MiddleBoss] Combat spawn must be on the arena NavMesh.", this);
            if (Agent.isOnNavMesh) { Agent.ResetPath(); Agent.isStopped = true; }
        }
        enabled = true;
    }

    private IEnumerator CastSpell()
    {
        if (Anim != null && Anim.runtimeAnimatorController != null) Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(spellWindup);
        if (!IsDead && Target != null && !Target.isDie)
        {
            foreach (var hit in Physics.OverlapSphere(transform.position + Vector3.up, spellRadius))
            {
                if (hit.GetComponentInParent<CharacterModel>() != Target) continue;
                Target.Damaged(Stat.attackDamage, false);
                break;
            }
        }
        yield return new WaitForSeconds(spellRecovery);
        OnPatternEnd();
    }

    private sealed class SpellPattern : BossPatternBase
    {
        private readonly D1_MiddleBoss owner;
        public SpellPattern(D1_MiddleBoss owner)
        {
            this.owner = owner;
            patternName = "Clockwork Bat Spell";
            cooldown = owner.spellCooldown;
            weight = 1f;
            range = owner.spellRadius;
        }

        public override void Execute(BossModel boss)
        {
            base.Execute(boss);
            owner.StartCoroutine(owner.CastSpell());
        }
    }
}
