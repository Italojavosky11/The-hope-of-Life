
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;

    public float kBForce;
    public float kBCount;
    public float kBTime;

    public bool isKnockRight;
    public bool podeMover = true;

    private Vector2 movement;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!podeMover)
        {
            movement = Vector2.zero;

            if (animator != null)
                animator.SetFloat("speed", 0f);

            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        if (animator != null)
        {
            animator.SetFloat("speed", movement.magnitude);
            AtualizarDirecao();
        }

        Flip();
    }

    void FixedUpdate()
    {
        if (!podeMover)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (kBCount <= 0)
        {
            rb.MovePosition(
                rb.position + movement * moveSpeed * Time.fixedDeltaTime
            );
        }

        KnockLogic();
    }

    void AtualizarDirecao()
    {
        if (movement.x != 0)
            animator.SetInteger("Direcao", 0);
        else if (movement.y > 0)
            animator.SetInteger("Direcao", 1);
        else if (movement.y < 0)
            animator.SetInteger("Direcao", 2);
    }

    void KnockLogic()
    {
        if (kBCount <= 0)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (isKnockRight)
                rb.linearVelocity = new Vector2(kBForce, kBForce);
            else
                rb.linearVelocity = new Vector2(-kBForce, kBForce);

            kBCount -= Time.deltaTime;
        }
    }

    void Flip()
    {
        if (movement.x > 0)
            transform.eulerAngles = new Vector2(0, 0);
        else if (movement.x < 0)
            transform.eulerAngles = new Vector2(0, 180);
    }
}