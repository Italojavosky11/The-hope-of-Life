
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Cobra : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float velocidade = 4f;
    [SerializeField, Min(0.1f)] private float tempoDePerseguicao = 2f;
    [SerializeField, Min(0.01f)] private float distanciaParaRetornar = 0.05f;

    [Header("Combate")]
    [SerializeField] private float dano = 20f;
    [SerializeField, Min(0.1f)] private float vidaMaxima = 20f;

    private float vidaAtual;

    private Rigidbody2D corpo;
    private Transform jogador;
    private Vector2 origem;
    private float fimDaPerseguicao;
    private bool retornando;
    private bool acertouJogador;
    private Action aoRetornar;

    private void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        vidaAtual = vidaMaxima;
    }

    public void Iniciar(
        Transform alvo,
        Vector2 posicaoDeOrigem,
        Action callbackAoRetornar)
    {
        jogador = alvo;
        origem = posicaoDeOrigem;
        aoRetornar = callbackAoRetornar;
        fimDaPerseguicao = Time.time + tempoDePerseguicao;
    }

    private void FixedUpdate()
    {
        if (!retornando &&
            (jogador == null || Time.time >= fimDaPerseguicao))
        {
            retornando = true;
        }

        Vector2 destino = retornando
            ? origem
            : (Vector2)jogador.position;

        Vector2 proximaPosicao = Vector2.MoveTowards(
            corpo.position,
            destino,
            velocidade * Time.fixedDeltaTime
        );

        corpo.MovePosition(proximaPosicao);
        AtualizarDirecao(destino.x - corpo.position.x);

        if (retornando &&
            Vector2.Distance(proximaPosicao, origem) <= distanciaParaRetornar)
        {
            aoRetornar?.Invoke();
            Destroy(gameObject);
        }
    }

    private void AtualizarDirecao(float direcaoX)
    {
        if (Mathf.Abs(direcaoX) < 0.01f)
            return;

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) *
                   (direcaoX >= 0f ? 1f : -1f);

        transform.localScale = escala;
    }

    public void ReceberDano(float quantidade)
    {
        if (retornando)
            return;

        vidaAtual -= quantidade;
        retornando = true;

        if (vidaAtual <= 0f)
        {
            aoRetornar?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (acertouJogador || !other.CompareTag("Player"))
            return;

        HeartSystem coracoes = other.GetComponent<HeartSystem>();

        if (coracoes == null)
            return;

        acertouJogador = true;
        coracoes.TomarDano(dano);
        retornando = true;
    }
}
