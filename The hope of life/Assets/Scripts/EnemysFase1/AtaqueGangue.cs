using UnityEngine;

public class AtaqueGangue : MonoBehaviour
{
    public GameObject prefabBala;
    public Sprite spriteBala;
    public float danoJogador = 1f;
    public float velocidadeBala = 8f;
    public float duracaoBala = 4f;
    public float intervaloEntreDisparos = 2f;
    public Vector2 deslocamentoSaida = new Vector2(0.55f, 0.1f);
    public Vector2 deslocamentoSaidaCima = new Vector2(0.12f, 0.3f);
    public Vector2 deslocamentoSaidaBaixo = new Vector2(0.12f, -0.2f);

    private SpriteRenderer spriteCapanga;
    private Animator animatorCapanga;
    private InimigoGangueFase1 inimigo;
    private float proximoDisparo;

    private static readonly int estadoParadoAtirando =
        Animator.StringToHash("Base Layer.Parado atirando");
    private static readonly int estadoCapanga1AtirandoCima =
        Animator.StringToHash("Base Layer.g1Capanga1-costa parado atirando");
    private static readonly int estadoCapanga1AtirandoBaixo =
        Animator.StringToHash("Base Layer.g1Capanga1 - frente atirando parado");
    private static readonly int estadoCapanga2AtirandoCima =
        Animator.StringToHash("Base Layer.g1Capanga 2 - atirando de costa parado");
    private static readonly int estadoCapanga2AtirandoBaixo =
        Animator.StringToHash("Base Layer.g1Capanga 2 - atirando de frente parado");

    public bool AnimacaoDeDisparoDirecionalAtiva
    {
        get
        {
            if (animatorCapanga == null)
                return false;

            AnimatorStateInfo estadoAtual = animatorCapanga.GetCurrentAnimatorStateInfo(0);
            if (EhEstadoDirecional(estadoAtual.fullPathHash))
                return true;

            if (animatorCapanga.IsInTransition(0))
            {
                AnimatorStateInfo proximoEstado = animatorCapanga.GetNextAnimatorStateInfo(0);
                return EhEstadoDirecional(proximoEstado.fullPathHash);
            }

            return false;
        }
    }

    void Awake()
    {
        spriteCapanga = GetComponent<SpriteRenderer>();
        animatorCapanga = GetComponent<Animator>();
        inimigo = GetComponent<InimigoGangueFase1>();
    }

    void LateUpdate()
    {
        if (animatorCapanga == null || inimigo == null || inimigo.jogador == null ||
            animatorCapanga.IsInTransition(0))
            return;

        AnimatorStateInfo estadoAtual = animatorCapanga.GetCurrentAnimatorStateInfo(0);
        if (EhEstadoDirecional(estadoAtual.fullPathHash))
            return;

        if (estadoAtual.fullPathHash != estadoParadoAtirando || Time.time < proximoDisparo)
            return;

        Vector2 direcaoAoJogador = (Vector2)inimigo.jogador.position - (Vector2)transform.position;
        bool capanga2 = animatorCapanga.runtimeAnimatorController != null &&
                        animatorCapanga.runtimeAnimatorController.name == "Gangue1 Capanga2";

        if (direcaoAoJogador.y > 0.15f)
        {
            animatorCapanga.Play(capanga2 ? estadoCapanga2AtirandoCima : estadoCapanga1AtirandoCima, 0, 0f);
            TentarAtirar(inimigo.jogador);
        }
        else if (direcaoAoJogador.y < -0.15f)
        {
            animatorCapanga.Play(capanga2 ? estadoCapanga2AtirandoBaixo : estadoCapanga1AtirandoBaixo, 0, 0f);
            TentarAtirar(inimigo.jogador);
        }
        else
        {
            TentarAtirar(inimigo.jogador);
        }
        proximoDisparo = Time.time + Mathf.Max(0.05f, intervaloEntreDisparos);
    }

    private static bool EhEstadoDirecional(int estado)
    {
        return estado == estadoCapanga1AtirandoCima ||
               estado == estadoCapanga1AtirandoBaixo ||
               estado == estadoCapanga2AtirandoCima ||
               estado == estadoCapanga2AtirandoBaixo;
    }

    public void TentarAtirar(Transform jogador)
    {
        if (jogador == null || prefabBala == null || spriteBala == null || animatorCapanga == null)
            return;

        Vector2 origem = transform.position;
        Vector2 direcao = ((Vector2)jogador.position - origem).normalized;
        if (direcao.sqrMagnitude < 0.0001f)
            return;

        float lado = spriteCapanga != null && spriteCapanga.flipX ? -1f : 1f;
        if (Mathf.Abs(direcao.x) > 0.01f)
        {
            lado = direcao.x < 0f ? -1f : 1f;
            if (spriteCapanga != null)
                spriteCapanga.flipX = direcao.x < 0f;
        }

        Vector2 deslocamento;
        if (direcao.y > 0.15f)
            deslocamento = new Vector2(lado * Mathf.Abs(deslocamentoSaidaCima.x), deslocamentoSaidaCima.y);
        else if (direcao.y < -0.15f)
            deslocamento = new Vector2(lado * Mathf.Abs(deslocamentoSaidaBaixo.x), deslocamentoSaidaBaixo.y);
        else
            deslocamento = new Vector2(lado * Mathf.Abs(deslocamentoSaida.x), deslocamentoSaida.y);

        Vector3 posicaoSaida = origem + deslocamento;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        GameObject objetoBala = Instantiate(prefabBala, posicaoSaida, Quaternion.Euler(0f, 0f, angulo));
        objetoBala.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        Bala bala = objetoBala.GetComponent<Bala>();
        if (bala == null)
        {
            Destroy(objetoBala);
            return;
        }

        bala.ConfigurarParaInimigo(spriteBala, danoJogador, velocidadeBala, duracaoBala);

        SpriteRenderer spriteProjetil = objetoBala.GetComponent<SpriteRenderer>();
        if (spriteProjetil != null && spriteCapanga != null)
        {
            spriteProjetil.sortingLayerID = spriteCapanga.sortingLayerID;
            spriteProjetil.sortingOrder = spriteCapanga.sortingOrder + 1;
        }
    }
}