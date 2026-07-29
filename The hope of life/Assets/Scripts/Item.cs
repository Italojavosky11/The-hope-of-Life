using UnityEngine;

public class Item : MonoBehaviour
{
    public Slot slotAtual;

    public void Clicar()
    {
        MouseItem.instancia.PegarItem(this);
    }
}