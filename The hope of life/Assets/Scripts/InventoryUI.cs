
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject SistemaDeInventario;

    private bool aberto;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            aberto = !aberto;
            SistemaDeInventario.SetActive(aberto);

            Time.timeScale = aberto ? 0f : 1f;
        }
    }
}
