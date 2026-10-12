using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class D1_MiddleBoss : BossModel
{
    [SerializeField, Min(0f)] private float spellWindup = 1.1f;
    [SerializeField, Min(0f)] private float spellRecovery = 1.2f;
    [SerializeField, Min(0.1f)] private float spellRadius = 2.5f;
    [SerializeField, Min(0f)] private float spellCooldown = 4f;
    [Header("전투 크기 (등장 연출 크기 기준)")]
    [SerializeField, Range(.01f, 1f)] private float combatScaleMultiplier = .5f;
    [Header("짤패턴")]
    [SerializeField, Min(.1f)] private float swipeRadius = 3f;
    [SerializeField, Min(0f)] private float swipeWindup = .4f;
    [SerializeField, Min(0f)] private float swipeRecovery = .55f;
    [SerializeField, Min(0f)] private float swipeCooldown = 2.5f;
    [SerializeField, Min(.1f)] private float pulseRadius = 2f;
    [SerializeField, Min(.1f)] private float pulseRange = 7f;
    [SerializeField, Min(0f)] private float pulseWindup = .65f;
    [SerializeField, Min(0f)] private float pulseInterval = .65f;
    [SerializeField, Min(0f)] private float pulseCooldown = 6f;

    private Vector3 presentationScale;
    private bool presentationScaleCaptured;
    private Material telegraphMaterial;
    protected override void Awake()
    {
        CapturePresentationScale();
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        ApplyCombatScale();
        Stat.attackRange = Mathf.Max(Stat.attackRange, spellRadius);
        normalPatterns.Add(new SpellPattern(this));
        normalPatterns.Add(new SwipePattern(this));
        normalPatterns.Add(new PulsePattern(this));
    }

    private void CapturePresentationScale()
    {
        // The scene binder can prepare an actor under an inactive parent before Awake.
        if (presentationScaleCaptured) return;
        presentationScale = transform.localScale;
        presentationScaleCaptured = true;
    }

    private void ApplyCombatScale()
    {
        CapturePresentationScale();
        transform.localScale = presentationScale * combatScaleMultiplier;
    }

    internal void RestorePresentationScale()
    {
        CapturePresentationScale();
        transform.localScale = presentationScale;
    }

    internal void PrepareForPresentation()
    {
        CapturePresentationScale();
        enabled = false;
        gameObject.SetActive(true);
        Reset();
        RestorePresentationScale();
        isInField = true;
        isCombatStarted = false;
        isImmunity = true;
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
    }

    internal void BeginCombat(Transform spawnPoint)
    {
        enabled = false;
        bossSpawnPoint = spawnPoint;
        gameObject.SetActive(true);
        Reset();
        ApplyCombatScale();
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

    private bool CanHit => !IsDead && Target != null && !Target.isDie;
    private void HitArea(Vector3 center, float radius, bool frontal, float damageMultiplier, Vector3 facing = default(Vector3))
    {
        if (!CanHit || Mathf.Abs(Target.transform.position.y - center.y) > 3f) return;
        if (frontal)
        {
            Vector3 direction = Target.transform.position - center; direction.y = 0f;
            if (direction.sqrMagnitude > .001f && Vector3.Angle(facing.sqrMagnitude > .001f ? facing : transform.forward, direction) > 65f) return;
        }
        foreach (var hit in Physics.OverlapSphere(center + Vector3.up, radius))
        {
            if (hit.GetComponentInParent<CharacterModel>() != Target) continue;
            Target.Damaged(Mathf.Max(1, Mathf.RoundToInt(Stat.attackDamage * damageMultiplier)), false);
            break;
        }
    }

    private void PlayShortAttack(string state)
    {
        if (Anim == null || Anim.runtimeAnimatorController == null) return;
        if (Anim.HasState(0, Animator.StringToHash(state))) Anim.CrossFadeInFixedTime(state, .05f);
        else Anim.SetTrigger("Attack");
    }

    private LineRenderer Telegraph(Vector3 center, float radius, float arc, float yaw = float.NaN)
    {
        var go = new GameObject("BatAttackTelegraph");
        patternObjects.Add(go);
        var line = go.AddComponent<LineRenderer>();
        if (telegraphMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) { patternObjects.Remove(go); Destroy(go); return null; }
            telegraphMaterial = new Material(shader);
        }
        line.sharedMaterial = telegraphMaterial;
        line.useWorldSpace = true; line.loop = arc >= 360f;
        line.widthMultiplier = .12f;
        line.startColor = line.endColor = new Color(1f, .65f, .05f, .9f);
        int segments = 48; line.positionCount = segments + 1;
        if (float.IsNaN(yaw)) yaw = transform.eulerAngles.y;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (yaw - arc * .5f + arc * i / segments) * Mathf.Deg2Rad;
            line.SetPosition(i, center + new Vector3(Mathf.Sin(angle) * radius, .08f, Mathf.Cos(angle) * radius));
        }
        return line;
    }

    private void RemoveTelegraph(LineRenderer line)
    {
        if (line == null) return;
        patternObjects.Remove(line.gameObject); Destroy(line.gameObject);
    }

    private IEnumerator CastSpell()
    {
        if (Anim != null && Anim.runtimeAnimatorController != null) Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(spellWindup);
        HitArea(transform.position, spellRadius, false, 1f);
        yield return new WaitForSeconds(spellRecovery);
        OnPatternEnd();
    }

    private IEnumerator WingSwipe()
    {
        PlayShortAttack("WingSwipe");
        Vector3 facing = Target != null ? Target.transform.position - transform.position : transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < .001f) facing = transform.forward;
        facing.Normalize();
        var line = Telegraph(transform.position, swipeRadius, 130f, Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg);
        yield return new WaitForSeconds(swipeWindup);
        HitArea(transform.position, swipeRadius, true, .7f, facing);
        RemoveTelegraph(line);
        yield return new WaitForSeconds(swipeRecovery);
        OnPatternEnd();
    }

    private IEnumerator DoublePulse()
    {
        PlayShortAttack("DoublePulse");
        if (Target == null) { OnPatternEnd(); yield break; }
        // Commit both explosions to the announced point so the player can dodge.
        Vector3 center = Target.transform.position;
        var line = Telegraph(center, pulseRadius, 360f);
        yield return new WaitForSeconds(pulseWindup);
        HitArea(center, pulseRadius, false, .55f);
        if (line != null) line.startColor = line.endColor = Color.red;
        yield return new WaitForSeconds(pulseInterval);
        HitArea(center, pulseRadius, false, .55f);
        RemoveTelegraph(line);
        yield return new WaitForSeconds(.45f);
        OnPatternEnd();
    }

    private void OnDestroy()
    {
        if (telegraphMaterial != null) Destroy(telegraphMaterial);
    }

    private sealed class SpellPattern : BossPatternBase
    {
        private readonly D1_MiddleBoss owner;
        public SpellPattern(D1_MiddleBoss owner)
        {
            this.owner = owner; patternName = "Clockwork Bat Spell";
            cooldown = owner.spellCooldown; weight = 1f; range = owner.spellRadius;
        }
        public override void Execute(BossModel boss)
        {
            base.Execute(boss); owner.StartCoroutine(owner.CastSpell());
        }
    }

    private sealed class SwipePattern : BossPatternBase
    {
        private readonly D1_MiddleBoss owner;
        public SwipePattern(D1_MiddleBoss owner)
        {
            this.owner = owner; patternName = "Clockwork Bat Wing Swipe";
            cooldown = owner.swipeCooldown; weight = 2f; range = owner.swipeRadius;
        }
        public override void Execute(BossModel boss)
        {
            base.Execute(boss); owner.StartCoroutine(owner.WingSwipe());
        }
    }

    private sealed class PulsePattern : BossPatternBase
    {
        private readonly D1_MiddleBoss owner;
        public PulsePattern(D1_MiddleBoss owner)
        {
            this.owner = owner; patternName = "Clockwork Bat Double Pulse";
            cooldown = owner.pulseCooldown; weight = 1f; range = owner.pulseRange;
        }
        public override void Execute(BossModel boss)
        {
            base.Execute(boss); owner.StartCoroutine(owner.DoublePulse());
        }
    }
}
