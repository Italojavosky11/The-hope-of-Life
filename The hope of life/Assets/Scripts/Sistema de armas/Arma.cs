using UnityEngine;

public class Arma : MonoBehaviour
{
    public DataArma dataArma;
    public Transform firePoint;

    private float nextFireTime;
    private Animator animatorJogador;
    private SpriteRenderer[] spritesArma;
    private bool atacando;

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

        if (atacando != pressionandoSpace)
        {
            atacando = pressionandoSpace;

            animatorJogador.SetBool("Atacando", atacando);
        }

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
        // Mantém o ponto configurado na arma para tiros laterais.
        if (Mathf.Abs(direcao.x) > Mathf.Abs(direcao.y))
            return firePoint.position;

        // Nos tiros verticais, usa a distância do ponto da arma ao player
        // e move a saída para a frente na direção da animação.
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
