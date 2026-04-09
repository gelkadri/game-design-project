using UnityEngine;
using UnityEngine.UI;

public class SkyHeartManager : MonoBehaviour
{
    public static SkyHeartManager instance;
    public GameObject damageEffect;

    private int maxHealth = 6;
    public int currentHealth;

    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite FullHeartSprite;
    [SerializeField] private Sprite HalfHeartSprite;
    [SerializeField] private Sprite EmptyHeartSprite;

    private GameObject explorer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        explorer = GameObject.FindObjectOfType<SkyExplorerController>().gameObject;
        currentHealth = maxHealth;
        DisplayHearts();
    }
   
  

    public void HurtPlayer()
    {

        if (currentHealth > 0)
        {
            currentHealth--;
            DisplayHearts();
            // explorer.GetComponent<SkyExplorerController>().Knockback();
        }
        else if (currentHealth <= 0)
        {
            SkyRealmGameManager.instance.Death();
        }
        
        Instantiate(damageEffect, explorer.transform.position, Quaternion.identity);
    }

    public void DisplayHearts()
    {
        int fullHeartsCount = currentHealth / 2; // Calculate the number of full hearts
        bool hasHalfHeart = (currentHealth % 2) == 1; // Check if there's a half heart needed

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < fullHeartsCount)
            {
                // Heart should be full
                hearts[i].sprite = FullHeartSprite;
            }
            else if (hasHalfHeart && i == fullHeartsCount)
            {
                // Heart should be half
                hearts[i].sprite = HalfHeartSprite;
            }
            else
            {
                // Heart should be empty
                hearts[i].sprite = EmptyHeartSprite;
            }
        }
    }

    

}
