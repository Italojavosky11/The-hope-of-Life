using UnityEngine;

public class BushDamage : MonoBehaviour
{
    [SerializeField] private float dano = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HeartSystem heart = other.GetComponent<HeartSystem>();
            Player player = other.GetComponent<Player>();

            if (heart != null)
            {
                heart.TomarDano(dano);
            }

            if (player != null)
            {
                player.kBCount = player.kBTime;

                if (other.transform.position.x <= transform.position.x)
                {
                    player.isKnockRight = true;
                }
                else
                {
                    player.isKnockRight = false;
                }
            }
        }
    }
}