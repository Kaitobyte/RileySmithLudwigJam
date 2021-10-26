using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button newGame;
    [SerializeField] private Button loadGame;
    [SerializeField] private Button about;
    [SerializeField] private Button quit;
    [SerializeField] private GameObject aboutScreen;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("hasSaved"))
        {
            loadGame.interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void newGameButton()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("MainLevel", LoadSceneMode.Single);
    }

    public void loadGameButton()
    {
        SceneManager.LoadScene("MainLevel", LoadSceneMode.Single);
    }

    public void aboutButton()
    {
        aboutScreen.SetActive(true);
    }

    public void quitButton()
    {
        Application.Quit();
    }

    public void goBackButton()
    {
        aboutScreen.SetActive(false);
    }

}
