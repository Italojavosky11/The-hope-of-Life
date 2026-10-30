
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BauArma : MonoBehaviour
{
    [Header("Animacao")]
    [SerializeField] private Animator animator;
    [SerializeField] private string parametroAbrir = "Abrir";

    [Header("Indicador de interacao")]
    [SerializeField] private SpriteRenderer indicadorTecla;

    [Header("Arma desbloqueada")]
    [SerializeField] private DataArma armaDesbloqueada;

    private bool jogadorPerto;
    private bool abrindo;
    private bool aberto;

    private void Awake()
    {
        Collider2D colisor = GetComponent<Collider2D>();
        colisor.isTrigger = true;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (indicadorTecla == null)
            indicadorTecla = GetComponentInChildren<SpriteRenderer>(true);

        AtualizarIndicador();
    }

    private void Update()
    {
        AtualizarIndicador();

        if (!jogadorPerto || abrindo || aberto)
            return;

        if (Input.GetKeyDown(KeyCode.T))
            AbrirBau();
    }

    private void AtualizarIndicador()
    {
        if (indicadorTecla != null)
            indicadorTecla.enabled = jogadorPerto && !abrindo && !aberto;
    }

    private void AbrirBau()
    {
        if (animator == null || armaDesbloqueada == null)
            return;

        abrindo = true;
        animator.SetTrigger(parametroAbrir);
    }

    public void FinalizarAbertura()
    {
        if (!abrindo || aberto)
            return;

        GerenciadorArmas.Desbloquear(armaDesbloqueada);

        aberto = true;
        abrindo = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorPerto = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorPerto = false;
    }
}
