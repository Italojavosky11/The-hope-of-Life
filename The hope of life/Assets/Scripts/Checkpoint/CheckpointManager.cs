using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [SerializeField] private DataArma[] armasIniciaisFase = new DataArma[0];

    private Vector3 checkpointPosition;
    private Vector3 inicioFase;
    private DataArma[] armasInicioFase = new DataArma[0];
    private DataArma[] armasNoCheckpoint = new DataArma[0];
    private bool possuiCheckpoint = false;
    private int mortesDesdeCheckpoint;

    public bool PossuiCheckpoint => possuiCheckpoint;

    private void Awake()
    {
        // Cada cena precisa aplicar suas armas iniciais, mesmo que já exista
        // outra instância do gerenciador no domínio atual.
        GerenciadorArmas.Desbloquear(armasIniciaisFase);

        if (Instance == null)
        {
            Instance = this;
            armasInicioFase = GerenciadorArmas.CapturarDesbloqueadas();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                inicioFase = player.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void DefinirCheckpoint(Vector3 posicao)
    {
        checkpointPosition = posicao;
        armasNoCheckpoint = GerenciadorArmas.CapturarDesbloqueadas();
        possuiCheckpoint = true;
        mortesDesdeCheckpoint = 0;
    }

    public bool RegistrarMorte()
    {
        if (!possuiCheckpoint)
            return true;

        mortesDesdeCheckpoint++;
        return mortesDesdeCheckpoint >= 3;
    }

    public void Respawn(GameObject player)
    {
        if (possuiCheckpoint)
            GerenciadorArmas.RestaurarDesbloqueadas(armasNoCheckpoint);
        else
            GerenciadorArmas.RestaurarDesbloqueadas(armasInicioFase);

        player.transform.position = possuiCheckpoint ? checkpointPosition : inicioFase;

        HeartSystem heartSystem = player.GetComponent<HeartSystem>();
        SedeSystem sedeSystem = FindFirstObjectByType<SedeSystem>();

        if (heartSystem != null)
        {
            heartSystem.RestaurarVida(heartSystem.vidaMaxima);
        }

        if (sedeSystem != null)
        {
            sedeSystem.RestaurarSede();
        }
    }

    public void RestaurarDesbloqueiosIniciaisDaFase()
    {
        GerenciadorArmas.RestaurarDesbloqueadas(armasInicioFase);
    }
}
