using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFasesManager : MonoBehaviour
{
    private const string FaseMaximaDesbloqueadaKey = "FaseMaximaDesbloqueada";

    private void Awake()
    {
        foreach (Button botao in GetComponentsInChildren<Button>(true))
        {
            int fase = botao.gameObject.name switch
            {
                "ButtonFase1" => 1,
                "ButtonFase2" => 2,
                "ButtonFase3" => 3,
                _ => 0
            };
            if (fase == 0) continue;
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => AbrirFase(fase));
            botao.interactable = fase <= ObterFaseMaximaDesbloqueada();
        }
    }

    private void AbrirFase(int fase)
    {
        if (fase <= ObterFaseMaximaDesbloqueada())
        {
            string nomeCena = fase == 1 ? "Cutscene" : "Level" + fase;
            if (Application.CanStreamedLevelBeLoaded(nomeCena))
                SceneManager.LoadScene(nomeCena);
            else
                Debug.LogWarning("A cena " + nomeCena + " não está disponível nas configurações de build.");
        }
    }

    public static void RegistrarConclusaoDaFase(int faseConcluida)
    {
        if (faseConcluida < 1 || faseConcluida >= 3) return;
        int proximaFase = faseConcluida + 1;
        if (proximaFase > ObterFaseMaximaDesbloqueada())
        {
            PlayerPrefs.SetInt(FaseMaximaDesbloqueadaKey, proximaFase);
            PlayerPrefs.Save();
        }
    }

    private static int ObterFaseMaximaDesbloqueada()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(FaseMaximaDesbloqueadaKey, 1), 1, 3);
    }
}
