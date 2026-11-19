using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Vector3 checkpointPosition;
    private Vector3 inicioFase;
    private bool possuiCheckpoint = false;
    private int mortesDesdeCheckpoint;

    public bool PossuiCheckpoint => possuiCheckpoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                inicioFase = player.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DefinirCheckpoint(Vector3 posicao)
    {
        checkpointPosition = posicao;
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
}
