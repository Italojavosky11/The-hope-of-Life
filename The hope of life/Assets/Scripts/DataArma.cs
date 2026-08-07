using UnityEngine;

[CreateAssetMenu(fileName = "Nova Arma", menuName = "Game/Weapon Data")]
public class DataArma : ScriptableObject
{
    
    public string weaponName;
    public Sprite icon;

    
    public GameObject prefabArma;
    public GameObject prefabBala;

    
    public float damage = 10f;
    public float bulletSpeed = 15f;
    public float fireRate = 0.4f;
    public float bulletLifeTime = 5f;
}