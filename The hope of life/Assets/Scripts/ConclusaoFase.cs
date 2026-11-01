using UnityEngine;
using UnityEngine.SceneManagement;

public class ConclusaoFase : MonoBehaviour
{
    [SerializeField, Range(1, 3)] private int faseConcluida = 1;
    private bool concluida;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (concluida || !other.CompareTag("Player")) return;
        concluida = true;
        MenuFasesManager.RegistrarConclusaoDaFase(faseConcluida);
        string proximaCena = "Level" + (faseConcluida + 1);
        if (faseConcluida < 3 && Application.CanStreamedLevelBeLoaded(proximaCena))
            SceneManager.LoadScene(proximaCena);
        else
            SceneManager.LoadScene("MenuFases");
    }
}
