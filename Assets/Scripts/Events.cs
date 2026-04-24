using UnityEngine;
using UnityEngine.SceneManagement;

public class Events : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPanel;

    public void OpenInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
    public void Level()
    {
        SceneManager.LoadScene(1);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
