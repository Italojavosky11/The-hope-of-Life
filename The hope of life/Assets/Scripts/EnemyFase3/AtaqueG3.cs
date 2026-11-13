using UnityEngine;

public class AtaqueG3 : MonoBehaviour
{
    public GameObject prefabBala;
    public Sprite spriteBala;
    public float danoJogador = 1f;
    public float velocidadeBala = 8f;
    public float duracaoBala = 4f;
    public float intervaloEntreDisparos = 2f;
    public Vector2 deslocamentoSaida = new Vector2(0.55f, 0.1f);

    private SpriteRenderer spriteCapanga;
    private InimigoGangue3 inimigo;
    private float proximoDisparo;

    void Awake()
    {
        spriteCapanga = GetComponent<SpriteRenderer>();
        inimigo = GetComponent<InimigoGangue3>();
    }

    void Update()
    {
        if (inimigo == null || !inimigo.EstaCercando || inimigo.jogador == null ||
            Time.time < proximoDisparo) return;

        TentarAtirar(inimigo.jogador);
        proximoDisparo = Time.time + Mathf.Max(0.05f, intervaloEntreDisparos);
    }

    private void TentarAtirar(Transform alvo)
    {
        if (alvo == null || prefabBala == null || spriteBala == null) return;
        Vector2 origem = transform.position;
        Vector2 direcao = ((Vector2)alvo.position - origem).normalized;
        if (direcao.sqrMagnitude < 0.0001f) return;

        if (spriteCapanga != null && Mathf.Abs(direcao.x) > 0.01f)
            spriteCapanga.flipX = direcao.x < 0f;

        float lado = direcao.x < 0f ? -1f : 1f;
        Vector2 deslocamento = new Vector2(lado * Mathf.Abs(deslocamentoSaida.x), deslocamentoSaida.y);
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        GameObject objetoBala = Instantiate(prefabBala, origem + deslocamento,
            Quaternion.Euler(0f, 0f, angulo));
        objetoBala.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        Bala bala = objetoBala.GetComponent<Bala>();
        if (bala == null) { Destroy(objetoBala); return; }
        bala.ConfigurarParaInimigo(spriteBala, danoJogador, velocidadeBala, duracaoBala);

        SpriteRenderer spriteProjetil = objetoBala.GetComponent<SpriteRenderer>();
        if (spriteProjetil != null && spriteCapanga != null)
        {
            spriteProjetil.sortingLayerID = spriteCapanga.sortingLayerID;
            spriteProjetil.sortingOrder = spriteCapanga.sortingOrder + 1;
        }
    }
}
