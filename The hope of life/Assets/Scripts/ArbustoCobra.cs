using UnityEngine;

/// <summary>
/// Coloque este componente no arbusto. Quando o jogador entra no raio definido,
/// uma cobra e criada na posicao do arbusto.
/// </summary>
public class ArbustoCobra : MonoBehaviour
{
    [Header("Cobra")]
    [SerializeField] private Cobra cobraPrefab;
    [SerializeField] private Transform pontoDeSpawn;

    [Header("Ativacao")]
    [SerializeField, Min(0.1f)] private float distanciaDeAtivacao = 2f;
    [SerializeField, Min(0f)] private float tempoParaNovoSpawn = 3f;

    private Transform jogador;
    private float proximoSpawnPermitido;
    private Cobra cobraAtual;

    private void Update()
    {
        EncontrarJogador();

        if (jogador == null || cobraAtual != null || Time.time < proximoSpawnPermitido)
            return;

        if (Vector2.Distance(transform.position, jogador.position) <= distanciaDeAtivacao)
            SpawnarCobra();
    }

    private void EncontrarJogador()
    {
        if (jogador != null)
            return;

        GameObject objetoJogador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJogador != null)
            jogador = objetoJogador.transform;
    }

    private void SpawnarCobra()
    {
        if (cobraPrefab == null)
        {
            Debug.LogWarning("ArbustoCobra precisa de um prefab de Cobra.", this);
            return;
        }

        Transform origem = pontoDeSpawn != null ? pontoDeSpawn : transform;
        cobraAtual = Instantiate(cobraPrefab, origem.position, Quaternion.identity);
        cobraAtual.Iniciar(jogador, origem.position, CobraRetornou);
    }

    private void CobraRetornou()
    {
        cobraAtual = null;
        proximoSpawnPermitido = Time.time + tempoParaNovoSpawn;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeAtivacao);
    }
}
