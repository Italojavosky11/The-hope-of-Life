using UnityEngine;

public class AtaqueG2 : MonoBehaviour
{
    public GameObject prefabBala;
    public Sprite spriteBala;
    public float danoJogador = 1f;
    public float velocidadeBala = 8f;
    public float duracaoBala = 4f;
    public float intervaloEntreDisparos = 2f;
    public Vector2 deslocamentoSaida = new Vector2(0.55f, 0.1f);

    private SpriteRenderer spriteCapanga;
    private Animator animatorCapanga;
    private InimigoGangue2 inimigo;
    private float proximoDisparo;
    private bool disparoFeitoNesteAtaque;

    private static readonly int estadoParadoAtirando =
        Animator.StringToHash("Base Layer.Parado atirando");
    private static readonly int parametroMovendo = Animator.StringToHash("Movendo");
    private static readonly int gatilhoAtirar = Animator.StringToHash("Atirar");

    void Awake()
    {
        spriteCapanga = GetComponent<SpriteRenderer>();
        animatorCapanga = GetComponent<Animator>();
        inimigo = GetComponent<InimigoGangue2>();
    }

    void LateUpdate()
    {
        if (animatorCapanga == null || inimigo == null || inimigo.jogador == null)
            return;

        AnimatorStateInfo estado = animatorCapanga.GetCurrentAnimatorStateInfo(0);
        bool estaAtirando = !animatorCapanga.IsInTransition(0) &&
                            estado.fullPathHash == estadoParadoAtirando;

        if (estaAtirando)
        {
            if (!disparoFeitoNesteAtaque)
            {
                TentarAtirar(inimigo.jogador);
                disparoFeitoNesteAtaque = true;
            }
            return;
        }

        if (animatorCapanga.IsInTransition(0))
            return;

        bool jogadorAoAlcance = Vector2.Distance(transform.position, inimigo.jogador.position)
                                <= inimigo.distanciaDeteccao;
        if (!jogadorAoAlcance ||
            animatorCapanga.GetBool(parametroMovendo) ||
            Time.time < proximoDisparo)
            return;

        animatorCapanga.SetTrigger(gatilhoAtirar);
        proximoDisparo = Time.time + Mathf.Max(0.05f, intervaloEntreDisparos);
        disparoFeitoNesteAtaque = false;
    }

    private void TentarAtirar(Transform jogador)
    {
        if (jogador == null || prefabBala == null || spriteBala == null)
            return;

        Vector2 origem = transform.position;
        Vector2 direcao = ((Vector2)jogador.position - origem).normalized;
        if (direcao.sqrMagnitude < 0.0001f)
            return;

        float lado = direcao.x < 0f ? -1f : 1f;
        if (spriteCapanga != null && Mathf.Abs(direcao.x) > 0.01f)
            spriteCapanga.flipX = direcao.x < 0f;

        Vector2 deslocamento = new Vector2(
            lado * Mathf.Abs(deslocamentoSaida.x),
            deslocamentoSaida.y
        );
        Vector3 posicaoSaida = origem + deslocamento;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        GameObject objetoBala = Instantiate(
            prefabBala,
            posicaoSaida,
            Quaternion.Euler(0f, 0f, angulo)
        );
        objetoBala.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        Bala bala = objetoBala.GetComponent<Bala>();
        if (bala == null)
        {
            Destroy(objetoBala);
            return;
        }

        bala.ConfigurarParaInimigo(
            spriteBala,
            danoJogador,
            velocidadeBala,
            duracaoBala
        );

        SpriteRenderer spriteProjetil = objetoBala.GetComponent<SpriteRenderer>();
        if (spriteProjetil != null && spriteCapanga != null)
        {
            spriteProjetil.sortingLayerID = spriteCapanga.sortingLayerID;
            spriteProjetil.sortingOrder = spriteCapanga.sortingOrder + 1;
        }
    }
}
