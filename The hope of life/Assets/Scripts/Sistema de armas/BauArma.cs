
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(InteractionManager))]
public class BauArma : MonoBehaviour
{
    [Header("Animacao")]
    [SerializeField] private Animator animator;
    [SerializeField] private string parametroAbrir = "Abrir";

    [Header("Arma desbloqueada")]
    [SerializeField] private DataArma armaDesbloqueada;

    private bool abrindo;
    private bool aberto;

    private void Awake()
    {
        if (GetComponent<InteractionManager>() == null)
            gameObject.AddComponent<InteractionManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

    }

    public bool PodeInteragir
    {
        get { return !abrindo && !aberto; }
    }

    public bool Interagir()
    {
        if (animator == null || armaDesbloqueada == null)
        {
            Debug.LogError(
                "Configure o Animator e a arma a desbloquear no baú.",
                this
            );
            return false;
        }

        abrindo = true;
        animator.SetTrigger(parametroAbrir);
        return true;
    }

    public void FinalizarAbertura()
    {
        if (!abrindo || aberto)
            return;

        GerenciadorArmas.Desbloquear(armaDesbloqueada);

        aberto = true;
        abrindo = false;
    }
}
