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

    [Header("Perseguição com pouca vida")]
    public float vidaLimiteParaPerseguir = 3f;
    public float duracaoPerseguicao = 3f;
    public float velocidadePerseguicao = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 posicaoAnterior;
    private string estadoAnimacaoDirecional;
    private HeartSystem coracoesJogador;

    private Transform destinoPatrulha;
    private Transform destinoVertical;
    private Transform pontoRetornoPatrulha;
    private Vector2 posicaoRetornoPatrulha;
    private float tempoRestanteParado;
    private float fimPerseguicao;
    private bool aguardandoNoPonto;
    private bool jaPerseguiuComPoucaVida;
    private float vidaAtual;

    private enum Estado
    {
        Patrulhando,
        IndoParaCobertura,
        EmCombate,
        PerseguindoJogadorComPoucaVida,
        RetornandoParaPatrulha
    }

    private Estado estadoAtual = Estado.Patrulhando;

    public bool EstaPerseguindoJogadorComPoucaVida =>
        estadoAtual == Estado.PerseguindoJogadorComPoucaVida;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        posicaoAnterior = rb.position;
        vidaAtual = vidaMaxima;
        if (jogador != null)
            coracoesJogador = jogador.GetComponent<HeartSystem>();
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
        VerificarPerseguicaoComPoucaVida();

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

            case Estado.PerseguindoJogadorComPoucaVida:
                PerseguirJogadorComPoucaVida();
                break;

            case Estado.RetornandoParaPatrulha:
                RetornarParaPatrulha();
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
        {
            animator.SetBool("Movendo", estaSeMovendo);

            if (estadoAtual == Estado.PerseguindoJogadorComPoucaVida)
            {
                const string estadoPerseguindo = "Base Layer.g2Capanga1-walkAtirando";
                if (estadoAnimacaoDirecional != estadoPerseguindo)
                {
                    animator.Play(estadoPerseguindo, 0, 0f);
                    estadoAnimacaoDirecional = estadoPerseguindo;
                }
            }
            else if (Mathf.Abs(deslocamento.y) > 0.001f)
            {
                string estadoDesejado = deslocamento.y > 0f
                    ? "Base Layer.g2Capanga - walk Costa"
                    : "Base Layer.g2Capanga - walk Frente";

                if (estadoAnimacaoDirecional != estadoDesejado)
                {
                    animator.Play(estadoDesejado, 0, 0f);
                    estadoAnimacaoDirecional = estadoDesejado;
                }
            }
            else if (estaSeMovendo && !string.IsNullOrEmpty(estadoAnimacaoDirecional))
            {
                animator.Play("Base Layer.Andando", 0, 0f);
                estadoAnimacaoDirecional = null;
            }
            else if (!estaSeMovendo && !string.IsNullOrEmpty(estadoAnimacaoDirecional))
            {
                animator.Play("Base Layer.g2Capanga1", 0, 0f);
                estadoAnimacaoDirecional = null;
            }
        }

        posicaoAnterior = rb.position;
    }

    void VerificarPerseguicaoComPoucaVida()
    {
        if (jogador == null)
            return;

        if (coracoesJogador == null)
            coracoesJogador = jogador.GetComponent<HeartSystem>();

        if (coracoesJogador == null)
            return;

        if (coracoesJogador.vida > vidaLimiteParaPerseguir)
        {
            jaPerseguiuComPoucaVida = false;
            return;
        }

        if (jaPerseguiuComPoucaVida ||
            estadoAtual == Estado.PerseguindoJogadorComPoucaVida ||
            estadoAtual == Estado.RetornandoParaPatrulha ||
            coracoesJogador.vida <= 0f ||
            Vector2.Distance(rb.position, jogador.position) > distanciaDeteccao)
            return;

        jaPerseguiuComPoucaVida = true;
        fimPerseguicao = Time.time + Mathf.Max(0.1f, duracaoPerseguicao);
        pontoRetornoPatrulha = ObterPontoPatrulhaMaisProximo();
        posicaoRetornoPatrulha = pontoRetornoPatrulha != null
            ? (Vector2)pontoRetornoPatrulha.position
            : rb.position;
        estadoAtual = Estado.PerseguindoJogadorComPoucaVida;
    }

    Transform ObterPontoPatrulhaMaisProximo()
    {
        if (pontoInicial == null)
            return pontoFinal;
        if (pontoFinal == null)
            return pontoInicial;

        float distanciaInicial = Vector2.Distance(rb.position, pontoInicial.position);
        float distanciaFinal = Vector2.Distance(rb.position, pontoFinal.position);
        return distanciaInicial <= distanciaFinal ? pontoInicial : pontoFinal;
    }

    void PerseguirJogadorComPoucaVida()
    {
        if (Time.time >= fimPerseguicao || jogador == null)
        {
            estadoAtual = Estado.RetornandoParaPatrulha;
            return;
        }

        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            jogador.position,
            velocidadePerseguicao * Time.fixedDeltaTime
        );
        rb.MovePosition(novaPosicao);
    }

    void RetornarParaPatrulha()
    {
        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            posicaoRetornoPatrulha,
            velocidadePatrulha * Time.fixedDeltaTime
        );
        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, posicaoRetornoPatrulha) > 0.01f)
            return;

        if (pontoRetornoPatrulha == pontoInicial)
            destinoPatrulha = pontoFinal;
        else if (pontoRetornoPatrulha == pontoFinal)
            destinoPatrulha = pontoInicial;

        estadoAtual = Estado.Patrulhando;
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
