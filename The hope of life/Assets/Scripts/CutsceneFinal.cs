
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneFinal : MonoBehaviour
{
    [Header("Cutscene")]
    public GameObject painelCutscene;
    public TMP_Text textoCutscene;
    public string[] paragrafos;
    public float velocidadeDigitacao = 0.04f;

    [Header("Creditos")]
    public GameObject painelCreditos;
    public RectTransform conteudoCreditos;
    public RectTransform viewportCreditos;
    public float velocidadeCreditos = 40f;
    public float tempoCreditos = 45f;

    [Header("Finalizacao")]
    public string nomeCenaMenu = "MenuPrincipal";

    private int indiceParagrafo;
    private bool digitando;
    private bool exibindoCreditos;

    void Start()
    {
        painelCutscene.SetActive(true);
        painelCreditos.SetActive(false);

        indiceParagrafo = 0;

        MostrarParagrafo();
    }

    void Update()
    {
        if (exibindoCreditos)
        {
            AtualizarCreditos();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (digitando)
            {
                StopAllCoroutines();
                textoCutscene.text = paragrafos[indiceParagrafo];
                digitando = false;
            }
            else
            {
                ProximoParagrafo();
            }
        }
    }

    void MostrarParagrafo()
    {
        if (indiceParagrafo >= paragrafos.Length)
        {
            IniciarCreditos();
            return;
        }

        textoCutscene.text = "";
        StartCoroutine(DigitarTexto());
    }

    IEnumerator DigitarTexto()
    {
        digitando = true;

        string texto = paragrafos[indiceParagrafo];

        foreach (char letra in texto)
        {
            textoCutscene.text += letra;
            yield return new WaitForSeconds(velocidadeDigitacao);
        }

        digitando = false;
    }

    void ProximoParagrafo()
    {
        indiceParagrafo++;
        MostrarParagrafo();
    }

    void IniciarCreditos()
    {
        painelCutscene.SetActive(false);
        painelCreditos.SetActive(true);

        exibindoCreditos = true;

        Canvas.ForceUpdateCanvases();

        conteudoCreditos.anchoredPosition =
            new Vector2(0, -viewportCreditos.rect.height);

        StartCoroutine(FinalizarCreditos());
    }

    void AtualizarCreditos()
    {
        Vector2 posicao = conteudoCreditos.anchoredPosition;

        posicao.y += velocidadeCreditos * Time.deltaTime;

        conteudoCreditos.anchoredPosition = posicao;
    }

    IEnumerator FinalizarCreditos()
    {
        yield return new WaitForSeconds(tempoCreditos);

        SceneManager.LoadScene(nomeCenaMenu);
    }
}
