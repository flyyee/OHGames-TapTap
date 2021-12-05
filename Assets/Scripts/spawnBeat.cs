using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class spawnBeat : MonoBehaviour
{
    bool game_started = false;
    bool game_ready = false; // prevents game from going straight to end screen after countdown

    AudioSource audioSource;
    bool m_Play;
    bool m_ToggleChange;

    float[] timings;
    public int current_timing_idx;
    public float time_elapsed;
    public List<List<GameObject>> beats;
    public float beats_boundary;

    public GameObject beatPrefab;
    public GameObject tailPrefab;

    float next_spawn_time;

    // const float beat_spawn_x1 = (float)-3.9;
    // const float beat_spawn_x2 = (float)-1.3;
    // const float beat_spawn_x3 = (float)+1.3;
    // const float beat_spawn_x4 = (float)+3.9;
    static readonly float[] beat_spawn_xlist = {-3.9f, -1.3f, 1.3f, 3.9f};
    const float beat_spawn_y = 4;
    const float beat_base_speed = 4;

    public Color col_white = new Color(1, 1, 1);
    public Color col_dblue = new Color(49, 77, 121);
    static readonly string[] input_asdf = {"a", "s", "d", "f"};
    static readonly string[] input_hjkl = {"h", "j", "k", "l"};
    public GameObject[] longBeats = {null, null, null, null};
    // int long_beat_count = 0;
    const float long_beat_prob = 0.1f;

    public float curr_score = 0;
    public int curr_streak = 0;
    const int x2_streak = 20;
    const int x3_streak = 50;
    public int curr_combo = 1;
    const float bar_height = 6.8f;
    public Text DispScore;
    public Text DispStreak;
    public Text Perfect;
    public Text CountdownText;
    public bool counting_down = false;
    public bool perfect_on = false;
    public float perfect_timer = 0;
    const float perfect_maxtime = 3;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        m_Play = false;
        m_ToggleChange = true;

        timings = new float[0];
        current_timing_idx = 0;
        beats = new List<List<GameObject>>();
        for (int i = 0; i < 4; i++)
        {
          beats.Add(new List<GameObject>());
        }

        GameObject spot = GameObject.Find("spot1");
        beats_boundary = spot.transform.position.y;

        DispScore.text = "Game Score: 0";
        Perfect.color = new Color(Perfect.color.r, Perfect.color.g, Perfect.color.b, 0);
        CountdownText.text = "";
        //Perfect.enabled = false;
        //next_spawn_time = 0;
    }

    private void spawn(int lane, bool hasTail = false, float tailLength = 0f)
    {
        GameObject b = Instantiate(beatPrefab) as GameObject;
        beat beat_script = b.GetComponent<beat>();
        beat_script.setSpeed(beat_base_speed);
        Vector3 pos;
        pos.x = (float)beat_spawn_xlist[lane];
        pos.y = beat_spawn_y;
        pos.z = 0;
        b.transform.position = pos;

        if (hasTail)
        {
          GameObject t = Instantiate(tailPrefab) as GameObject;
          beat_script.setTail(t, tailLength);
          // longBeats[lane] = t;
          // long_beat_count++;
        }

        beats[lane].Add(b);
    }

    public void dispCombo(int streak)
    {
      float streak_height = bar_height/50f * streak;
      GameObject comboBar = GameObject.Find("ComboBarFiller");

      Vector3 new_pos = comboBar.transform.position;
      new_pos.y = streak_height/2 - 3.4f;
      comboBar.transform.position = new_pos;

      Vector3 new_scale = comboBar.transform.localScale;
      new_scale.y = streak_height;
      comboBar.transform.localScale = new_scale;

    }

    // Update is called once per frame
    void Update()
    {
        if (!counting_down) StartCoroutine(StartingCountdown());

        if (game_ready && !game_started)
        {
            game_started = true;
            m_ToggleChange = true;
            m_Play = !m_Play;
            timings = new float[500];
            for (int x = 0; x < 500; x++)
            {
                timings[x] = ((x + 6) * (float)(60.0 / 130.0)) + time_elapsed - (Math.Abs(beat_spawn_y - beats_boundary) / beat_base_speed);
            }
            //next_spawn_time = Time.time + (60 / 130);
        }

        //Check to see if you just set the toggle to positive
        if (m_Play == true && m_ToggleChange == true)
        {
            //Play the audio you attach to the AudioSource component
            audioSource.Play();
            //Ensure audio doesn�t play more than once
            m_ToggleChange = false;
        }
        // TODO: add pause
        //Check if you just set the toggle to false
        if (m_Play == false && m_ToggleChange == true)
        {
            //Stop the audio
            audioSource.Stop();
            //Ensure audio doesn�t play more than once
            m_ToggleChange = false;
        }

        // Check the combo
        curr_combo = 1 + Convert.ToInt32(curr_streak >= x2_streak) + Convert.ToInt32(curr_streak >= x3_streak);

        // Handle beats for each lane individually
        for (int i = 0; i < 4; i++)
        {
          GameObject spot = GameObject.Find("spot" + (i+1).ToString());
          spot spot_script = spot.GetComponent<spot>();
          bool keydown = Input.GetKey(input_asdf[i]) || Input.GetKey(input_hjkl[i]);
          // spot.GetComponent<Renderer>().material.color = keydown? col_white : col_dblue;
          spot_script.spotTapFill.transform.localScale = keydown? new Vector3(1,1,1) : new Vector3(0,0,0);
          while (beats[i].Count > 0)
          {
              // Check for keypresses for spots
              // 1st spot – a/h; 2nd spot – s/j; 3rd spot – d/k; 4th spot – f/l
              if (Input.GetKeyDown(input_asdf[i]) || Input.GetKeyDown(input_hjkl[i]))
              {
                spot_script.tapResponse();
              }

              // Handle long beats
              if (longBeats[i])
              {
                tail tail_script = longBeats[i].GetComponent<tail>();
                if (keydown)
                {
                  curr_score += curr_combo;
                } else
                {
                  // TODO: some long beats not deleted
                  // currently seems like this part of the code runs late for some of the long beats
                  // but it still runs eventually

                  // print("Destroying long beat");
                  tail_script.fading = false;
                  Destroy(longBeats[i]);
                  longBeats[i] = null;
                }
              }

              if (beats[i].Count == 0) break;

              // beat sc = beats[0].GetComponent<beat>();
              // sc.setSpeed(40);
              if (beats[i][0].transform.position.y < beats_boundary)
              {
                  beat beat_script = beats[i][0].GetComponent<beat>();
                  if (beat_script.hasTail)
                  {
                    Destroy(beat_script.getTail());
                  }
                  // Destroy(beats[i][0]); // TODO: replace with beats falling animation
                  StartCoroutine(FallingBeats(beats[i][0]));
                  beats[i].RemoveAt(0);
                  curr_streak = 0;
              } else
              {
                  break;
              }
          }
        }

        // Keep track of game score, streak & combo bar
        DispScore.text = "Game Score: " + curr_score.ToString("0.00");
        DispStreak.text = "Streak: " + curr_streak.ToString();
        dispCombo(Mathf.Min(curr_streak, x3_streak));

        // Display perfect
        if (perfect_on)
        {
          perfect_timer += Time.deltaTime;
          if (perfect_timer > perfect_maxtime)
          {
            StartCoroutine(FadeTextToZeroAlpha(1f, Perfect));
            perfect_on = false;
            perfect_timer = 0;
          }
        }

        // Beat timing
        time_elapsed += Time.deltaTime; // TODO: is this efficient?
        if (current_timing_idx < timings.Length)
        {
            if (time_elapsed > timings[current_timing_idx])
            {
                current_timing_idx++;
                int next_lane = UnityEngine.Random.Range(0,4);

                spawn(
                  next_lane,
                  Mathf.Floor(UnityEngine.Random.Range(0, 1/long_beat_prob)) == 0,
                  UnityEngine.Random.Range(2,5)
                );
            }
        }

        // another beat timing system
        //if (Time.time > next_spawn_time && next_spawn_time != 0.0)
        //{
        //    print(Time.time);
        //    print(next_spawn_time);
        //    next_spawn_time = Time.time + (float)(60.0 / 130.0);
        //    spawn((float)-3.9);
        //}

        // Check for end of game
        if (game_started && !audioSource.isPlaying)
        {
          PlayerPrefs.SetFloat("GameScore", curr_score);
          SceneManager.LoadScene("EndingScreen");
        }
    }

    public IEnumerator StartingCountdown(float lag = 1f)
    {
      counting_down = true;
      for (int i = 3; i > 0; i--)
      {
        CountdownText.text = i.ToString();
        yield return new WaitForSeconds(lag);
      }
      CountdownText.text = "";
      game_ready = true;
    }

    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator FallingBeats(GameObject b, float lag = 0.05f)
    {
      int i = 0;
      while (b.transform.localScale.x > 0)
      {
        b.transform.localScale = new Vector3(b.transform.localScale.x - i*0.1f, b.transform.localScale.y - i*0.1f, b.transform.localScale.z);
        i++;
        yield return new WaitForSeconds(lag);
      }
      Destroy(b);
    }
}
