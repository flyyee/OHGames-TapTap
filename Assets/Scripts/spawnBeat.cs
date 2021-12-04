using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spawnBeat : MonoBehaviour
{
    bool game_started = false;

    AudioSource audioSource;
    bool m_Play;
    bool m_ToggleChange;

    float[] timings;
    public int current_timing_idx;
    public float time_elapsed;
    public List<GameObject> beats;
    public float beats_boundary;

    public GameObject beatPrefab;

    float next_spawn_time;

    // const float beat_spawn_x1 = (float)-3.9;
    // const float beat_spawn_x2 = (float)-1.3;
    // const float beat_spawn_x3 = (float)+1.3;
    // const float beat_spawn_x4 = (float)+3.9;
    static readonly float[] beat_spawn_xlist = {-3.9f, -1.3f, 1.3f, 3.9f};
    const float beat_spawn_y = 4;
    const float beat_base_speed = 4;

    public float currScore = 0;
    public int currStreak = 0;
    public Text DispScore;
    public Text DispStreak;
    public Text Perfect;
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
        beats = new List<GameObject>();

        GameObject spot = GameObject.Find("spot1");
        beats_boundary = spot.transform.position.y;

        DispScore.text = "Game Score: 0";
        Perfect.color = new Color(Perfect.color.r, Perfect.color.g, Perfect.color.b, 0);
        //Perfect.enabled = false;
        //next_spawn_time = 0;
    }

    private void spawn(float x, float y = beat_spawn_y, float z = 0)
    {
        GameObject b = Instantiate(beatPrefab) as GameObject;
        beat beat_script = b.GetComponent<beat>();
        beat_script.setSpeed(beat_base_speed);
        Vector3 pos;
        pos.x = (float)x;
        pos.y = y;
        pos.z = z;
        b.transform.position = pos;
        beats.Add(b);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !game_started)
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

        while (beats.Count > 0)
        {
            // Check for keypresses for spots
            // 1st spot – a/h; 2nd spot – s/j; 3rd spot – d/k; 4th spot – f/l
            if (Input.GetKeyDown("a") || Input.GetKeyDown("h"))
            {
              GameObject spot = GameObject.Find("spot1");
              spot.GetComponent<spot>().tapResponse();
            }
            if (Input.GetKeyDown("s") || Input.GetKeyDown("j"))
            {
              GameObject spot = GameObject.Find("spot2");
              spot.GetComponent<spot>().tapResponse();
            }
            if (Input.GetKeyDown("d") || Input.GetKeyDown("k"))
            {
              GameObject spot = GameObject.Find("spot3");
              spot.GetComponent<spot>().tapResponse();
            }
            if (Input.GetKeyDown("f") || Input.GetKeyDown("l"))
            {
              GameObject spot = GameObject.Find("spot4");
              spot.GetComponent<spot>().tapResponse();
            }

            //beat sc = beats[0].GetComponent<beat>();
            //sc.setSpeed(40);
            if (beats[0].transform.position.y < beats_boundary)
            {
                Destroy(beats[0]); // TODO: replace with beats falling animation
                beats.RemoveAt(0);
                if (currScore != 0 && currStreak != 0)
                {
                  // Combo: x2 for streak of >20, x3 for streak of >30, x4 for streak of >40, x5 for streak of >50
                  // if score is -ve it'll be divided
                  currScore *= Mathf.Pow(Mathf.Min(currStreak/10+1, 5), currScore/Math.Abs(currScore));
                }
                currStreak = 0;
            } else
            {
                break;
            }
        }

        // Keep track of game score & streak
        DispScore.text = "Game Score: " + currScore.ToString("0.00");
        if (currStreak > 0) {
          DispStreak.text = "Streak: " + currStreak.ToString();
        } else
        {
          DispStreak.text = "Streak: ";
        }

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

        //print(time_elapsed);
        time_elapsed += Time.deltaTime; // TODO: is this efficient?
        if (current_timing_idx < timings.Length)
        {
            if (time_elapsed > timings[current_timing_idx])
            {
                current_timing_idx++;
                // spawn a new beat!
                // spawn((float)beat_spawn_x1);

                // spawn a new beat at a random lane (for testing)
                spawn((float)beat_spawn_xlist[UnityEngine.Random.Range(0,4)]);
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

        if (!audioSource.isPlaying) {
          // next level
        }
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
}
