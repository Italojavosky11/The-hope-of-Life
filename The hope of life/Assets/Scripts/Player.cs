using UnityEngine;

public class Player : MonoBehaviour
{
   public GameObject balaPrefab;
   public Transform FirePoint;
   public float speed =  5f;
   public int vidaMax = 100;

   public int vidaAtual;

    void Start()
    {
        vidaAtual = vidaMax;
    }
   void Update()
   {

         if (Input.GetKeyDown(KeyCode.Z))
         {
              Atirar();
         }
       float moveHorizontal = Input.GetAxis("Horizontal");
       float moveVertical = Input.GetAxis("Vertical");

       Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);
       transform.Translate(movement * speed * Time.deltaTime);
   }
   

   public void sistemaDeVida(int dano)
   {
       vidaAtual -= dano;
       if (vidaAtual <= 0)
       {
           Morrer();
       }
   }

   public void Morrer(){ Destroy(gameObject); }


   void Atirar()
    {
        Instantiate(balaPrefab, FirePoint.position, FirePoint.rotation);
    }
}
