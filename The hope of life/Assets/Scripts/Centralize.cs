using UnityEngine;

public class Centralize : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position += (transform.parent.position - transform.position) * 5 * Time.deltaTime;
    }
}
