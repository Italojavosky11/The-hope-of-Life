using UnityEngine;

public class inventoryManager : MonoBehaviour
{
    
    public GameObject inventarioCanvas;
    public KeyCode teclaInventario = KeyCode.E;

    private bool inventarioAberto = false;

    void Start()
    {
        inventarioCanvas.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaInventario))
        {
            inventarioAberto = !inventarioAberto;
            inventarioCanvas.SetActive(inventarioAberto);

            // Mantém o cursor visível
            Cursor.visible = true;

            // Mantém o cursor livre para continuar usando o mouse
            Cursor.lockState = CursorLockMode.None;
        }
    }        


}
