using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class JohnKentBoss : MonoBehaviour
{
    [System.Serializable]
    public class BossWeapon
    {
        public string weaponName;
        public Transform firePoint;
        public float damage = 1f;
        public float projectileSpeed = 9f;
        [Min(0.05f)] public float fireInterval = 0.6f;
        [Min(0.1f)] public float reloadTime = 1.5f;
        [Min(1)] public int magazineSize = 8;
        [HideInInspector] public int ammunition;
        [HideInInspector] public float reloadUntil;
    }

    [Header("Vida")]
    [Min(1f)] public float maxHealth = 200f;
    public float CurrentHealth { get; private set; }

    [Header("Pontos de movimentação (offsets da posição inicial)")]
    public Vector2[] movementPoints = { new Vector2(-3f, 0f), new Vector2(0f, 2.5f), new Vector2(3f, 0f), new Vector2(0f, -2.5f), new Vector2(-2f, -2f) };
    [Min(0.1f)] public float initialMovementSpeed = 1.8f;
    [Min(0.1f)] public float maximumMovementSpeed = 4f;
    [Min(0.05f)] public float arrivalDistance = 0.12f;
    [Min(1f)] public float minimumPointWait = 1f;

    [Header("Perseguição G2 quando o jogador está com pouca vida")]
    public Transform player;
    public float playerHeartThreshold = 3f;
    [Min(0.1f)] public float chaseStopDistance = 1.2f;
    public HeartSystem playerHealth;

    [Header("Dificuldade dos disparos")]
    [Min(0.05f)] public float initialFireInterval = 0.65f;
    [Min(0.05f)] public float minimumFireInterval = 0.12f;
    public GameObject projectilePrefab;
    public Sprite projectileSprite;
    public float projectileLifetime = 4f;
    public float playerDamage = 1f;

    [Header("Armas")]
    public BossWeapon rightWeapon = new BossWeapon { weaponName = "Right", damage = 1.5f, projectileSpeed = 9f, fireInterval = 0.65f, magazineSize = 8, reloadTime = 1.5f };
    public BossWeapon leftWeapon = new BossWeapon { weaponName = "Left", damage = 0.8f, projectileSpeed = 8.5f, fireInterval = 0.35f, magazineSize = 10, reloadTime = 1.2f };

    private static readonly int AnimationParameter = Animator.StringToHash("Animacao");
    private static readonly int DeathStateHash = Animator.StringToHash("Base Layer.jkAnimaçãod e morte");
    private static readonly string[] AnimationNames = { "walkNormal", "jkAtirandoParado", "jkFrenteAtirandoParado", "jkCostaAtirandoParado", "jkAtirandoAndando", "jkFrenteAtirandoAndando", "jkCostaAtirandoAndando", "walkFrente", "walkCosta", "Idle", "Idle frente", "idle costa" };
    private static readonly int[] StateHashes = BuildHashes(true);
    private static readonly int[] ShortHashes = BuildHashes(false);
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 origin;
    private Vector2 facing = Vector2.right;
    private Vector2 movementDirection;
    private Vector2 previousPosition;
    private float pointTravelStarted;
    private int pointIndex = -1;
    private bool moving;
    private bool chasing;
    private float pointWaitStarted;
    private float nextShotTime;
    private float shotStarted;
    private int weaponTurn;
    private int pendingAnimation = -1;
    private BossWeapon pendingWeapon;
    private bool pendingShot;
    private bool shotDispatched;
    private bool dead;
    private float movementSpeed;
    private float difficultyProgress;
    private float intervalMultiplier = 1f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        origin = body.position;
        previousPosition = body.position;
        CurrentHealth = Mathf.Max(1f, maxHealth);
        InitWeapon(rightWeapon);
        InitWeapon(leftWeapon);
        if (movementPoints == null || movementPoints.Length != 5)
            movementPoints = new[] { new Vector2(-3f, 0f), new Vector2(0f, 2.5f), new Vector2(3f, 0f), new Vector2(0f, -2.5f), new Vector2(-2f, -2f) };
        FindPlayer();
        UpdateDifficulty();
        pointWaitStarted = Time.time;
    }

    private void Update()
    {
        if (dead) return;
        FindPlayer();
        UpdateDifficulty();
        UpdateReload(rightWeapon);
        UpdateReload(leftWeapon);
        UpdateChaseCondition();
        UpdateFacing();
        if (!moving && !chasing && Time.time - pointWaitStarted >= Mathf.Max(1f, minimumPointWait))
            SelectNextPoint();
        UpdateFire();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (dead || body == null) return;
        Vector2 destination;
        float stopAt;
        if (chasing && player != null)
        {
            destination = player.position;
            stopAt = chaseStopDistance;
        }
        else if (moving && pointIndex >= 0 && pointIndex < movementPoints.Length)
        {
            destination = origin + movementPoints[pointIndex];
            stopAt = arrivalDistance;
        }
        else
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 delta = destination - body.position;
        if (delta.magnitude <= stopAt)
        {
            body.linearVelocity = Vector2.zero;
            if (moving && !chasing)
            {
                moving = false;
                pointWaitStarted = Time.time;
            }
            return;
        }

        if (!chasing && Time.time - pointTravelStarted >= 6f)
        {
            moving = false;
            pointWaitStarted = Time.time;
            body.linearVelocity = Vector2.zero;
            return;
        }
        movementDirection = delta.normalized;
        Vector2 next = Vector2.MoveTowards(body.position, destination, movementSpeed * Time.fixedDeltaTime);
        body.MovePosition(next);
    }

    private void SelectNextPoint()
    {
        if (movementPoints.Length != 5) return;
        int next = pointIndex < 0 ? Random.Range(0, 5) : Random.Range(0, 4);
        if (pointIndex >= 0 && next >= pointIndex) next++;
        pointIndex = next;
        pointTravelStarted = Time.time;
        moving = true;
        movementDirection = (origin + movementPoints[pointIndex] - body.position).normalized;
    }

    private void UpdateChaseCondition()
    {
        if (player == null || playerHealth == null) return;
        bool shouldChase = playerHealth.vida < playerHeartThreshold;
        if (shouldChase == chasing) return;
        chasing = shouldChase;
        moving = false;
        if (!chasing) pointWaitStarted = Time.time;
        body.linearVelocity = Vector2.zero;
    }

    private void UpdateDifficulty()
    {
        difficultyProgress = 1f - Mathf.Clamp01(CurrentHealth / Mathf.Max(1f, maxHealth));
        float speedRatio = Mathf.Max(1f, maximumMovementSpeed / Mathf.Max(0.1f, initialMovementSpeed));
        movementSpeed = initialMovementSpeed * Mathf.Pow(speedRatio, difficultyProgress);
        float intervalRatio = Mathf.Clamp(minimumFireInterval / Mathf.Max(0.05f, initialFireInterval), 0.01f, 1f);
        intervalMultiplier = Mathf.Pow(intervalRatio, difficultyProgress);
    }

    private void UpdateFire()
    {
        if (player == null || projectilePrefab == null) return;
        if (pendingShot)
        {
            ResolvePendingShot();
            return;
        }
        if (Time.time < nextShotTime) return;
        BossWeapon weapon = weaponTurn == 0 ? rightWeapon : leftWeapon;
        if (!Ready(weapon))
        {
            weaponTurn = 1 - weaponTurn;
            return;
        }
        pendingWeapon = weapon;
        facing = DirectionToPlayer();
        pendingAnimation = GetAnimationValue(facing, true, moving || chasing);
        animator.Play(StateHashes[pendingAnimation], 0, 0f);
        shotStarted = Time.time;
        shotDispatched = false;
        pendingShot = true;
    }

    private void ResolvePendingShot()
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.IsInTransition(0)) return;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (pendingAnimation < 0 || info.shortNameHash != ShortHashes[pendingAnimation]) return;
        float clipTime = info.normalizedTime * info.length;
        if (!shotDispatched && clipTime >= 0.333f && pendingWeapon != null && pendingWeapon.ammunition > 0)
        {
            Fire(pendingWeapon);
            pendingWeapon.ammunition--;
            if (pendingWeapon.ammunition <= 0) pendingWeapon.reloadUntil = Time.time + pendingWeapon.reloadTime;
            nextShotTime = Time.time + Mathf.Max(0.05f, pendingWeapon.fireInterval * intervalMultiplier);
            weaponTurn = 1 - weaponTurn;
            shotDispatched = true;
        }
        if (shotDispatched && clipTime >= 0.40f)
        {
            pendingWeapon = null;
            pendingAnimation = -1;
            pendingShot = false;
            shotDispatched = false;
        }
    }

    private void Fire(BossWeapon weapon)
    {
        Vector2 direction = DirectionToPlayer();
        Vector2 originShot = weapon.firePoint != null ? (Vector2)weapon.firePoint.position : body.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject projectile = Instantiate(projectilePrefab, originShot, Quaternion.Euler(0f, 0f, angle));
        Bala bullet = projectile.GetComponent<Bala>();
        if (bullet == null) { Destroy(projectile); return; }
        bullet.ConfigurarParaInimigo(projectileSprite, playerDamage, weapon.projectileSpeed, projectileLifetime);
        bullet.speed = weapon.projectileSpeed;
        bullet.damage = weapon.damage;
    }

    private bool Ready(BossWeapon weapon)
    {
        if (weapon == null || Time.time < weapon.reloadUntil) return false;
        if (weapon.ammunition <= 0)
        {
            weapon.ammunition = 0;
            weapon.reloadUntil = Time.time + weapon.reloadTime;
            return false;
        }
        return true;
    }

    private void UpdateReload(BossWeapon weapon)
    {
        if (weapon != null && weapon.ammunition <= 0 && Time.time >= weapon.reloadUntil)
            weapon.ammunition = Mathf.Max(1, weapon.magazineSize);
    }

    private void UpdateFacing()
    {
        if (player != null) facing = DirectionToPlayer();
        if (spriteRenderer != null && Mathf.Abs(facing.x) > 0.01f)
            spriteRenderer.flipX = facing.x < 0f;
    }

    private static int GetAnimationValue(Vector2 direction, bool attacking, bool moving)
    {
        bool vertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
        if (attacking)
        {
            if (moving) return vertical ? (direction.y < 0f ? 5 : 6) : 4;
            return vertical ? (direction.y < 0f ? 2 : 3) : 1;
        }
        if (moving) return vertical ? (direction.y < 0f ? 7 : 8) : 0;
        return vertical ? (direction.y < 0f ? 10 : 11) : 9;
    }

    private void UpdateAnimator()
    {
        if (animator == null || !animator.isActiveAndEnabled) return;
        Vector2 direction = moving || chasing ? movementDirection : facing;
        bool vertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
        int value;
        if (pendingShot && pendingAnimation >= 1 && pendingAnimation <= 6)
            value = pendingAnimation;
        else if (moving || chasing)
            value = vertical ? (direction.y < 0f ? 7 : 8) : 0;
        else
            value = vertical ? (direction.y < 0f ? 10 : 11) : 9;
        animator.SetInteger(AnimationParameter, value);
    }

    private Vector2 DirectionToPlayer()
    {
        Vector2 delta = player != null ? (Vector2)player.position - body.position : facing;
        return delta.sqrMagnitude > 0.0001f ? delta.normalized : facing;
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject target = GameObject.FindGameObjectWithTag("Player");
            if (target != null) player = target.transform;
        }
        if (playerHealth == null && player != null) playerHealth = player.GetComponent<HeartSystem>();
    }

    private static void InitWeapon(BossWeapon weapon)
    {
        if (weapon != null) weapon.ammunition = Mathf.Max(1, weapon.magazineSize);
    }

    private static int[] BuildHashes(bool fullPath)
    {
        int[] hashes = new int[AnimationNames.Length];
        for (int i = 0; i < hashes.Length; i++) hashes[i] = Animator.StringToHash(fullPath ? "Base Layer." + AnimationNames[i] : AnimationNames[i]);
        return hashes;
    }

    public void ReceiveDamage(float amount)
    {
        if (dead) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Max(0f, amount));
        UpdateDifficulty();
        if (CurrentHealth <= 0f) Die();
    }

    private void Die()
    {
        dead = true;
        pendingShot = false;
        moving = false;
        chasing = false;
        body.linearVelocity = Vector2.zero;
        foreach (Collider2D hitbox in GetComponents<Collider2D>()) hitbox.enabled = false;
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetInteger(AnimationParameter, -1);
            animator.Play(DeathStateHash, 0, 0f);
            animator.Update(0f);
        }
        enabled = false;
    }

    private void OnDisable()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}