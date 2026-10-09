using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool ativado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (ativado)
        {
            return;
        }

        CheckpointManager.Instance.DefinirCheckpoint(transform.position);

        ativado = true;
    }
}