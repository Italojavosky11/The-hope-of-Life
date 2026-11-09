
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private TMP_Text textoHistoria;
    [SerializeField] private TMP_Text textoContinuar;

    [Header("Efeito de Digitação")]
    [SerializeField] private AudioSource audioSource;

    [Header("Configurações")]
    [SerializeField] private string cenaGameplay = "Fase1";
    [SerializeField] private float velocidadeTexto = 0.04f;

    [Header("Textos da Cutscene")]
    [TextArea(3, 6)]
    [SerializeField] private string[] textos;

    private int indiceAtual;
    private bool escrevendo;
    private bool finalizada;
    private Coroutine rotinaAtual;

    private void Start()
    {
        textoHistoria.text = "";
        textoContinuar.gameObject.SetActive(false);

        rotinaAtual = StartCoroutine(ExibirTexto());
    }
    private void Update()
    {
        if (finalizada)
            return;

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (escrevendo)
            {
                StopCoroutine(rotinaAtual);
                audioSource.Stop();
                textoHistoria.text = textos[indiceAtual];
                escrevendo = false;
            }
            else
            {
                indiceAtual++;

                if (indiceAtual >= textos.Length)
                {
                    FinalizarCutscene();
                    return;
                }

                rotinaAtual = StartCoroutine(ExibirTexto());
            }
        }
    }

    private IEnumerator ExibirTexto()
    {
        escrevendo = true;
        textoContinuar.gameObject.SetActive(false);
        textoHistoria.text = "";

        if (audioSource != null)
            audioSource.Play();

        foreach (char letra in textos[indiceAtual])
        {
            textoHistoria.text += letra;
            yield return new WaitForSeconds(velocidadeTexto);
        }

        if (audioSource != null)
            audioSource.Stop();

        escrevendo = false;
        textoContinuar.gameObject.SetActive(true);
    }

    private void FinalizarCutscene()
    {
        if (finalizada)
            return;

        finalizada = true;
        SceneManager.LoadScene(cenaGameplay);
    }
}
