using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class landingScreen : MonoBehaviour
{
    public Button startBtn;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = startBtn.GetComponent<Button>();
        btn.onClick.AddListener(startGame);
        // Text btnText = btn.GetComponentInChildren<Text>();
        // btnText.text = "Start Game!";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startGame()
    {
      SceneManager.LoadScene("SampleScene");
    }
}
