using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class TransicaoFases : MonoBehaviour
{
    private const float DuracaoTelaCarregamento = 2f;
    private static bool transicaoEmAndamento;
    private GameObject telaCarregamento;

    public static void Iniciar(string cenaDestino)
    {
        if (transicaoEmAndamento)
            return;

        transicaoEmAndamento = true;
        var objetoTransicao = new GameObject("TransicaoFases");
        DontDestroyOnLoad(objetoTransicao);
        TransicaoFases transicao = objetoTransicao.AddComponent<TransicaoFases>();
        transicao.StartCoroutine(transicao.CarregarCena(cenaDestino));
    }

    private IEnumerator CarregarCena(string cenaDestino)
    {
        CriarTelaCarregamento();
        yield return new WaitForSecondsRealtime(DuracaoTelaCarregamento);
        SceneManager.LoadScene(cenaDestino);
        yield return null;
        Destroy(telaCarregamento);
        Destroy(gameObject);
        transicaoEmAndamento = false;
    }

    private void CriarTelaCarregamento()
    {
        telaCarregamento = new GameObject("TelaCarregamento");
        telaCarregamento.transform.SetParent(transform, false);

        Canvas canvas = telaCarregamento.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        telaCarregamento.AddComponent<CanvasScaler>();
        telaCarregamento.AddComponent<GraphicRaycaster>();

        GameObject fundo = new GameObject("FundoPreto");
        fundo.transform.SetParent(telaCarregamento.transform, false);
        RectTransform fundoRect = fundo.AddComponent<RectTransform>();
        fundoRect.anchorMin = Vector2.zero;
        fundoRect.anchorMax = Vector2.one;
        fundoRect.offsetMin = Vector2.zero;
        fundoRect.offsetMax = Vector2.zero;
        fundo.AddComponent<CanvasRenderer>();
        fundo.AddComponent<Image>().color = Color.black;

        GameObject texto = new GameObject("TextoCarregando");
        texto.transform.SetParent(telaCarregamento.transform, false);
        RectTransform textoRect = texto.AddComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;
        texto.AddComponent<CanvasRenderer>();
        TextMeshProUGUI componenteTexto = texto.AddComponent<TextMeshProUGUI>();
        componenteTexto.text = "Carregando...";
        componenteTexto.alignment = TextAlignmentOptions.Center;
        componenteTexto.color = Color.white;
        componenteTexto.font = Resources.Load<TMP_FontAsset>("Minecraft SDF");
        componenteTexto.fontSize = 32;
    }
}
