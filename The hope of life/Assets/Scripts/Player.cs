using UnityEngine;



public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    public float kBForce;
    public float kBCount;
    public float kBTime;

    public bool isKnockRight;

    
    Vector2 movement;

    void Update()
    {
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Flip();
        
    }

    void FixedUpdate()
    {
        if (kBCount <= 0)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        KnockLogic();
    }

    void KnockLogic()
    {
        if (kBCount <= 0)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (isKnockRight)
            {
                rb.linearVelocity = new Vector2(kBForce, kBForce);
            }
            else
            {
                rb.linearVelocity = new Vector2(-kBForce, kBForce);
            }

            kBCount -= Time.deltaTime;
        }
    }

    void Flip() {
        if(movement.x > 0) {transform.eulerAngles = new Vector2(0, 0);}
        else if(movement.x < 0) {transform.eulerAngles = new Vector2(0, 180);}
    }
}