using UnityEngine;

public class BarrilDeAgua : MonoBehaviour
{
    public float aguaRecuperada = 1f;

    private Animator animator;
    private Collider2D col;
    private bool destruido = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    public void DestruirBarril()
    {
        if (destruido) return;

        destruido = true;

        if (col != null)
            col.enabled = false;

        animator.SetTrigger("Destruir");
    }

    // Chamar esta função no último frame da animação
    public void FinalizarDestruicao()
    {
        SedeSystem sede = FindFirstObjectByType<SedeSystem>();

        if (sede != null)
        {
            sede.RecuperarSede(aguaRecuperada);
        }

        Destroy(gameObject);
    }
}