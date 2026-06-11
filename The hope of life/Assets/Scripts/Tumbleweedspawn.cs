using UnityEngine;

public class Tumbleweedspawn : MonoBehaviour
{
    public GameObject arbusto;
    public Transform Spawnpoint;

    private bool spawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spawned) return;

        if (other.CompareTag("Player"))
        {
            spawned = true;

            GameObject tumbleweed = Instantiate(
                arbusto,
                Spawnpoint.position,
                Quaternion.identity
            );

            Tweblede script = tumbleweed.GetComponent<Tweblede>();

            if (script != null)
            {
                script.SetTarget(other.transform);
            }
        }
    }
}