using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class landingScreen : MonoBehaviour
{
    public Button startBtn;
<<<<<<< HEAD
=======
    public Dropdown songchoices;

    void Awake()
	{
        // for testing only
        PlayerPrefs.SetInt("InfinityUnlocked", 1);
        if (PlayerPrefs.GetInt("InfinityUnlocked") == 1)
        {
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = "Einsteins (Infinity)";
            songchoices.options.Add(od);
            Dropdown.OptionData od2 = new Dropdown.OptionData();
            od2.text = "Dynamite (Infinity)";
            songchoices.options.Add(od2);
        }
    }

>>>>>>> 4ce35071c1dfb40ac0a12c082e0a65884e2739f6
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
<<<<<<< HEAD
=======
        GameObject canvas = GameObject.Find("Dropdown");
        Dropdown dd = canvas.GetComponent<Dropdown>();
        string choice = dd.options[dd.value].text;
        if (choice.StartsWith("Little"))
		{
            PlayerPrefs.SetInt("songchoice", 1); // einstein
        } else if (choice.StartsWith("Dynamite"))
		{
            if (choice.Contains("Infinity"))
			{
                PlayerPrefs.SetInt("songchoice", 4); // dynamite infinity
            } else
			{
                PlayerPrefs.SetInt("songchoice", 2); // dynamite
            }
        } else
		{
            PlayerPrefs.SetInt("songchoice", 3); // einstein infinity
        }
>>>>>>> 4ce35071c1dfb40ac0a12c082e0a65884e2739f6
      SceneManager.LoadScene("SampleScene");
    }
}
