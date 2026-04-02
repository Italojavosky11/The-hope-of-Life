using UnityEngine;

public class Player : MonoBehaviour
{
    #region moviment
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
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
}
