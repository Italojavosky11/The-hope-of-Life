using UnityEngine;

public class Arma : MonoBehaviour
{
    public DataArma dataArma;
    public Transform firePoint;

    private float nextFireTime;
    private Animator animatorJogador;
    private Player controladorJogador;
    private SpriteRenderer[] spritesArma;
    private bool atacando;
    private static readonly int IdleSpringfieldAtirando =
        Animator.StringToHash("Base Layer.IdleAtirandoSpringfield");
    private static readonly int IdleColtAtirando =
        Animator.StringToHash("Base Layer.IdleAtirandoColt");
    private static readonly int IdleRevolverAtirando =
        Animator.StringToHash("Base Layer.IdleAtirandoRevolver");
    private static readonly int IdleCostaAtirando =
        Animator.StringToHash("Base Layer.Idle Costa Atirando");
    private static readonly int IdleFrenteAtirando =
        Animator.StringToHash("Base Layer.IdleAtirandoFrente");
    private static readonly int WalkColt =
        Animator.StringToHash("Base Layer.Colt.WalkColt");
    private static readonly int WalkColtFrente =
        Animator.StringToHash("Base Layer.Colt.WalkColtFrente");
    private static readonly int WalkColtCosta =
        Animator.StringToHash("Base Layer.Colt.WalkColtCosta");
    private static readonly int WalkSpringfield =
        Animator.StringToHash("Base Layer.Springfield.WalkSpringfield");
    private static readonly int WalkSpringfieldFrente =
        Animator.StringToHash("Base Layer.Springfield.WalkSpringfieldFrente");
    private static readonly int WalkSpringfieldCosta =
        Animator.StringToHash("Base Layer.Springfield.WalkSpringfieldCosta");

    void Awake()
    {
        animatorJogador = GetComponentInParent<Animator>();
        controladorJogador = GetComponentInParent<Player>();
        spritesArma = GetComponentsInChildren<SpriteRenderer>(true);

        AtualizarVisual(false);
    }

    void Update()
    {
        if (animatorJogador == null)
            return;

        bool jogadorPodeAtirar = Time.timeScale > 0f &&
            (controladorJogador == null ||
             (controladorJogador.isActiveAndEnabled &&
              controladorJogador.podeMover));
        bool pressionandoSpace = jogadorPodeAtirar &&
            Input.GetKey(KeyCode.Space);

        atacando = pressionandoSpace;
        if (animatorJogador.GetBool("Atacando") != atacando)
            animatorJogador.SetBool("Atacando", atacando);

        // A animação é apenas visual: esperar o Animator entrar em um estado
        // específico fazia o tiro depender de transições que podem não ocorrer
        // após o Rebind usado no respawn ou ao retomar o jogo.
        if (pressionandoSpace)
            Atirar();
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
        ReproduzirAnimacaoDeTiro();

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

    private void ReproduzirAnimacaoDeTiro()
    {
        if (animatorJogador == null)
            return;

        int direcao = animatorJogador.GetInteger("Direcao");
        if (animatorJogador.GetFloat("speed") > 0.01f)
        {
            int estadoAndando = ObterEstadoAndandoComArma(direcao);
            if (estadoAndando != 0 &&
                animatorJogador.HasState(0, estadoAndando))
            {
                AnimatorStateInfo atual =
                    animatorJogador.GetCurrentAnimatorStateInfo(0);
                bool jaEstaNoEstado = atual.fullPathHash == estadoAndando;

                if (!jaEstaNoEstado && animatorJogador.IsInTransition(0))
                {
                    AnimatorStateInfo proximo =
                        animatorJogador.GetNextAnimatorStateInfo(0);
                    jaEstaNoEstado = proximo.fullPathHash == estadoAndando;
                }

                if (!jaEstaNoEstado)
                    animatorJogador.CrossFadeInFixedTime(
                        estadoAndando, 0.03f, 0, 0f);
            }

            return;
        }

        int estado;
        if (direcao == 1)
        {
            estado = IdleCostaAtirando;
        }
        else if (direcao == 2)
        {
            estado = IdleFrenteAtirando;
        }
        else if (dataArma.weaponName == "Springfield")
        {
            estado = IdleSpringfieldAtirando;
        }
        else if (dataArma.weaponName == "Colt")
        {
            estado = IdleColtAtirando;
        }
        else
        {
            estado = IdleRevolverAtirando;
        }

        if (animatorJogador.HasState(0, estado))
            animatorJogador.CrossFadeInFixedTime(estado, 0.03f, 0, 0f);
    }

    private int ObterEstadoAndandoComArma(int direcao)
    {
        if (dataArma == null)
            return 0;

        if (dataArma.weaponName == "Colt")
        {
            if (direcao == 1)
                return WalkColtCosta;
            if (direcao == 2)
                return WalkColtFrente;
            return WalkColt;
        }

        if (dataArma.weaponName == "Springfield")
        {
            if (direcao == 1)
                return WalkSpringfieldCosta;
            if (direcao == 2)
                return WalkSpringfieldFrente;
            return WalkSpringfield;
        }

        return 0;
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
