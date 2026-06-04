using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item[] item;

    public  GameObject mouseItem;

    public void DragItem ( GameObject Button)
    {
        mouseItem = Button;
        mouseItem.transform.position = Input.mousePosition;
    }


    public void Dropitem ( GameObject Button)
    {
        if (mouseItem != null)
        {

            Transform aux = mouseItem.transform.parent;
            mouseItem.transform.SetParent(Button.transform.parent);
            Button.transform.SetParent(aux);

            
        }
        
    }

}
