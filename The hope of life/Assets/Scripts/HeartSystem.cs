using UnityEngine;
using UnityEngine.UI;

public class HeartSystem : MonoBehaviour
{
    public float vida;
    public float vidaMaxima;

    public Image[] coracao;
    public Sprite cheio;
    public Sprite meio;
    public Sprite vazio;

    void Update()
    {
        LogicaCoracao();
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
}
