using UnityEngine;



public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    
    Vector2 movement;

    void Update()
    {
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Flip();
    }

    void FixedUpdate()
    {
        
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Flip() {
        if(movement.x > 0) {transform.eulerAngles = new Vector2(0, 0);}
        else if(movement.x < 0) {transform.eulerAngles = new Vector2(0, 180);}
    }
}