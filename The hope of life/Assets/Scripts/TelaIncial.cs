using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaInicial : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("TelaDeFases");
    }
}