using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class JohnKentBoss : MonoBehaviour
{
    public enum CombatPhase { Phase1 = 1, Phase2 = 2, Phase3 = 3 }
    private enum BrainState { Waiting, Reposition, Phase1Alternating, Phase1Double, Phase2Fan, Phase2Advance, Phase2Dodge, Phase2Burst, Phase3Rain, Phase3Sequence, Phase3Chase, SpecialWarning, Dead }
    private enum ShotPhase { None, Windup, Recovery }

    [System.Serializable]
    public class BossWeapon
    {
        public string weaponName = "Right";
        public Transform firePoint;
        public float damage = 1.5f;
        public float projectileSpeed = 9f;
        public float fireInterval = 0.7f;
        public float reloadTime = 1.8f;
        public int magazineSize = 6;
        [HideInInspector] public int ammunition;
        [HideInInspector] public float nextShotTime;
        [HideInInspector] public bool reloading;
        [HideInInspector] public float reloadCompleteTime;
    }

    [Header("Health and phases")]
    [Min(1f)] public float maxHealth = 150f;
    [Range(0.01f, 0.99f)] public float phase2Threshold = 0.65f;
    [Range(0.01f, 0.99f)] public float phase3Threshold = 0.30f;
    public float CurrentHealth { get; private set; }
    public CombatPhase CurrentPhase { get; private set; } = CombatPhase.Phase1;

    [Header("Target and movement")]
    public Transform player;
    public float movementSpeed = 2.5f;
    public float phase2AdvanceSpeed = 3f;
    public float phase3ChaseSpeed = 3.5f;
    public float preferredDistance = 5f;
    public float minimumDistance = 2.5f;
    public float maximumDistance = 7f;
    public float dodgeDuration = 0.45f;
    public float dodgeSpeed = 5f;
    public float advanceDuration = 1.6f;
    [Min(0f)] public float wallSkin = 0.02f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Sprite projectileSprite;
    public float projectileLifetime = 4f;
    public float playerDamage = 1f;

    [Header("Attack timing")]
    public float phase1AttackInterval = 1.2f;
    public float phase2AttackInterval = 0.8f;
    public float phase3AttackInterval = 0.55f;
    public float specialWarningTime = 1.1f;
    public float specialDamage = 4f;
    public float specialProjectileSpeed = 11f;
    public float specialInterval = 7f;
    public int fanProjectileCount = 5;
    public float fanAngle = 60f;
    public int bulletRainCount = 12;

    [Header("Shot animation timing")]
    [Tooltip("Seconds into the existing attack clip when the projectile is spawned.")]
    [Min(0.01f)] public float shotEventTime = 0.333f;
    [Tooltip("Seconds into the existing attack clip when it returns to locomotion.")]
    [Min(0.02f)] public float shotRecoveryTime = 0.40f;
    [Min(0.1f)] public float shotAnimationTimeout = 1.2f;
    [Min(0f)] public float movementAnimationThreshold = 0.01f;

    [Header("Independent weapons")]
    public BossWeapon rightWeapon = new BossWeapon { weaponName = "Right" };
    public BossWeapon leftWeapon = new BossWeapon { weaponName = "Left", damage = 0.8f, projectileSpeed = 8.5f, fireInterval = 0.35f, magazineSize = 10, reloadTime = 1.3f };

    private const int RightMask = 1;
    private const int LeftMask = 2;
    private const int BothMask = RightMask | LeftMask;
    private static readonly int AnimationParameter = Animator.StringToHash("Animacao");
    private static readonly string[] AnimationNames =
    {
        "walkNormal", "jkAtirandoParado", "jkFrenteAtirandoParado", "jkCostaAtirandoParado",
        "jkAtirandoAndando", "jkFrenteAtirandoAndando", "jkCostaAtirandoAndando",
        "walkFrente", "walkCosta", "Idle", "Idle frente", "idle costa"
    };
    private static readonly int[] AnimationShortHashes = BuildAnimationHashes(false);
    private static readonly int[] AnimationPathHashes = BuildAnimationHashes(true);

    private Rigidbody2D body;
    private Collider2D collisionShape;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ContactFilter2D movementFilter;
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[8];
    private BrainState state = BrainState.Waiting;
    private ShotPhase shotPhase;
    private Vector2 facingDirection = Vector2.right;
    private Vector2 movementIntent;
    private Vector2 actualMovement;
    private Vector2 lastBodyPosition;
    private Vector2 shotAimDirection = Vector2.right;
    private Vector2 dodgeDirection;
    private float stateEndTime;
    private float nextActionTime;
    private float nextPatternShotTime;
    private float nextSpecialTime;
    private int patternShotIndex;
    private int patternShotCount;
    private int pendingMask;
    private int shotAnimationValue = -1;
    private float shotStartedAt;
    private float specialFireTime;
    private float pendingRightAngle;
    private float pendingLeftAngle;
    private float pendingDamageScale = 1f;
    private float pendingSpeedOverride = -1f;
    private bool pendingSpecialShot;
    private bool specialShotFired;
    private bool dead;

    public bool IsDead => dead;
    public float HealthPercent => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        collisionShape = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementFilter = new ContactFilter2D { useLayerMask = true, layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer), useTriggers = false };
        if (rightWeapon == null) rightWeapon = new BossWeapon { weaponName = "Right" };
        if (leftWeapon == null) leftWeapon = new BossWeapon { weaponName = "Left", damage = 0.8f, projectileSpeed = 8.5f, fireInterval = 0.35f, magazineSize = 10, reloadTime = 1.3f };
        body.gravityScale = 0f;
        body.freezeRotation = true;
        lastBodyPosition = body.position;
        CurrentHealth = Mathf.Max(1f, maxHealth);
        rightWeapon.ammunition = Mathf.Clamp(rightWeapon.ammunition <= 0 ? rightWeapon.magazineSize : rightWeapon.ammunition, 1, Mathf.Max(1, rightWeapon.magazineSize));
        leftWeapon.ammunition = Mathf.Clamp(leftWeapon.ammunition <= 0 ? leftWeapon.magazineSize : leftWeapon.ammunition, 1, Mathf.Max(1, leftWeapon.magazineSize));
        nextSpecialTime = Time.time + specialInterval;
    }

    private void Start()
    {
        FindPlayerIfNeeded();
        nextActionTime = Time.time + 0.25f;
    }

    private void Update()
    {
        if (dead) return;
        FindPlayerIfNeeded();
        UpdatePhase();
        UpdateReload(rightWeapon);
        UpdateReload(leftWeapon);
        if (player == null)
        {
            movementIntent = Vector2.zero;
            CancelShotCycle();
            state = BrainState.Waiting;
            UpdateAnimation();
            return;
        }
        UpdateFacing();
        if (state == BrainState.Waiting && Time.time >= nextActionTime)
            BeginNextAction(Vector2.Distance(body.position, player.position));
        UpdateAction();
        UpdateShotCycle();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (body == null || dead) return;
        actualMovement = (body.position - lastBodyPosition) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        lastBodyPosition = body.position;
        if (movementIntent.sqrMagnitude < 0.000001f)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        body.linearVelocity = Vector2.zero;
        MoveAvoidingWalls();
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null) return;
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) player = target.transform;
    }

    private void BeginNextAction(float distance)
    {
        if (CurrentPhase == CombatPhase.Phase1)
        {
            if (distance < minimumDistance) BeginAction(BrainState.Reposition, 2.5f, 0);
            else if (Random.value < 0.28f) BeginAction(BrainState.Phase1Double, 2.5f, 1);
            else BeginAction(BrainState.Phase1Alternating, 4f, 2);
            return;
        }
        if (CurrentPhase == CombatPhase.Phase2)
        {
            switch (Random.Range(0, 4))
            {
                case 0: BeginAction(BrainState.Phase2Fan, 5f, Mathf.Max(3, fanProjectileCount)); break;
                case 1: BeginAction(BrainState.Phase2Advance, Mathf.Max(0.4f, advanceDuration), 6); break;
                case 2: BeginAction(BrainState.Phase2Dodge, Mathf.Max(0.1f, dodgeDuration), 0); break;
                default: BeginAction(BrainState.Phase2Burst, 6f, 8); break;
            }
            return;
        }
        if (Time.time >= nextSpecialTime)
        {
            specialShotFired = false;
            specialFireTime = Time.time + Mathf.Max(0.2f, specialWarningTime);
            BeginAction(BrainState.SpecialWarning, Mathf.Max(0.2f, specialWarningTime) + 4f, 1);
            return;
        }
        switch (Random.Range(0, 3))
        {
            case 0: BeginAction(BrainState.Phase3Rain, 7f, Mathf.Max(6, bulletRainCount)); break;
            case 1: BeginAction(BrainState.Phase3Sequence, 4f, 3); break;
            default: BeginAction(BrainState.Phase3Chase, Mathf.Max(0.4f, advanceDuration), 6); break;
        }
    }

    private void BeginAction(BrainState newState, float duration, int shotCount)
    {
        CancelShotCycle();
        state = newState;
        movementIntent = Vector2.zero;
        stateEndTime = Time.time + Mathf.Max(0.1f, duration);
        patternShotIndex = 0;
        patternShotCount = shotCount;
        nextPatternShotTime = Time.time;
        if (newState == BrainState.Phase2Dodge)
        {
            Vector2 toPlayer = DirectionToPlayer();
            if (toPlayer.sqrMagnitude < 0.0001f) toPlayer = facingDirection;
            dodgeDirection = new Vector2(-toPlayer.y, toPlayer.x).normalized;
            if (Random.value < 0.5f) dodgeDirection = -dodgeDirection;
        }
    }

    private void UpdateAction()
    {
        movementIntent = Vector2.zero;
        if (state != BrainState.Waiting && Time.time >= stateEndTime && shotPhase == ShotPhase.None &&
            state != BrainState.Reposition && state != BrainState.Phase2Dodge &&
            state != BrainState.Phase2Advance && state != BrainState.Phase3Chase)
        {
            if (state == BrainState.SpecialWarning && !specialShotFired)
                nextSpecialTime = Time.time + specialInterval;
            CompleteAction();
            return;
        }
        float distance = Vector2.Distance(body.position, player.position);
        Vector2 toPlayer = DirectionToPlayer();
        switch (state)
        {
            case BrainState.Reposition:
                if (distance >= preferredDistance || Time.time >= stateEndTime) CompleteAction();
                else movementIntent = (toPlayer.sqrMagnitude > 0.0001f ? -toPlayer : -facingDirection) * movementSpeed;
                break;
            case BrainState.Phase1Alternating: UpdateAlternatingPattern(phase1AttackInterval); break;
            case BrainState.Phase1Double: UpdateDoublePattern(); break;
            case BrainState.Phase2Fan: UpdateFanPattern(); break;
            case BrainState.Phase2Advance:
                if (distance > minimumDistance * 0.65f) movementIntent = toPlayer * phase2AdvanceSpeed;
                UpdateTimedAlternatingPattern(phase2AttackInterval * 0.5f);
                break;
            case BrainState.Phase2Dodge:
                if (Time.time >= stateEndTime) CompleteAction();
                else movementIntent = dodgeDirection * dodgeSpeed;
                break;
            case BrainState.Phase2Burst: UpdateAlternatingPattern(phase2AttackInterval); break;
            case BrainState.Phase3Rain: UpdateRainPattern(); break;
            case BrainState.Phase3Sequence: UpdateThreeShotSequence(); break;
            case BrainState.Phase3Chase:
                if (distance > minimumDistance * 0.5f) movementIntent = toPlayer * phase3ChaseSpeed;
                UpdateTimedAlternatingPattern(phase3AttackInterval);
                break;
            case BrainState.SpecialWarning: UpdateSpecialShot(); break;
        }
    }

    private void UpdateAlternatingPattern(float interval)
    {
        if (shotPhase != ShotPhase.None) return;
        if (patternShotIndex >= patternShotCount) { CompleteAction(); return; }
        if (Time.time < nextPatternShotTime) return;
        int mask = patternShotIndex % 2 == 0 ? RightMask : LeftMask;
        if (TryQueueVolley(mask, 0f, 0f, 1f, -1f, false)) nextPatternShotTime = Time.time + Mathf.Max(0.03f, interval);
        else nextPatternShotTime = Time.time + 0.05f;
    }

    private void UpdateDoublePattern()
    {
        if (shotPhase != ShotPhase.None) return;
        if (patternShotIndex >= 1) { CompleteAction(); return; }
        if (!TryQueueVolley(BothMask, -8f, 8f, 1f, -1f, false)) nextPatternShotTime = Time.time + 0.05f;
    }

    private void UpdateFanPattern()
    {
        if (shotPhase != ShotPhase.None) return;
        if (patternShotIndex >= patternShotCount) { CompleteAction(); return; }
        if (Time.time < nextPatternShotTime) return;
        float fraction = patternShotCount <= 1 ? 0.5f : (float)patternShotIndex / (patternShotCount - 1);
        float offset = -fanAngle * 0.5f + fanAngle * fraction;
        int mask = patternShotIndex % 2 == 0 ? RightMask : LeftMask;
        bool queued = mask == RightMask
            ? TryQueueVolley(mask, offset, 0f, 1f, -1f, false)
            : TryQueueVolley(mask, 0f, offset, 1f, -1f, false);
        nextPatternShotTime = Time.time + (queued ? Mathf.Max(0.03f, phase2AttackInterval * 0.2f) : 0.05f);
    }

    private void UpdateTimedAlternatingPattern(float interval)
    {
        if (Time.time >= stateEndTime)
        {
            if (shotPhase == ShotPhase.None) CompleteAction();
            return;
        }
        if (shotPhase != ShotPhase.None || Time.time < nextPatternShotTime) return;
        int mask = patternShotIndex % 2 == 0 ? RightMask : LeftMask;
        if (TryQueueVolley(mask, 0f, 0f, 1f, -1f, false)) nextPatternShotTime = Time.time + Mathf.Max(0.03f, interval);
        else nextPatternShotTime = Time.time + 0.05f;
    }

    private void UpdateRainPattern()
    {
        if (shotPhase != ShotPhase.None) return;
        if (patternShotIndex >= patternShotCount) { CompleteAction(); return; }
        if (Time.time < nextPatternShotTime) return;
        float angle = patternShotIndex * (360f / patternShotCount);
        int mask = patternShotIndex % 2 == 0 ? RightMask : LeftMask;
        bool queued = mask == RightMask
            ? TryQueueVolley(mask, angle, 0f, 1f, -1f, false)
            : TryQueueVolley(mask, 0f, angle, 1f, -1f, false);
        nextPatternShotTime = Time.time + (queued ? Mathf.Max(0.03f, phase3AttackInterval * 0.25f) : 0.05f);
    }

    private void UpdateThreeShotSequence()
    {
        if (shotPhase != ShotPhase.None) return;
        if (patternShotIndex >= 3) { CompleteAction(); return; }
        if (Time.time < nextPatternShotTime) return;
        int mask = patternShotIndex == 0 ? RightMask : patternShotIndex == 1 ? LeftMask : BothMask;
        bool queued = patternShotIndex == 2
            ? TryQueueVolley(mask, -5f, 5f, 1f, -1f, false)
            : TryQueueVolley(mask, 0f, 0f, 1f, -1f, false);
        nextPatternShotTime = Time.time + (queued ? 0.22f : 0.05f);
    }

    private void UpdateSpecialShot()
    {
        if (specialShotFired && shotPhase == ShotPhase.None)
        {
            nextSpecialTime = Time.time + specialInterval;
            CompleteAction();
            return;
        }
        if (Time.time < specialFireTime || shotPhase != ShotPhase.None) return;
        if (!TryQueueVolley(RightMask, 0f, 0f, specialDamage, specialProjectileSpeed, true))
            nextPatternShotTime = Time.time + 0.05f;
    }

    private bool TryQueueVolley(int mask, float rightAngle, float leftAngle, float damageScale, float speedOverride, bool special)
    {
        if (dead || player == null || projectilePrefab == null || shotPhase != ShotPhase.None) return false;
        if ((mask & RightMask) != 0 && !WeaponReady(rightWeapon)) return false;
        if ((mask & LeftMask) != 0 && !WeaponReady(leftWeapon)) return false;
        pendingMask = mask;
        pendingRightAngle = rightAngle;
        pendingLeftAngle = leftAngle;
        pendingDamageScale = damageScale;
        pendingSpeedOverride = speedOverride;
        pendingSpecialShot = special;
        shotAimDirection = DirectionToPlayer();
        shotPhase = ShotPhase.Windup;
        shotStartedAt = Time.time;
        shotAnimationValue = -1;
        StartShotAnimation(GetShotAnimationValue());
        return true;
    }

    private bool WeaponReady(BossWeapon weapon)
    {
        if (weapon.reloading) return false;
        if (weapon.ammunition <= 0) { StartReload(weapon); return false; }
        return Time.time >= weapon.nextShotTime;
    }

    private void StartReload(BossWeapon weapon)
    {
        if (weapon.reloading) return;
        weapon.reloading = true;
        weapon.reloadCompleteTime = Time.time + Mathf.Max(0.1f, weapon.reloadTime);
    }

    private void UpdateReload(BossWeapon weapon)
    {
        if (weapon == null || !weapon.reloading || Time.time < weapon.reloadCompleteTime) return;
        weapon.ammunition = Mathf.Max(1, weapon.magazineSize);
        weapon.reloading = false;
    }

    private void UpdateShotCycle()
    {
        if (shotPhase == ShotPhase.None) return;
        if (player == null || animator == null || !animator.isActiveAndEnabled) { CancelShotCycle(); return; }
        if (Time.time - shotStartedAt > shotAnimationTimeout) { CancelShotCycle(); return; }
        if (shotPhase == ShotPhase.Windup)
        {
            shotAimDirection = DirectionToPlayer();
            int requiredAnimation = GetShotAnimationValue();
            if (requiredAnimation != shotAnimationValue) { StartShotAnimation(requiredAnimation); return; }
        }
        if (!IsShotAnimationActive()) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = Mathf.Max(0.01f, info.length);
        if (shotPhase == ShotPhase.Windup && info.normalizedTime >= shotEventTime / clipLength)
        {
            FirePendingVolley();
            shotPhase = ShotPhase.Recovery;
        }
        else if (shotPhase == ShotPhase.Recovery && info.normalizedTime >= shotRecoveryTime / clipLength)
        {
            bool wasSpecial = pendingSpecialShot;
            CancelShotCycle();
            if (wasSpecial) specialShotFired = true;
        }
    }

    private int GetShotAnimationValue()
    {
        bool moving = actualMovement.sqrMagnitude >= movementAnimationThreshold * movementAnimationThreshold;
        return GetAnimationStateValue(moving ? actualMovement : shotAimDirection, moving, true);
    }

    private void StartShotAnimation(int value)
    {
        shotAnimationValue = value;
        animator.SetInteger(AnimationParameter, value);
        animator.Play(AnimationPathHashes[value], 0, 0f);
    }

    private bool IsShotAnimationActive()
    {
        return animator != null && animator.isInitialized && animator.layerCount > 0 &&
            !animator.IsInTransition(0) &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimationShortHashes[shotAnimationValue];
    }

    private void FirePendingVolley()
    {
        bool fired = false;
        if ((pendingMask & RightMask) != 0)
            fired |= FireWeapon(rightWeapon, 0, pendingRightAngle, pendingDamageScale, pendingSpeedOverride);
        if ((pendingMask & LeftMask) != 0)
            fired |= FireWeapon(leftWeapon, 1, pendingLeftAngle, pendingDamageScale, pendingSpeedOverride);
        if (fired)
        {
            patternShotIndex++;
            if (pendingSpecialShot) specialShotFired = true;
        }
    }

    private bool FireWeapon(BossWeapon weapon, int index, float offset, float damageScale, float speedOverride)
    {
        if (weapon == null || weapon.reloading || weapon.ammunition <= 0 || Time.time < weapon.nextShotTime || player == null) return false;
        Vector2 direction = Rotate(shotAimDirection, offset);
        if (direction.sqrMagnitude < 0.0001f) return false;
        float side = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 origin = weapon.firePoint != null
            ? (Vector2)transform.TransformPoint(new Vector3(Mathf.Abs(weapon.firePoint.localPosition.x) * side, weapon.firePoint.localPosition.y, weapon.firePoint.localPosition.z))
            : (Vector2)transform.position + new Vector2(0.35f * side, index == 0 ? 0.12f : -0.12f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject projectile = Instantiate(projectilePrefab, origin, Quaternion.Euler(0f, 0f, angle));
        projectile.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        Bala bullet = projectile.GetComponent<Bala>();
        if (bullet == null) { Destroy(projectile); return false; }
        float speed = speedOverride > 0f ? speedOverride : weapon.projectileSpeed;
        bullet.ConfigurarParaInimigo(projectileSprite, playerDamage, speed, projectileLifetime);
        bullet.speed = speed;
        bullet.damage = weapon.damage * damageScale;
        weapon.ammunition--;
        weapon.nextShotTime = Time.time + Mathf.Max(0.05f, weapon.fireInterval);
        SpriteRenderer shotRenderer = projectile.GetComponent<SpriteRenderer>();
        if (shotRenderer != null && spriteRenderer != null)
        {
            shotRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            shotRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        return true;
    }

    private void UpdateFacing()
    {
        Vector2 toPlayer = shotPhase != ShotPhase.None ? shotAimDirection : DirectionToPlayer();
        if (toPlayer.sqrMagnitude > 0.0001f) facingDirection = toPlayer;
        if (spriteRenderer != null && Mathf.Abs(facingDirection.x) > 0.01f)
            spriteRenderer.flipX = facingDirection.x < 0f;
    }

    private void UpdateAnimation()
    {
        if (animator == null || !animator.isActiveAndEnabled) return;
        if (shotPhase != ShotPhase.None && shotAnimationValue >= 0)
        {
            animator.SetInteger(AnimationParameter, shotAnimationValue);
            return;
        }
        bool moving = actualMovement.sqrMagnitude >= movementAnimationThreshold * movementAnimationThreshold;
        Vector2 direction = moving ? actualMovement : facingDirection;
        animator.SetInteger(AnimationParameter, GetAnimationStateValue(direction, moving, false));
    }

    private static int GetAnimationStateValue(Vector2 direction, bool moving, bool shooting)
    {
        int vertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x) ? (direction.y >= 0f ? 1 : -1) : 0;
        if (shooting)
        {
            if (vertical > 0) return moving ? 6 : 3;
            if (vertical < 0) return moving ? 5 : 2;
            return moving ? 4 : 1;
        }
        if (!moving)
        {
            if (vertical > 0) return 11;
            if (vertical < 0) return 10;
            return 9;
        }
        if (vertical > 0) return 8;
        if (vertical < 0) return 7;
        return 0;
    }

    private void CompleteAction()
    {
        if (shotPhase != ShotPhase.None) return;
        state = BrainState.Waiting;
        movementIntent = Vector2.zero;
        nextActionTime = Time.time + AttackIntervalForPhase() * Random.Range(0.65f, 1.2f);
    }

    private float AttackIntervalForPhase()
    {
        if (CurrentPhase == CombatPhase.Phase1) return phase1AttackInterval;
        if (CurrentPhase == CombatPhase.Phase2) return phase2AttackInterval;
        return phase3AttackInterval;
    }

    private void UpdatePhase()
    {
        phase2Threshold = Mathf.Clamp01(phase2Threshold);
        phase3Threshold = Mathf.Clamp(phase3Threshold, 0f, Mathf.Max(0f, phase2Threshold - 0.001f));
        CombatPhase next = HealthPercent >= phase2Threshold ? CombatPhase.Phase1 : HealthPercent >= phase3Threshold ? CombatPhase.Phase2 : CombatPhase.Phase3;
        if ((int)next <= (int)CurrentPhase) return;
        CurrentPhase = next;
        CancelShotCycle();
        movementIntent = Vector2.zero;
        state = BrainState.Waiting;
        nextActionTime = Time.time + 0.2f;
        if (next == CombatPhase.Phase3) nextSpecialTime = Time.time + specialInterval;
    }

    private void CancelShotCycle()
    {
        shotPhase = ShotPhase.None;
        pendingMask = 0;
        pendingSpecialShot = false;
        shotAnimationValue = -1;
    }

    private Vector2 DirectionToPlayer()
    {
        if (player == null) return facingDirection;
        Vector2 direction = (Vector2)player.position - body.position;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : facingDirection;
    }

    private void MoveAvoidingWalls()
    {
        Vector2 displacement = movementIntent * Time.fixedDeltaTime;
        float distance = displacement.magnitude;
        if (distance < 0.000001f) return;
        if (collisionShape == null) { body.MovePosition(body.position + displacement); return; }
        Vector2 direction = displacement / distance;
        int hitCount = collisionShape.Cast(direction, movementFilter, movementHits, distance + wallSkin);
        RaycastHit2D nearestHit = default;
        bool blocked = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (movementHits[i].collider == null || movementHits[i].collider.isTrigger) continue;
            if (!blocked || movementHits[i].distance < nearestHit.distance) { nearestHit = movementHits[i]; blocked = true; }
        }
        if (!blocked) { body.MovePosition(body.position + displacement); return; }
        float safeDistance = Mathf.Max(0f, nearestHit.distance - wallSkin);
        Vector2 allowed = direction * Mathf.Min(distance, safeDistance);
        Vector2 remainder = displacement - allowed;
        Vector2 slide = remainder - nearestHit.normal * Vector2.Dot(remainder, nearestHit.normal);
        if (slide.sqrMagnitude < 0.000001f) slide = FindOpenWallTangent(nearestHit.normal, distance - allowed.magnitude);
        body.MovePosition(body.position + allowed + CastAndClamp(slide));
    }

    private Vector2 FindOpenWallTangent(Vector2 normal, float maxDistance)
    {
        if (maxDistance <= 0f) return Vector2.zero;
        Vector2 tangent = new Vector2(-normal.y, normal.x);
        float positive = AvailableDistance(tangent, maxDistance);
        float negative = AvailableDistance(-tangent, maxDistance);
        Vector2 chosen = positive >= negative ? tangent : -tangent;
        return chosen * Mathf.Min(maxDistance, Mathf.Max(0f, Mathf.Max(positive, negative) - wallSkin));
    }

    private float AvailableDistance(Vector2 direction, float maxDistance)
    {
        if (direction.sqrMagnitude < 0.000001f || collisionShape == null) return 0f;
        int count = collisionShape.Cast(direction.normalized, movementFilter, movementHits, maxDistance);
        float available = maxDistance;
        for (int i = 0; i < count; i++)
            if (movementHits[i].collider != null && !movementHits[i].collider.isTrigger)
                available = Mathf.Min(available, Mathf.Max(0f, movementHits[i].distance - wallSkin));
        return available;
    }

    private Vector2 CastAndClamp(Vector2 displacement)
    {
        float distance = displacement.magnitude;
        if (distance <= 0.0001f || collisionShape == null) return displacement;
        return displacement.normalized * AvailableDistance(displacement.normalized, distance);
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(vector.x * Mathf.Cos(radians) - vector.y * Mathf.Sin(radians), vector.x * Mathf.Sin(radians) + vector.y * Mathf.Cos(radians));
    }

    private static int[] BuildAnimationHashes(bool fullPath)
    {
        int[] hashes = new int[AnimationNames.Length];
        for (int i = 0; i < AnimationNames.Length; i++)
            hashes[i] = Animator.StringToHash(fullPath ? "Base Layer." + AnimationNames[i] : AnimationNames[i]);
        return hashes;
    }

    public void ReceiveDamage(float amount)
    {
        if (dead) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Max(0f, amount));
        UpdatePhase();
        if (CurrentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        state = BrainState.Dead;
        movementIntent = Vector2.zero;
        CancelShotCycle();
        if (body != null) body.linearVelocity = Vector2.zero;
        if (animator != null && animator.isActiveAndEnabled) animator.SetInteger(AnimationParameter, 0);
        enabled = false;
    }

    private void OnDisable()
    {
        movementIntent = Vector2.zero;
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}



