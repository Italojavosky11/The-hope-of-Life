using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InimigoGangue2 : MonoBehaviour
{
    public Transform jogador;

    [Header("Patrulha")]
    public Transform pontoInicial;
    public Transform pontoFinal;
    public float velocidadePatrulha = 2f;

    [Header("Detecção")]
    public float distanciaDeteccao = 5f;

    [Header("Cobertura")]
    public Transform pontoDeCobertura;
    public float velocidadeCobertura = 3f;

    [Header("Movimentação na Cobertura")]
    public Transform pontoSuperior;
    public Transform pontoInferior;
    public float velocidadeCombate = 1.5f;

    [Header("Pausa na Cobertura")]
    public float tempoParadoEmCadaPonto = 2f;

    [Header("Combate")]
    public float vidaMaxima = 125f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 posicaoAnterior;

    private Transform destinoPatrulha;
    private Transform destinoVertical;
    private float tempoRestanteParado;
    private bool aguardandoNoPonto;
    private float vidaAtual;

    private enum Estado
    {
        Patrulhando,
        IndoParaCobertura,
        EmCombate
    }

    private Estado estadoAtual = Estado.Patrulhando;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        posicaoAnterior = rb.position;
        vidaAtual = vidaMaxima;
    }

    void Start()
    {
        if (pontoInicial == null || pontoFinal == null)
            return;

        rb.position = pontoInicial.position;
        destinoPatrulha = pontoFinal;
    }

    void FixedUpdate()
    {
        switch (estadoAtual)
        {
            case Estado.Patrulhando:
                Patrulhar();
                VerificarJogador();
                break;

            case Estado.IndoParaCobertura:
                IrParaCobertura();
                break;

            case Estado.EmCombate:
                MovimentarNaCobertura();
                break;
        }

        AtualizarAnimacao();
    }

    void AtualizarAnimacao()
    {
        Vector2 deslocamento = rb.position - posicaoAnterior;
        bool estaSeMovendo = deslocamento.sqrMagnitude > 0.000001f;

        if (spriteRenderer != null && Mathf.Abs(deslocamento.x) > 0.001f)
            spriteRenderer.flipX = deslocamento.x < 0f;

        if (animator != null)
            animator.SetBool("Movendo", estaSeMovendo);

        posicaoAnterior = rb.position;
    }

    void Patrulhar()
    {
        if (destinoPatrulha == null)
            return;

        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            destinoPatrulha.position,
            velocidadePatrulha * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, destinoPatrulha.position) < 0.01f)
        {
            destinoPatrulha = destinoPatrulha == pontoInicial
                ? pontoFinal
                : pontoInicial;
        }
    }

    void VerificarJogador()
    {
        if (jogador == null || pontoDeCobertura == null)
            return;

        float distancia = Vector2.Distance(
            rb.position,
            jogador.position
        );

        if (distancia <= distanciaDeteccao)
        {
            estadoAtual = Estado.IndoParaCobertura;
        }
    }

    void IrParaCobertura()
    {
        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            pontoDeCobertura.position,
            velocidadeCobertura * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, pontoDeCobertura.position) < 0.01f)
        {
            estadoAtual = Estado.EmCombate;
            destinoVertical = pontoSuperior;
            aguardandoNoPonto = false;
        }
    }

    void MovimentarNaCobertura()
    {
        if (pontoSuperior == null || pontoInferior == null)
            return;

        if (aguardandoNoPonto)
        {
            tempoRestanteParado -= Time.fixedDeltaTime;
            if (tempoRestanteParado > 0f)
                return;

            destinoVertical = destinoVertical == pontoSuperior
                ? pontoInferior
                : pontoSuperior;
            aguardandoNoPonto = false;
        }

        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            destinoVertical.position,
            velocidadeCombate * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, destinoVertical.position) < 0.01f)
        {
            aguardandoNoPonto = true;
            tempoRestanteParado = Mathf.Max(0f, tempoParadoEmCadaPonto);
        }
    }

    public void ReceberDano(float dano)
    {
        if (vidaAtual <= 0f)
            return;

        vidaAtual = Mathf.Max(0f, vidaAtual - Mathf.Max(0f, dano));
        if (vidaAtual <= 0f)
            Destroy(gameObject);
    }
}
