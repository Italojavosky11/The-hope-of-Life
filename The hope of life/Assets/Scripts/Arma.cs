using UnityEngine;

public class Arma : MonoBehaviour
{
    public DataArma dataArma;

    public Transform firePoint;

    private float nextFireTime;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Atirar();
        }
    }

    void Atirar()
    {
        if (dataArma == null)
            return;

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + dataArma.fireRate;

        GameObject bala = Instantiate(
            dataArma.prefabBala,
            firePoint.position,
            firePoint.rotation
        );

        Bala balaScript = bala.GetComponent<Bala>();

        if (balaScript != null)
        {
            balaScript.Configurar(dataArma);
        }
    }
}