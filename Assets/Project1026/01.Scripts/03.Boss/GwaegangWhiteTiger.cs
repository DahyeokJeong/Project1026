using System;
using UnityEngine;

public sealed class GwaegangWhiteTiger : MonoBehaviour
{
    public enum ActionState { Idle, Chase, Claw, Stomp, JumpReady, Jump, Recovery, Dead }
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite[] idle, move, attack, stomp, jumpReady, jumpSlam, dead;
    [SerializeField] private float maxHp = 800f, attackPower = 75f, defense = 20f, moveSpeed = 6f;
    [SerializeField] private float clawDuration = 0.8f, deathDuration = 0.8f;
    [SerializeField] private Color warningColor = new Color(1f, 0.12f, 0.1f, 0.35f);
    private PlayerModel player;
    private NavigationArea navigation;
    private MapArea map;
    private Collider2D hurtbox;
    private LineRenderer warning;
    private Material warningMaterial;
    private float clock, stateTime, nextClaw, nextStomp, nextJump;
    private Vector2 facing = Vector2.right, launch, landing;
    private bool hitApplied, deathReported;
    public ActionState State { get; private set; }
    public float CurrentHp { get; private set; }
    public float MaxHp => maxHp;
    public event Action Defeated;

    public void Initialize(PlayerModel target, NavigationArea area, MapArea bounds)
    {
        player = target;
        navigation = area;
        map = bounds;
        hurtbox = GetComponent<Collider2D>();
        CurrentHp = maxHp;
        clock = 0f;
        nextClaw = 2f;
        nextStomp = 10f;
        nextJump = 6f;
        deathReported = false;
        hurtbox.enabled = true;
        CreateWarning();
        SetState(ActionState.Idle);
        Managers.Damage.Register(transform, TakeDamage);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    private void Tick(float dt)
    {
        if (player == null || !Managers.BattleState.CanUpdate || deathReported) return;
        clock += dt;
        stateTime += dt;
        body.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
        switch (State)
        {
            case ActionState.Dead:
                Animate(dead, deathDuration);
                if (stateTime >= deathDuration) { deathReported = true; Defeated?.Invoke(); }
                return;
            case ActionState.Claw:
                Animate(attack, clawDuration);
                if (!hitApplied && stateTime >= clawDuration * 0.5f)
                {
                    hitApplied = true;
                    Vector2 delta = player.transform.position - transform.position;
                    if (delta.sqrMagnitude <= 4f && (delta == Vector2.zero || Vector2.Dot(facing, delta.normalized) >= 0.7071067f)) DamagePlayer(1f);
                }
                if (stateTime >= clawDuration) SetState(ActionState.Idle);
                return;
            case ActionState.Stomp:
                Animate(stomp, 1.5f);
                if (!hitApplied && stateTime >= 1f)
                {
                    hitApplied = true;
                    warning.enabled = false;
                    HitCircle(transform.position, 4f, 1.5f);
                }
                if (stateTime >= 1.5f) SetState(ActionState.Idle);
                return;
            case ActionState.JumpReady:
                Animate(jumpReady, 1f);
                if (stateTime >= 1f) SetState(ActionState.Jump);
                return;
            case ActionState.Jump:
                float t = Mathf.Clamp01(stateTime);
                transform.position = Vector2.Lerp(launch, landing, t);
                body.transform.localPosition = Vector3.up * (Mathf.Sin(t * Mathf.PI) * 2f);
                Animate(jumpSlam, 1.5f);
                if (t >= 1f)
                {
                    body.transform.localPosition = Vector3.zero;
                    warning.enabled = false;
                    HitCircle(landing, 2f, 2f);
                    SetState(ActionState.Recovery);
                }
                return;
            case ActionState.Recovery:
                Animate(jumpSlam, 1.5f, 1f);
                if (stateTime >= 0.5f) SetState(ActionState.Idle);
                return;
        }
        if (player.IsDead) return;
        Vector2 offset = player.transform.position - transform.position;
        float distance = offset.magnitude;
        if (distance <= 4f && clock >= nextStomp)
        {
            nextStomp = clock + 10f;
            SetState(ActionState.Stomp);
            ShowWarning(transform.position, 4f);
        }
        else if (distance > 4f && distance <= 8f && clock >= nextJump)
        {
            nextJump = clock + 6f;
            launch = transform.position;
            landing = player.transform.position; // Snapshot once at cast start.
            Face(offset);
            SetState(ActionState.JumpReady);
            ShowWarning(landing, 2f);
        }
        else if (distance <= 2f)
        {
            if (clock >= nextClaw)
            {
                nextClaw = clock + 2f;
                Face(offset);
                SetState(ActionState.Claw);
            }
            else
            {
                if (State != ActionState.Idle) SetState(ActionState.Idle);
                Animate(idle, 0.6f, loop: true);
            }
        }
        else
        {
            if (State != ActionState.Chase) SetState(ActionState.Chase);
            Vector2 direction = navigation != null && navigation.IsReady ? navigation.GetPathDirection(transform.position) : offset.normalized;
            Vector2 destination = (Vector2)transform.position + direction * Mathf.Min(moveSpeed * dt, distance - 2f);
            if (map == null || map.CanPlace(destination, 0.5f)) transform.position = destination;
            Face(direction);
            Animate(move, 0.8f, loop: true);
        }
    }

    private void SetState(ActionState state) { State = state; stateTime = 0f; hitApplied = false; }
    private void Face(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;
        facing = direction.normalized;
        if (Mathf.Abs(direction.x) > 0.001f) body.flipX = direction.x < 0f;
    }
    private void Animate(Sprite[] frames, float duration, float offset = 0f, bool loop = false)
    {
        if (frames == null || frames.Length == 0) return;
        int frame = Mathf.FloorToInt((stateTime + offset) / duration * frames.Length);
        body.sprite = frames[loop ? frame % frames.Length : Mathf.Min(frame, frames.Length - 1)];
    }
    public void TakeDamage(float damage)
    {
        if (CurrentHp <= 0f || damage <= 0f || !Managers.BattleState.CanUpdate) return;
        CurrentHp = Mathf.Max(0f, CurrentHp - Mathf.Max(0f, damage - defense));
        if (CurrentHp > 0f) return;
        SetState(ActionState.Dead);
        warning.enabled = false;
        hurtbox.enabled = false;
        body.transform.localPosition = Vector3.zero;
        Managers.Damage.Unregister(transform);
    }
    private void HitCircle(Vector2 center, float radius, float multiplier)
    {
        if (((Vector2)player.transform.position - center).sqrMagnitude <= radius * radius) DamagePlayer(multiplier);
    }
    private void DamagePlayer(float multiplier)
    {
        if (!player.IsDead) Managers.Damage.ApplyDamage(player.transform, Mathf.Floor(attackPower * multiplier + 0.5f));
    }
    private void CreateWarning()
    {
        if (warning != null) return;
        var go = new GameObject("Attack range warning");
        go.transform.SetParent(transform, false);
        warning = go.AddComponent<LineRenderer>();
        warning.useWorldSpace = true;
        warning.loop = true;
        warning.positionCount = 96;
        warning.widthMultiplier = 0.1f;
        warningMaterial = new Material(Shader.Find("Sprites/Default"));
        warning.sharedMaterial = warningMaterial;
        warning.startColor = warning.endColor = warningColor;
        warning.sortingLayerName = "Effect";
        warning.sortingOrder = 100;
        warning.enabled = false;
    }
    private void ShowWarning(Vector2 center, float radius)
    {
        for (int i = 0; i < warning.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / warning.positionCount;
            warning.SetPosition(i, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        warning.enabled = true;
    }
    private void OnDisable()
    {
        if (Managers.Initialized) Managers.Damage.Unregister(transform);
        if (warning != null) warning.enabled = false;
    }
    private void OnDestroy()
    {
        if (warningMaterial == null) return;
        if (Application.isPlaying) Destroy(warningMaterial);
        else DestroyImmediate(warningMaterial);
    }
}
