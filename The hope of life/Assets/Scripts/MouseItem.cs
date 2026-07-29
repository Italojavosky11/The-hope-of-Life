
using UnityEngine;

public class MouseItem : MonoBehaviour
{
    public static MouseItem instancia;

    public Item itemSelecionado;

    void Awake()
    {
        instancia = this;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void PegarItem(Item item)
    {
        itemSelecionado = item;

        item.transform.SetParent(transform);

        item.transform.localPosition = Vector3.zero;
    }
}