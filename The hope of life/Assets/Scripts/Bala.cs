using UnityEngine;

public class Bala : MonoBehaviour
{
    public float speed;
    public float damage;
    public float lifeTime;

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
        // Depois você adicionará o dano no inimigo aqui.

        Destroy(gameObject);
    }
}