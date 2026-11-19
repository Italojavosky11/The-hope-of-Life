using UnityEngine;

public class Arma : MonoBehaviour
{
    public DataArma dataArma;
    public Transform firePoint;

    private float nextFireTime;
    private Animator animatorJogador;
    private SpriteRenderer[] spritesArma;
    private bool atacando;

    private static readonly int[] estadosDeDisparo =
    {
        Animator.StringToHash("IdleAtirandoColt"),
        Animator.StringToHash("IdleAtirandoRevolver"),
        Animator.StringToHash("IdleAtirandoSpringfield"),
        Animator.StringToHash("IdleAtirandoFrente"),
        Animator.StringToHash("Idle Costa Atirando"),
        Animator.StringToHash("AtirandoRevolverParado")
    };

    void Awake()
    {
        animatorJogador = GetComponentInParent<Animator>();
        spritesArma = GetComponentsInChildren<SpriteRenderer>(true);

        AtualizarVisual(false);
    }

    void Update()
    {
        bool pressionandoSpace =
            Input.GetKey(KeyCode.Space) &&
            animatorJogador != null;

        atacando = pressionandoSpace;
        if (animatorJogador.GetBool("Atacando") != atacando)
            animatorJogador.SetBool("Atacando", atacando);

        if (pressionandoSpace && AnimacaoDeDisparoAtiva())
            Atirar();
    }

    private bool AnimacaoDeDisparoAtiva()
    {
        if (animatorJogador == null || !animatorJogador.isInitialized)
            return false;

        for (int layer = 0; layer < animatorJogador.layerCount; layer++)
        {
            AnimatorStateInfo estadoAtual = animatorJogador.GetCurrentAnimatorStateInfo(layer);
            if (EhAnimacaoDeDisparo(estadoAtual.shortNameHash))
                return true;

            if (animatorJogador.IsInTransition(layer) &&
                EhAnimacaoDeDisparo(animatorJogador.GetNextAnimatorStateInfo(layer).shortNameHash))
                return true;
        }

        return false;
    }

    private static bool EhAnimacaoDeDisparo(int hash)
    {
        for (int i = 0; i < estadosDeDisparo.Length; i++)
        {
            if (hash == estadosDeDisparo[i])
                return true;
        }

        return false;
    }

    void Atirar()
    {
        if (dataArma == null ||
            dataArma.prefabBala == null ||
            firePoint == null)
            return;

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + dataArma.fireRate;

        Vector2 direcao = ObterDirecaoTiro();

        float angulo = Mathf.Atan2(direcao.y, direcao.x)
                       * Mathf.Rad2Deg;

        Quaternion rotacao = Quaternion.Euler(0f, 0f, angulo);

        Vector3 pontoSaida = ObterPontoSaidaBala(direcao);

        GameObject bala = Instantiate(
            dataArma.prefabBala,
            pontoSaida,
            rotacao
        );

        Bala balaScript = bala.GetComponent<Bala>();

        if (balaScript != null)
            balaScript.Configurar(dataArma);
    }

    Vector3 ObterPontoSaidaBala(Vector2 direcao)
    {
        
        if (Mathf.Abs(direcao.x) > Mathf.Abs(direcao.y))
            return firePoint.position;

        
        float distanciaDoPlayer = Vector2.Distance(
            animatorJogador.transform.position,
            firePoint.position
        );

        return animatorJogador.transform.position +
               (Vector3)(direcao.normalized * distanciaDoPlayer);
    }

    Vector2 ObterDirecaoTiro()
    {
        int direcao = animatorJogador.GetInteger("Direcao");

        if (direcao == 1)
            return Vector2.up;

        if (direcao == 2)
            return Vector2.down;

        float lado = animatorJogador.transform.eulerAngles.y;

        return Mathf.Abs(Mathf.DeltaAngle(lado, 180f)) < 1f
            ? Vector2.left
            : Vector2.right;
    }

    void AtualizarVisual(bool mostrar)
    {
        if (spritesArma == null)
            return;

        foreach (SpriteRenderer sprite in spritesArma)
        {
            if (sprite != null)
                sprite.enabled = mostrar;
        }
    }

    void OnDisable()
    {
        if (animatorJogador != null)
            animatorJogador.SetBool("Atacando", false);
    }
}
