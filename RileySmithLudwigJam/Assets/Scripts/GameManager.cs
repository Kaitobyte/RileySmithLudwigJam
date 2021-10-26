using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject finishLine;
    [SerializeField] private GameObject textDisplay;
    [SerializeField] private GameObject mms_Manual;
    [SerializeField] private GameObject endOfGame;
    [SerializeField] private GameObject quitMenu;
    [SerializeField] private Text finalTime;

    private bool alreadyFinished = false;
    private float endTimer;
    private string playerTime;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (finishLine.GetComponent<FinishLine>().isFinished() && !alreadyFinished)
        {
            playerTime = player.GetComponent<PlayerController>().getGameTime().ToString("F2");
            textDisplay.GetComponent<Text>().text = playerTime;
            alreadyFinished = true;
        }

        if (alreadyFinished)
        {
            endTimer += Time.deltaTime;
        }

        if (endTimer > 5)
        {
            finalTime.text = playerTime;
            textDisplay.SetActive(false);
            endOfGame.SetActive(true);

            if (Input.GetKeyDown("return"))
            {
                Debug.Log("Loading Scene");
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }

        }

        if (Input.GetKey("tab"))
        {
            mms_Manual.SetActive(true);
        }
        else
        {
            mms_Manual.SetActive(false);
        }

        if(Input.GetKeyDown("escape"))
        {
            quitMenu.SetActive(true);
        }

    }

    public void quitGameButton()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void continueGameButton()
    {
        quitMenu.SetActive(false);
    }

}
