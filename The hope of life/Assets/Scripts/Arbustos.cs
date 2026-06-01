using UnityEngine;

public class Arbusto : MonoBehaviour
{
    public float velocidade = 2f;
    public Transform pontoA;
    public Transform pontoB;

    private Vector3 alvo;

    void Start()
    {
        alvo = pontoB.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            alvo,
            velocidade * Time.deltaTime
        );

        if(Vector3.Distance(transform.position, alvo) < 0.1f)
        {
            alvo = alvo == pontoA.position ? pontoB.position : pontoA.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // causar dano
        }
    }
}