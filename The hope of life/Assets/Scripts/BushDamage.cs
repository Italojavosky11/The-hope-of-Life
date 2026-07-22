using UnityEngine;

public class BushDamage : MonoBehaviour
{
    [SerializeField] private float dano = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HeartSystem heart = other.GetComponent<HeartSystem>();

            if (heart != null)
            {
                heart.TomarDano(dano);
            }
        }
    }
}
