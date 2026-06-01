using UnityEngine;

public class SistemadeAgua : MonoBehaviour
{
    #region variaveis
    public int agua = 20;
    public int aguaAtual;

    private float intervalodetempo = 10f;
    private float timer;

    public Player player;
    #endregion

    #region start e update
    public void Start()
    {
        aguaAtual = agua;

        
        if (player == null)
        {
            player = GetComponent<Player>();
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= intervalodetempo)
        {
            timer = 0f;

            if (aguaAtual > 0)
            {
                aguaAtual--; // agora perde 1 corretamente
            }
            else
            {
                player.sistemaDeVida(10); 
            }
        }

        
        if (aguaAtual < 0)
        {
            aguaAtual = 0;
        }
    }
    #endregion

    #region coletaragua
    public void coletaragua()
    {
        aguaAtual += 4; // cada gota = 4 

        if (aguaAtual > agua)
        {
            aguaAtual = agua;
        }
    }
    #endregion
}