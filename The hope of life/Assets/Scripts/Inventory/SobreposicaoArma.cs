
using UnityEngine;

public class SobreposicaoArma : MonoBehaviour
{
    public DataArma arma;
    public GameObject bloqueio;

    private void Start()
    {
        AtualizarBloqueio();
    }

    private void Update()
    {
        AtualizarBloqueio();
    }

    private void AtualizarBloqueio()
    {
        if (arma == null || bloqueio == null)
            return;

        bool desbloqueada = GerenciadorArmas.EstaDesbloqueada(arma);

        bloqueio.SetActive(!desbloqueada);
    }
}
