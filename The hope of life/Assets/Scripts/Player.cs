using UnityEngine;

public class Player : MonoBehaviour
{
    
    
    public float speed = 5f;
    public int vidaMax = 100;
    public int vidaAtual;
    
    private Rigidbody2D rb;
    #region moviment
    void Start()
    {
        vidaAtual = vidaMax;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveX, moveY) * speed;
        rb.linearVelocity = movement;
    }
    #endregion
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                vidaAtual -= enemy.dano;
            }

            if (vidaAtual <= 0)
            {
                vidaAtual = 0;
                gameObject.SetActive(false);
            }
        }
    }
}
