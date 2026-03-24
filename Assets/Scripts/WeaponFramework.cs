using UnityEngine;

public class WeaponFramework : Weapon
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy" && gameObject.tag == "Player")
        {
            (Enemy)other.heath -= damage;
        }
    }
}
