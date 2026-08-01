using UnityEngine;

public class inventoryManager : MonoBehaviour
{
    
    public GameObject inventarioCanvas;
    public KeyCode teclaInventario = KeyCode.E;

    private bool inventarioAberto = false;

    void Start()
    {
        inventarioCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaInventario))
        {
            inventarioAberto = !inventarioAberto;
            inventarioCanvas.SetActive(inventarioAberto);

            // Mostra ou esconde o mouse
            Cursor.visible = inventarioAberto;

            // Libera ou trava o mouse
            Cursor.lockState = inventarioAberto ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }        


}
