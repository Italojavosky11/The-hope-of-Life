
using UnityEngine;


public class InimigoGangueFase1 : MonoBehaviour
{
    public Transform pontoInicial;
    public Transform pontoFinal;

    public float velocidade = 2f;

    private Rigidbody2D rb;
    private Transform destino;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (pontoInicial == null || pontoFinal == null)
            return;

        rb.position = pontoInicial.position;
        destino = pontoFinal;
    }

    void FixedUpdate()
    {
        if (pontoInicial == null || pontoFinal == null)
            return;

        Patrulhar();
    }

    void Patrulhar()
    {
        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            destino.position,
            velocidade * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, destino.position) < 0.001f)
        {
            destino = destino == pontoInicial ? pontoFinal : pontoInicial;
        }
    }
}
