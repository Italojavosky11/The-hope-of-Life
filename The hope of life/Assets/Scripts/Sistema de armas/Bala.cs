using UnityEngine;

public class Bala : MonoBehaviour
{
    public float speed;
    public float damage;
    public float lifeTime;

    private Vector2 direcao;
    private bool balaDoInimigo;
    private bool impactoResolvido;

    public void Configurar(DataArma dataArma)
    {
        speed = dataArma.bulletSpeed;
        damage = dataArma.damage;
        lifeTime = dataArma.bulletLifeTime;

        direcao = transform.right;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = dataArma.spriteBala;

        Destroy(gameObject, lifeTime);
    }

    public void ConfigurarParaInimigo(
        Sprite spriteBala,
        float danoJogador,
        float velocidade,
        float tempoDeVida)
    {
        speed = velocidade;
        damage = danoJogador;
        lifeTime = tempoDeVida;
        balaDoInimigo = true;
        direcao = transform.right;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = spriteBala;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direcao * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (balaDoInimigo)
        {
            if (impactoResolvido)
                return;

            if (other.GetComponentInParent<InimigoGangueFase1>() != null)
                return;

            HeartSystem coracoes = other.GetComponentInParent<HeartSystem>();
            if (coracoes != null)
            {
                impactoResolvido = true;
                coracoes.TomarDano(damage);
                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Bala") ||
                other.gameObject.layer == LayerMask.NameToLayer("Default"))
                return;

            Destroy(gameObject);
            return;
        }
        // Prioriza inimigos (cobra)
        Cobra cobra = other.GetComponentInParent<Cobra>();
        if (cobra != null)
        {
            cobra.ReceberDano(damage);
            Destroy(gameObject);
            return;
        }

        InimigoGangueFase1 gangue = other.GetComponentInParent<InimigoGangueFase1>();
        if (gangue != null)
        {
            gangue.ReceberDano(damage);
            Destroy(gameObject);
            return;
        }

        InimigoGangue2 gangue2 = other.GetComponentInParent<InimigoGangue2>();
        if (gangue2 != null)
        {
            gangue2.ReceberDano(damage);
            Destroy(gameObject);
            return;
        }

        // Barril de �gua
        BarrilDeAgua barril = other.GetComponentInParent<BarrilDeAgua>();
        if (barril != null)
        {
            barril.DestruirBarril();
            Destroy(gameObject);
            return;
        }

        // Ignora jogador, outras balas e a layer Default
        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Bala"))
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
            return;

        // Qualquer outro colisor destr�i a bala
        Destroy(gameObject);
    }
}