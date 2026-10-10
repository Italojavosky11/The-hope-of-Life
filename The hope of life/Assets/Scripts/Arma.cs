
using UnityEngine;

public class Arma : MonoBehaviour
{
    public DataArma dataArma;
    public Transform firePoint;

    private float nextFireTime;
    private Animator animatorJogador;
    private SpriteRenderer[] spritesArma;
    private bool atacando;
    private bool visualArmaMostrado;

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

        bool mostrarArmaSeparada = atacando &&
            animatorJogador != null &&
            animatorJogador.GetFloat("speed") <= 0.01f;

        if (visualArmaMostrado != mostrarArmaSeparada)
        {
            visualArmaMostrado = mostrarArmaSeparada;
            AtualizarVisual(visualArmaMostrado);
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

        GameObject bala = Instantiate(
            dataArma.prefabBala,
            firePoint.position,
            rotacao
        );

        Bala balaScript = bala.GetComponent<Bala>();

        if (balaScript != null)
            balaScript.Configurar(dataArma);
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