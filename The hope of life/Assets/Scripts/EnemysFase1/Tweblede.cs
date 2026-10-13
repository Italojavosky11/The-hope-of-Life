using UnityEngine;

public class Tweblede : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 5f;

    private Vector2 startPosition;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public void SetTarget(Transform player)
    {
        Vector2 direction =
            ((Vector2)player.position - (Vector2)transform.position)
            .normalized;

        rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        if (Vector2.Distance(startPosition, transform.position) >= distance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) { Destroy(gameObject); }
        
    }
}