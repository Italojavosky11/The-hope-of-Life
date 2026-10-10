
using UnityEngine;

public class Bala : MonoBehaviour
{
    public float speed;
    public float damage;
    public float lifeTime;

    private Vector2 direcao;

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

    void Update()
    {
        transform.position +=
            (Vector3)(direcao * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BarrilDeAgua barril =
            other.GetComponentInParent<BarrilDeAgua>();

        if (barril != null)
            barril.DestruirBarril();

        Destroy(gameObject);
    }
}