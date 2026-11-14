using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class InimigoGangue3 : MonoBehaviour
{
    public Transform jogador;

    [Header("Patrulha")]
    public Transform pontoInicial;
    public Transform pontoFinal;
    public float velocidadePatrulha = 2f;

    [Header("Detecção")]
    public float distanciaDeteccao = 7f;

    [Header("Cerco coordenado")]
    public float distanciaDoJogador = 2.5f;
    public float velocidadeCerco = 2.5f;
    public float distanciaMinimaEntreCapangas = 1.1f;
    public LayerMask camadasObstaculos;
    public float raioVerificacaoObstaculo = 0.25f;

    [Header("Combate")]
    public float vidaMaxima = 125f;

    private static readonly List<InimigoGangue3> membros = new List<InimigoGangue3>();
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform destinoPatrulha;
    private Vector2 posicaoAnterior;
    private string estadoAnimacaoAtual;
    private float vidaAtual;
    private bool detectandoJogador;
    private bool jogadorMorto;
    private enum Estado { Patrulhando, Cercando }
    private Estado estadoAtual = Estado.Patrulhando;

    public bool EstaCercando => estadoAtual == Estado.Cercando;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        posicaoAnterior = rb.position;
        vidaAtual = vidaMaxima;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        if (!membros.Contains(this)) membros.Add(this);
    }

    void OnDisable()
    {
        membros.Remove(this);
    }

    void Start()
    {
        if (pontoInicial != null && pontoFinal != null)
        {
            rb.position = pontoInicial.position;
            destinoPatrulha = pontoFinal;
        }
    }

    void FixedUpdate()
    {
        LimparMembros();
        detectandoJogador = jogador != null &&
            Vector2.Distance(rb.position, jogador.position) <= distanciaDeteccao &&
            !JogadorFoiDerrotado();

        if (detectandoJogador && !membros.Contains(this)) membros.Add(this);
        else if (!detectandoJogador) membros.Remove(this);

        int participantes = ContarParticipantes();
        bool cercoAtivo = participantes >= 2 && detectandoJogador;

        if (cercoAtivo)
        {
            estadoAtual = Estado.Cercando;
            MoverParaPosicaoDeCerco(participantes);
        }
        else
        {
            estadoAtual = Estado.Patrulhando;
            Patrulhar();
        }

        AtualizarAnimacao();
    }

    private bool JogadorFoiDerrotado()
    {
        if (jogador == null) return true;
        HeartSystem vida = jogador.GetComponent<HeartSystem>();
        return vida != null && vida.vida <= 0f;
    }

    private int ContarParticipantes()
    {
        int total = 0;
        foreach (InimigoGangue3 membro in membros)
            if (membro != null && membro.isActiveAndEnabled && membro.detectandoJogador &&
                membro.jogador == jogador && !membro.JogadorFoiDerrotado()) total++;
        return total;
    }

    private void MoverParaPosicaoDeCerco(int total)
    {
        List<InimigoGangue3> grupo = new List<InimigoGangue3>();
        foreach (InimigoGangue3 membro in membros)
            if (membro != null && membro.isActiveAndEnabled && membro.detectandoJogador &&
                membro.jogador == jogador) grupo.Add(membro);
        grupo.Sort((a, b) => a.GetEntityId().CompareTo(b.GetEntityId()));

        int indice = grupo.IndexOf(this);
        if (indice < 0) return;

        Vector2 centro = jogador.position;
        Vector2 direcaoBase = (rb.position - centro).normalized;
        if (direcaoBase.sqrMagnitude < 0.001f) direcaoBase = Vector2.right;
        float angulo = 360f * indice / Mathf.Max(1, total);
        Vector2 alvo = centro + (Vector2)(Quaternion.Euler(0, 0, angulo) * direcaoBase) * distanciaDoJogador;

        // Procura slots vizinhos quando o alvo fica dentro de um obstáculo.
        if (Physics2D.OverlapCircle(alvo, raioVerificacaoObstaculo, camadasObstaculos) != null)
        {
            bool encontrou = false;
            for (int passo = 1; passo <= total * 2; passo++)
            {
                float desvio = 360f * passo / (total * 2f);
                Vector2 candidato = centro + (Vector2)(Quaternion.Euler(0, 0, angulo + desvio) * direcaoBase) * distanciaDoJogador;
                if (Physics2D.OverlapCircle(candidato, raioVerificacaoObstaculo, camadasObstaculos) == null)
                { alvo = candidato; encontrou = true; break; }
            }
            if (!encontrou) return;
        }

        // Desloca o slot para longe de aliados próximos, preservando posições distintas.
        foreach (InimigoGangue3 aliado in grupo)
        {
            if (aliado == this) continue;
            Vector2 separacao = alvo - aliado.rb.position;
            if (separacao.magnitude < distanciaMinimaEntreCapangas && separacao.sqrMagnitude > 0.001f)
                alvo += separacao.normalized * (distanciaMinimaEntreCapangas - separacao.magnitude);
        }

        Vector2 proxima = Vector2.MoveTowards(rb.position, alvo, velocidadeCerco * Time.fixedDeltaTime);
        rb.MovePosition(proxima);
    }

    private void Patrulhar()
    {
        if (destinoPatrulha == null) return;
        Vector2 nova = Vector2.MoveTowards(rb.position, destinoPatrulha.position,
            velocidadePatrulha * Time.fixedDeltaTime);
        rb.MovePosition(nova);
        if (Vector2.Distance(nova, destinoPatrulha.position) < 0.03f)
            destinoPatrulha = destinoPatrulha == pontoInicial ? pontoFinal : pontoInicial;
    }

    private void AtualizarAnimacao()
    {
        Vector2 deslocamento = rb.position - posicaoAnterior;
        bool movendo = deslocamento.sqrMagnitude > 0.000001f;
        if (spriteRenderer != null && Mathf.Abs(deslocamento.x) > 0.001f)
            spriteRenderer.flipX = deslocamento.x < 0f;
        if (animator != null)
        {
            if (movendo)
            {
                string estado = Mathf.Abs(deslocamento.y) > Mathf.Abs(deslocamento.x)
                    ? (deslocamento.y > 0 ? "Base Layer.g3Capanga costa" : "Base Layer.g3Capaga frente")
                    : (EstaCercando ? "Base Layer.g3Capanga walk atirando" : "Base Layer.g3Capanga-Walk");
                if (estadoAnimacaoAtual != estado)
                {
                    animator.Play(estado, 0, 0f);
                    estadoAnimacaoAtual = estado;
                }
            }
            else if (EstaCercando && estadoAnimacaoAtual != "Base Layer.g3capanga atirando")
            {
                animator.Play("Base Layer.g3capanga atirando", 0, 0f);
                estadoAnimacaoAtual = "Base Layer.g3capanga atirando";
            }
        }
        posicaoAnterior = rb.position;
    }

    private static void LimparMembros()
    {
        for (int i = membros.Count - 1; i >= 0; i--)
            if (membros[i] == null || !membros[i].isActiveAndEnabled) membros.RemoveAt(i);
    }

    public void ReceberDano(float dano)
    {
        if (vidaAtual <= 0f) return;
        vidaAtual = Mathf.Max(0f, vidaAtual - Mathf.Max(0f, dano));
        if (vidaAtual <= 0f) Destroy(gameObject);
    }
}
