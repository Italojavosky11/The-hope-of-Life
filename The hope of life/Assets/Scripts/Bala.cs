using UnityEngine;

public class Bala : MonoBehaviour
{
    public float speed;
    public float damage;
    public float lifeTime;

    public void Configurar(DataArma dataArma)
    {
        speed = dataArma.bulletSpeed;
        damage = dataArma.damage;
        lifeTime = dataArma.bulletLifeTime;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = dataArma.spriteBala;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Depois adicionaremos o dano no inimigo aqui.

        Destroy(gameObject);
    }
}