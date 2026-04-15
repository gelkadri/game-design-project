using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyHeartManager : MonoBehaviour
{
    public static SkyHeartManager instance;
    public GameObject damageEffect;

    private int maxHealth = 6;
    private int startingHealth = 6;
    private static int savedHealth = -1;
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
        SkyExplorerController ctrl = GameObject.FindObjectOfType<SkyExplorerController>();
        if (ctrl != null)
            explorer = ctrl.gameObject;

        if (hearts == null || hearts.Length == 0)
            FindHeartImages();

        currentHealth = savedHealth > 0 ? savedHealth : startingHealth;
        DisplayHearts();
    }

    private void FindHeartImages()
    {
        string[] heartNames = { "Heart", "Heart (1)", "Heart (2)" };
        List<Image> found = new List<Image>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
        foreach (string hName in heartNames)
        {
            foreach (Image img in allImages)
            {
                if (img.gameObject.name == hName)
                {
                    found.Add(img);
                    if (FullHeartSprite == null)
                        FullHeartSprite = img.sprite;
                    break;
                }
            }
        }

        found.Sort((a, b) =>
            a.rectTransform.anchoredPosition.x.CompareTo(b.rectTransform.anchoredPosition.x));

        hearts = found.ToArray();
    }
   
  

    public void LoseHeart()
    {
        currentHealth -= 2;
        if (currentHealth < 0)
            currentHealth = 0;
        savedHealth = currentHealth;
        DisplayHearts();
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public static void ResetHealth()
    {
        savedHealth = -1;
    }

    public void HealPlayer()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += 2;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
            DisplayHearts();
        }
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
        if (hearts == null || hearts.Length == 0)
            return;

        int fullHeartsCount = currentHealth / 2;
        bool hasHalfHeart = (currentHealth % 2) == 1;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            if (i < fullHeartsCount)
            {
                if (FullHeartSprite != null) hearts[i].sprite = FullHeartSprite;
                hearts[i].color = Color.white;
            }
            else if (hasHalfHeart && i == fullHeartsCount)
            {
                if (HalfHeartSprite != null)
                    hearts[i].sprite = HalfHeartSprite;
                hearts[i].color = new Color(1f, 1f, 1f, 0.4f);
            }
            else
            {
                if (EmptyHeartSprite != null)
                    hearts[i].sprite = EmptyHeartSprite;
                else
                    hearts[i].color = new Color(1f, 1f, 1f, 0.15f);
            }
        }
    }

    

}
