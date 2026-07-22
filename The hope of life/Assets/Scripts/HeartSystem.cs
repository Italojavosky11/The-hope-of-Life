using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeartSystem : MonoBehaviour
{
    public float vida;
    public float vidaMaxima;

    public Image[] coracao;
    public Sprite cheio;
    public Sprite meio;
    public Sprite vazio;

    void Start()
    {
        LogicaCoracao();
    }

    public void TomarDano(float dano)
    {
        vida -= dano;

        if (vida < 0)
            vida = 0;

        LogicaCoracao();
        DeadStage();
    }

    void LogicaCoracao()
    {
        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }

        for (int i = 0; i < coracao.Length; i++)
        {
            if (vida >= i + 1)
            {
                coracao[i].sprite = cheio;
            }
            else if (vida >= i + 0.5f)
            {
                coracao[i].sprite = meio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            coracao[i].enabled = (i < vidaMaxima);
        }
    }

    void DeadStage()
    {
        if (vida <= 0)
        {
            GetComponent<Player>().enabled = false;

            Invoke(nameof(ReiniciarFase), 2f);
        }
    }

    void ReiniciarFase()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}