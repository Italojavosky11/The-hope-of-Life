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

    public Player player;

    private bool morreu = false;

    void Start()
    {
        LogicaCoracao();
    }

    public void TomarDano(float dano)
    {
        if (morreu)
            return;

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
        if (vida <= 0 && !morreu)
        {
            morreu = true;

            player.enabled = false;

            Invoke(nameof(Respawn), 2f);
        }
    }

    void Respawn()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.Respawn(gameObject);
        }

        vida = Mathf.Clamp(vida, 0, vidaMaxima);

        morreu = false;

        player.enabled = true;

        LogicaCoracao();
    }

    public void RestaurarVida(float vidaSalva)
    {
        vida = Mathf.Clamp(vidaSalva, 0f, vidaMaxima);
        LogicaCoracao();
    }
}