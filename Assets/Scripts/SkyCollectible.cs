using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SkyCollectible : MonoBehaviour
{
    public enum CollectibleKind
    {
        coin = 0,
        gem = 1,
        heart = 2,
        skyCrystal = 1,
        health = 2
    }

    [FormerlySerializedAs("pt")]
    public CollectibleKind collectibleKind;
    [FormerlySerializedAs("PickupEffect")]
    [SerializeField] GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collectibleKind == CollectibleKind.coin)
        {
            if (collision.gameObject.tag == "Player")
            {
                SkyRealmGameManager.instance.IncrementCoinCount();
           
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

                Destroy(this.gameObject,0.2f);
                
            }
            
        }

        if (collectibleKind == CollectibleKind.gem || collectibleKind == CollectibleKind.skyCrystal)
        {
            if (collision.gameObject.tag == "Player")
            {
                SkyRealmGameManager.instance.IncrementGemCount();
                SkyRealmGameManager.instance.PlayRewardSound();
            
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

                Destroy(this.gameObject, 0.2f);

            }

        }
    }
}
