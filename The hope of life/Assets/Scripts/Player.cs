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

    Vector2 movement;

    int ultimaDirecao = 0;
    string animacaoAtual = "";

    void Update()
    {
        if (!podeMover)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            TocarAnimacaoIdle();
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Speed", movement.magnitude);

        AtualizarAnimacao();

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
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        KnockLogic();
    }

    void AtualizarAnimacao()
    {
        if (movement.magnitude > 0)
        {
            if (movement.x != 0)
            {
                ultimaDirecao = 0;
                TocarAnimacao("Walk");
            }
            else if (movement.y > 0)
            {
                ultimaDirecao = 1;
                TocarAnimacao("Walk costa");
            }
            else if (movement.y < 0)
            {
                ultimaDirecao = 2;
                TocarAnimacao("Walk frente");
            }
        }
        else
        {
            TocarAnimacaoIdle();
        }
    }

    void TocarAnimacaoIdle()
    {
        if (ultimaDirecao == 0)
        {
            TocarAnimacao("Idle");
        }
        else if (ultimaDirecao == 1)
        {
            TocarAnimacao("Idle costa");
        }
        else if (ultimaDirecao == 2)
        {
            TocarAnimacao("Idle frente");
        }
    }

    void TocarAnimacao(string nomeAnimacao)
    {
        if (animacaoAtual != nomeAnimacao)
        {
            animator.Play(nomeAnimacao);
            animacaoAtual = nomeAnimacao;
        }
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
            {
                rb.linearVelocity = new Vector2(kBForce, kBForce);
            }
            else
            {
                rb.linearVelocity = new Vector2(-kBForce, kBForce);
            }

            kBCount -= Time.deltaTime;
        }
    }

    void Flip()
    {
        if (movement.x > 0)
        {
            transform.eulerAngles = new Vector2(0, 0);
        }
        else if (movement.x < 0)
        {
            transform.eulerAngles = new Vector2(0, 180);
        }
    }
}