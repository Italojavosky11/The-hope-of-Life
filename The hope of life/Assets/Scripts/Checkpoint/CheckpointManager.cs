using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Vector3 checkpointPosition;
    private bool possuiCheckpoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    }

    public void Respawn(GameObject player)
    {
        if (!possuiCheckpoint)
        {
            return;
        }

        player.transform.position = checkpointPosition;

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