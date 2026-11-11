
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InimigoGangueFase1 : MonoBehaviour
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

    private Rigidbody2D rb;

    private Transform destinoPatrulha;
    private Transform destinoVertical;

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
        }
    }

    void MovimentarNaCobertura()
    {
        if (pontoSuperior == null || pontoInferior == null)
            return;

        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            destinoVertical.position,
            velocidadeCombate * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, destinoVertical.position) < 0.01f)
        {
            destinoVertical = destinoVertical == pontoSuperior
                ? pontoInferior
                : pontoSuperior;
        }
    }
}
