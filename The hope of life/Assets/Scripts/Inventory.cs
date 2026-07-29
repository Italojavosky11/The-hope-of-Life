using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryPanel;
    private bool aberto = false;
    public Player player;
    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            aberto = !aberto;

            inventoryPanel.SetActive(aberto);

            player.podeMover = !aberto;
        }
    }
}
