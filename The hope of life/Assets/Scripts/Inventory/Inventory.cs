
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public Item[] item;
    public GameObject mouseItem;

    private GameObject armaEquipada;
    private Transform jogador;
    private Animator animatorJogador;

    private void Awake()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            jogador = playerObject.transform;
            animatorJogador = playerObject.GetComponent<Animator>();
        }
    }

    private void Start()
    {
        EquiparArmaDoPrimeiroSlot();
    }

    public void DragItem(GameObject button)
    {
        mouseItem = button;
        mouseItem.transform.position = Input.mousePosition;
    }

    public void DropItem(GameObject button)
    {
        GameObject itemArrastado =
            mouseItem != null ? mouseItem : button;

        if (itemArrastado == null)
            return;

        Transform slotDestino = EncontrarSlotSobOMouse(itemArrastado);
        Transform slotOrigem = itemArrastado.transform.parent;

        if (slotDestino != null && slotOrigem != null)
        {
            if (slotOrigem != slotDestino)
            {
                if (EhPrimeiroSlot(slotDestino) &&
                    !ItemPodeSerEquipado(itemArrastado))
                {
                    itemArrastado.transform.SetParent(slotOrigem, false);
                    itemArrastado.transform.localPosition = Vector3.zero;
                    mouseItem = null;
                    return;
                }

                Transform itemNoDestino = null;

                for (int i = 0; i < slotDestino.childCount; i++)
                {
                    Transform filho = slotDestino.GetChild(i);

                    if (filho != itemArrastado.transform)
                    {
                        itemNoDestino = filho;
                        break;
                    }
                }

                if (itemNoDestino != null &&
                    EhPrimeiroSlot(slotDestino) &&
                    !ItemPodeSerEquipado(itemNoDestino.gameObject))
                {
                    itemArrastado.transform.SetParent(slotOrigem, false);
                    itemArrastado.transform.localPosition = Vector3.zero;
                    mouseItem = null;
                    return;
                }

                itemArrastado.transform.SetParent(slotDestino, false);
                itemArrastado.transform.localPosition = Vector3.zero;

                if (itemNoDestino != null)
                {
                    itemNoDestino.SetParent(slotOrigem, false);
                    itemNoDestino.localPosition = Vector3.zero;
                }

                if (EhPrimeiroSlot(slotOrigem) ||
                    EhPrimeiroSlot(slotDestino))
                {
                    EquiparArmaDoPrimeiroSlot();
                }
            }
        }

        mouseItem = null;
    }

    private Transform EncontrarSlotSobOMouse(GameObject itemArrastado)
    {
        if (EventSystem.current == null)
            return null;

        var dadosPonteiro = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(dadosPonteiro, resultados);

        foreach (RaycastResult resultado in resultados)
        {
            Transform atual = resultado.gameObject.transform;

            if (atual == itemArrastado.transform ||
                atual.IsChildOf(itemArrastado.transform))
                continue;

            while (atual.parent != null && atual.parent != transform)
                atual = atual.parent;

            if (atual.parent == transform &&
                atual.name.StartsWith("Slot"))
                return atual;
        }

        return null;
    }

    private bool EhPrimeiroSlot(Transform slot)
    {
        return slot != null &&
               slot.parent == transform &&
               slot.name == "Slot1";
    }

    private DataArma ObterDataArma(GameObject itemUI)
    {
        if (itemUI == null ||
            !int.TryParse(itemUI.name, out int indice) ||
            item == null ||
            indice < 0 ||
            indice >= item.Length)
            return null;

        return item[indice].dataArma;
    }

    private bool ItemPodeSerEquipado(GameObject itemUI)
    {
        DataArma dataArma = ObterDataArma(itemUI);

        return dataArma != null &&
               GerenciadorArmas.EstaDesbloqueada(dataArma);
    }

    private void EquiparArmaDoPrimeiroSlot()
    {
        if (jogador == null)
            return;

        Transform primeiroSlot = transform.Find("Slot1");

        if (primeiroSlot == null || primeiroSlot.childCount == 0)
        {
            RemoverArmaEquipada();
            AtualizarArmaAtual(0);
            return;
        }

        string nomeItem = primeiroSlot.GetChild(0).name;

        if (!int.TryParse(nomeItem, out int indice) ||
            item == null ||
            indice < 0 ||
            indice >= item.Length)
        {
            RemoverArmaEquipada();
            AtualizarArmaAtual(0);
            return;
        }

        DataArma dataArma = item[indice].dataArma;

        if (dataArma == null ||
            dataArma.prefabArma == null ||
            !GerenciadorArmas.EstaDesbloqueada(dataArma))
        {
            RemoverArmaEquipada();
            AtualizarArmaAtual(0);
            return;
        }

        Arma armaAtual = armaEquipada != null
            ? armaEquipada.GetComponent<Arma>()
            : null;

        if (armaAtual != null && armaAtual.dataArma == dataArma)
        {
            AtualizarArmaAtual(indice + 1);
            return;
        }

        RemoverArmaEquipada();

        armaEquipada = Instantiate(dataArma.prefabArma, jogador);

        armaEquipada.transform.localPosition =
            dataArma.prefabArma.transform.localPosition;

        armaEquipada.transform.localRotation =
            dataArma.prefabArma.transform.localRotation;

        armaEquipada.transform.localScale =
            dataArma.prefabArma.transform.localScale;

        AtualizarArmaAtual(indice + 1);
    }

    private void AtualizarArmaAtual(int valor)
    {
        if (animatorJogador != null)
            animatorJogador.SetInteger("ArmaAtual", valor);
    }

    private void RemoverArmaEquipada()
    {
        if (armaEquipada != null)
        {
            Destroy(armaEquipada);
            armaEquipada = null;
        }
    }
}
