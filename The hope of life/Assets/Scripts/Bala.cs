using UnityEngine;

public class Bala : MonoBehaviour
{
    public float lifetime = 3f;
    public float speed = 20f;

    public int danoBoss = 20;
    public int danoEnemy1 = 50;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy1"))
        {
            Enemy1fase enemy1 = collision.GetComponent<Enemy1fase>();
            if (enemy1 != null)
            {
                enemy1.TomarDano(danoEnemy1);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Boss"))
        {
            Boss boss = collision.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TomarDano(danoBoss);
            }
            Destroy(gameObject);
        }
    }
}