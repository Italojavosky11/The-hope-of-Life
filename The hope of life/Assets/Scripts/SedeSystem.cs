using UnityEngine;
using UnityEngine.UI;

public class SedeSystem : MonoBehaviour
{
    
    public float sede;
    public float sedeMaxima = 5f;

    
    public Image garrafa;
    public Sprite cheia;
    public Sprite tresQuartos;
    public Sprite metade;
    public Sprite umQuarto;
    public Sprite vazia;

    
    public float tempoParaPerderSede = 40f;
    private float contador;

    
    public HeartSystem heartSystem;
    public float danoSede = 0.5f;
    public float tempoDano = 1f;
    private float contadorDano;

    void Start()
    {
        sede = sedeMaxima;

        contador = tempoParaPerderSede;
        contadorDano = tempoDano;

        AtualizarGarrafa();
    }

    void Update()
    {
        contador -= Time.deltaTime;

        if (contador <= 0f)
        {
            PerderSede(1f);
            contador = tempoParaPerderSede;
        }

        if (sede <= 0)
        {
            contadorDano -= Time.deltaTime;

            if (contadorDano <= 0)
            {
                heartSystem.TomarDano(danoSede);
                contadorDano = tempoDano;
            }
        }
        else
        {
            contadorDano = tempoDano;
        }
    }

    void AtualizarGarrafa()
    {
        sede = Mathf.Clamp(sede, 0, sedeMaxima);

        if (sede >= 5)
        {
            garrafa.sprite = cheia;
        }
        else if (sede >= 4)
        {
            garrafa.sprite = tresQuartos;
        }
        else if (sede >= 3)
        {
            garrafa.sprite = metade;
        }
        else if (sede >= 1)
        {
            garrafa.sprite = umQuarto;
        }
        else
        {
            garrafa.sprite = vazia;
        }
    }

    public void PerderSede(float quantidade)
    {
        sede -= quantidade;
        sede = Mathf.Clamp(sede, 0, sedeMaxima);

        AtualizarGarrafa();
    }

    public void RecuperarSede(float quantidade)
    {
        sede += quantidade;
        sede = Mathf.Clamp(sede, 0, sedeMaxima);

        AtualizarGarrafa();
    }
}