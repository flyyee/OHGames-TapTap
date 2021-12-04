using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class endingScreen : MonoBehaviour
{
    public Button PlayAgain;
    public Button MainMenu;
    public Text DispScore;

    // Start is called before the first frame update
    void Start()
    {
      Button restartBtn = PlayAgain.GetComponent<Button>();
      restartBtn.onClick.AddListener(restartGame);
      Button exitBtn = MainMenu.GetComponent<Button>();
      exitBtn.onClick.AddListener(exitGame);
      DispScore.text = PlayerPrefs.GetFloat("GameScore").ToString("0.00"); 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void restartGame()
    {
      SceneManager.LoadScene("SampleScene");
    }

    public void exitGame()
    {
      SceneManager.LoadScene("LandingScreen");
    }
}
